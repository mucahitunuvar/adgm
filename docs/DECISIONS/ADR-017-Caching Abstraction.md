# ADR-017 — Caching Abstraction: ICacheService, User-Scoped Caching, and ReferenceData's Key Strategy

* **Status:** Accepted
* **Date:** 2026-09-17
* **Related:** AGENTS.md §33 (Caching), ARCHITECTURE.md §34 (Caching), ADR-016 (ReferenceData Module
  Scope and Access — the previous, per-module `IMemoryCache` usage this ADR replaces),
  PERFORMANCE.md §11 (Pagination)

## Context

Caching today is entirely ReferenceData-specific and hand-rolled: `ReferenceDataLookupReader`
injects `Microsoft.Extensions.Caching.Memory.IMemoryCache` directly, six admin-managed CRUD command
handlers each also take a raw `IMemoryCache cache` parameter just to call
`LookupCacheInvalidator.Invalidate(cache, type)`, and that invalidator can only remove two fixed keys
(`activeOnly=true`/`false`) — because plain `IMemoryCache` has no prefix/wildcard removal API. A
recent pagination change (moving all 23 lookup GET endpoints onto `PagedResult<T>`) worked around
this by caching the *entire* unpaginated list per `(type, activeOnly)` and paging it in memory,
specifically to avoid needing more than two invalidatable keys per lookup type.

No other module has any caching yet, and there is no shared caching abstraction other modules could
adopt without re-implementing the same `IMemoryCache` wiring, key tracking, and invalidation logic
themselves.

## Decision 1 — `ICacheService` in SharedKernel, `MemoryCacheService` in Infrastructure

A single abstraction, usable by any module:

```csharp
// SharedKernel.Abstractions
public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string key, Func<CancellationToken, Task<T>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
    void Remove(string key);
    void RemoveByPrefix(string prefix);
}
```

`ICacheService` lives in **SharedKernel**, not `BuildingBlocks.Infrastructure` (unlike the recent
`QueryablePagingExtensions.ToPagedResultAsync`, which had to live in Infrastructure specifically
because it takes/returns `IQueryable<T>` and calls EF Core — both forbidden outside a module's own
`Infrastructure` namespace per AGENTS.md §16/§18). `ICacheService`'s signature exposes neither
`IQueryable<T>` nor any EF Core/`IMemoryCache` type — it is a plain, framework-independent
key/factory/TTL contract — so it does not create the same problem. Every layer except Domain (which
has no reason to cache anything — caching is an Application/Infrastructure concern) can depend on it
directly, the same way every layer already depends on `Result`/`PagedResult<T>`.

The implementation, `MemoryCacheService` (`BuildingBlocks.Infrastructure.Caching`), wraps
`Microsoft.Extensions.Caching.Memory.IMemoryCache` and is registered once at the Host composition
root via a new `AddCaching(IConfiguration)` extension (mirroring `AddMessaging<TDbContext>`'s
single-registration pattern). Default TTL is configurable
(`CacheSettings.DefaultTtlMinutes`, appsettings `Caching:DefaultTtlMinutes`, defaulting to 60) —
callers only override it when a specific entry has a different natural lifetime (e.g. SEED reference
data, which does not change outside a migration and can safely use a much longer TTL).

**If distributed caching (Redis) becomes necessary later**, only `MemoryCacheService` changes to a
`RedisCacheService` behind the same `ICacheService` interface, registered in the same one place — no
consumer code changes, exactly the same "swap the Infrastructure implementation" pattern already
used for `IReferenceDataLookupReader` (ADR-016 Decision 2) and `IIntegrationEventPublisher`. This is
not a promise Redis is coming soon; AGENTS.md §33 is explicit that IMemoryCache is the correct
starting point and Redis is not a day-one requirement. The point of the abstraction is only that
*when* that need arises, it is a contained, single-file change.

### RemoveByPrefix over plain IMemoryCache

`IMemoryCache` has no built-in way to enumerate or prefix-match its own keys. `MemoryCacheService`
tracks its own key set (a `ConcurrentDictionary<string, byte>`, updated on every `GetOrCreateAsync`)
so `RemoveByPrefix` can scan and remove matching entries. Every `Set` call registers a
`PostEvictionCallback` so a key is removed from that tracking set the moment `IMemoryCache` evicts it
for *any* reason (natural TTL expiry, capacity eviction, or an explicit `Remove`) — not just when
`ICacheService.Remove` is called directly. Without this, the tracking set would grow unboundedly as
entries expire on their own, which matters in particular for Decision 2's user-scoped keys: every
distinct user who ever populates a user-scoped cache entry leaves a permanent tracking-set entry
otherwise, even long after that entry's TTL has passed.

## Decision 2 — User-Scoped Caching: a New `ICurrentUserContext`, Not Identity's `ICurrentUserService`

The request asks for a user-scoped cache helper that derives its key from the current caller's user
id, worded around reusing "`ICurrentUserService`". That interface
(`GenclikMerkezi.Modules.Identity.Application.Abstractions.ICurrentUserService`) is owned by the
**Identity module** — it lives in Identity's own `Application.Abstractions` namespace and its `Role`
property is typed as `Identity.Domain.UserRole`. Neither SharedKernel nor
`BuildingBlocks.Infrastructure` may depend on a module's Application/Domain types (AGENTS.md §9/§49,
enforced by `ModuleBoundaryTests`) — and since the Identity module itself already depends on both
SharedKernel and BuildingBlocks.Infrastructure (every module does), the reverse dependency would also
be a circular project reference, a build-time impossibility, not just a style violation.

**Decision:** add a new, minimal, SharedKernel-owned interface instead:

```csharp
// SharedKernel.Abstractions
public interface ICurrentUserContext
{
    Guid? UserId { get; }
}
```

implemented by `HttpContextCurrentUserContext` (`BuildingBlocks.Infrastructure.Security`), which
reads the same `ClaimTypes.NameIdentifier` claim off `IHttpContextAccessor.HttpContext.User` that
Identity's own `CurrentUserService` already reads for its `UserId` property. This is deliberately
**not** a shared implementation between the two interfaces — Identity's `ICurrentUserService` is left
completely untouched (existing behavior, existing call sites, unchanged), and
`HttpContextCurrentUserContext` is a independent, ~10-line read of the same standard JWT claim. The
duplication is small and is the accepted cost of not crossing a module boundary; it is not a new
concept invented by this ADR, just the same "read the identity claims off HttpContext" logic that
already exists once, now also available in a form modules other than Identity can depend on.

`IUserScopedCacheService.GetOrCreateForCurrentUserAsync<T>(...)` (`BuildingBlocks.Infrastructure.Caching`)
composes `ICacheService` + `ICurrentUserContext`: it builds the key as `$"user:{userId}:{key}"` and
delegates to `ICacheService.GetOrCreateAsync`. When there is no current user (`UserId` is `null` —
an anonymous/unauthenticated caller), it calls the factory directly without caching at all, rather
than caching under a shared "anonymous" key, so that one anonymous request's cached result can never
be served back to a different anonymous caller.

No feature calls `IUserScopedCacheService` yet — it exists, is registered, and is unit tested, ready
for the first module (Candidate is the likely first: e.g. caching a candidate's own recommended job
list) that needs it.

## Decision 3 — ReferenceData's Cache Key Strategy Changes: Per-Page/Filter Keys, Prefix Invalidation

`ReferenceDataLookupReader.ListAsync`/`ListByParentAsync` previously cached the *entire* unpaginated
lookup list per `(type, activeOnly)` and applied `Skip`/`Take` in memory, specifically to keep
`LookupCacheInvalidator` working with only two fixed, always-known keys per type — the only
invalidation plain `IMemoryCache.Remove` could reliably do.

With `RemoveByPrefix`, that constraint is gone. `ReferenceDataLookupReader` now caches each
`(type, activeOnly, page, pageSize)` — and, for parent-scoped lookups, `(type, parentId, activeOnly,
page, pageSize)` — combination under its own key (`ReferenceDataCacheKeys.List`/`ListByParent`), and
every admin-managed CRUD handler invalidates all of them for a type in one call:
`cacheService.RemoveByPrefix($"referencedata:list:{type}:")`. This is both simpler (the DB query and
its cached result are no longer split into two different shapes — "cache the whole thing, page
separately") and more correct: `ListByParentAsync` results (District-by-province, TaxOffice-by-
province) are now actually cached too, which they were not before.

SEED types (`Country`, `Province`, `District`, `Language`) use a 24-hour TTL — this data changes only
via a migration, never at runtime. ADMIN-MANAGED types use the service's configured default TTL
(`ttl: null`), relying on CRUD-triggered `RemoveByPrefix` invalidation to keep them fresh rather than
a short TTL doing that job on a timer.

## Consequences

### Positive

* Any future module's list/lookup endpoint gets the same `GetOrCreateAsync`/`Remove`/`RemoveByPrefix`
  contract without re-deriving `IMemoryCache` key-tracking and prefix-invalidation logic from
  scratch (ReferenceData had to build that from nothing; the next module does not).
* User-scoped caching is available, registered, and tested before any feature needs it — adding the
  first real caller is a single `IUserScopedCacheService` injection, not new infrastructure.
* Swapping to Redis later touches exactly one registration and one implementation file.

### Negative

* A second, narrower "who is the current user" abstraction now exists alongside Identity's
  `ICurrentUserService` (Decision 2). A future consolidation (e.g. Identity's `CurrentUserService`
  implementing both interfaces) is possible but out of scope here — revisit only if the duplication
  becomes a real maintenance cost, not preemptively.
* `MemoryCacheService`'s own key-tracking `ConcurrentDictionary` is additional in-process state
  alongside `IMemoryCache`'s own storage — a deliberate, small, and now eviction-cleaned trade-off
  for `RemoveByPrefix`, not a full second cache.

## Alternatives Considered

### Reuse Identity's `ICurrentUserService` directly, accept the module coupling

Rejected — not just a style preference: SharedKernel/BuildingBlocks.Infrastructure depending on
Identity while Identity depends on both is a circular project reference, which does not compile.

### `GetOrCreateForCurrentUserAsync` as a static extension method on `ICacheService` instead of a dedicated `IUserScopedCacheService`

Considered (the request explicitly allowed either). Rejected in favor of a dedicated, injectable
service: a static extension method would need `ICurrentUserContext` passed in at every call site
(extension methods cannot carry injected dependencies of their own), which is more boilerplate per
caller than injecting one `IUserScopedCacheService` and calling one method. A dedicated service also
matches the request's "arayüz ve implementasyon tam çalışır ve test edilmiş olsun" framing more
naturally than a static helper would.

### Keep ReferenceData's "cache full list, page in memory" approach and only add `ICacheService` for future modules

Rejected — `RemoveByPrefix` directly removes the reason that approach existed in the first place, and
the request explicitly asks for paginated cache keys to include their paging parameters (so different
page/filter combinations do not overwrite each other), which the old approach could not do at the
per-request level (it only ever cached one page's worth of pre-materialized data: everything).
