# ADR-014 — CAP for Outbox Pattern and RabbitMQ Integration

* **Status:** Accepted
* **Date:** 2026-09-16
* **Related:** ADR-006 (RabbitMQ and Outbox Pattern), ADR-013 (Notification Module Reordering)

## Context

ADR-006 mandates RabbitMQ + the Transactional Outbox Pattern for reliable
cross-module event publishing, but did not pick a concrete implementation.
Building this by hand means: an Outbox table per producing module, a
background processor that polls it and publishes to RabbitMQ, a consumer
host on the Notification side, retry/backoff, and consumer-side
idempotency tracking (AGENTS.md §27) — all non-trivial, easy-to-get-wrong
infrastructure.

The project already went through this exact evaluation once for MediatR:
a widely-used library (MediatR v13+) moved to a commercial license, and
the project deliberately stayed on the last free version (v12.x, Apache
2.0). The same question applies to the obvious message-bus abstraction
candidate, MassTransit: v8 is Apache-2.0 but loses support at the end of
2026, and v9 ships under a commercial license (source-available, not
open source). Given the project's explicit "no budget for commercial
licensing, prefer free options now and for future licensing decisions
too" stance, MassTransit is not a viable default.

## Decision

**DotNetCore.CAP** (MIT license, `dotnetcore/CAP` on GitHub, ~14.6M NuGet
downloads, actively maintained — v10.0.2 as of January 2026) is used for
both the Outbox pattern and RabbitMQ integration.

Packages (in `BuildingBlocks.Infrastructure`, shared by every module that
needs to publish or consume integration events):

```text
DotNetCore.CAP                      - core: publish/subscribe API, processing host
DotNetCore.CAP.RabbitMQ             - RabbitMQ transport (production/development)
DotNetCore.CAP.SqlServer            - SqlServer-backed outbox/inbox storage
Savorboard.CAP.InMemoryMessageQueue - in-memory transport (Testing environment only)
```

Configuration is environment-driven for the **transport** only, mirroring
ADR-012's `Database:Provider` pattern:

```text
Development / Production → RabbitMQ transport, real broker required
Testing                  → In-memory transport, no broker required
```

CAP is registered **exactly once**, at the host composition root
(`Program.cs`), against **one** designated module's DbContext via
`x.UseEntityFramework<TDbContext>()` — see the Amendment below for why this
is not "per producing module" as originally written here. That DbContext's
database ends up holding CAP's outbox/inbox tables (`cap.Published` /
`cap.Received`) alongside its own tables — verified by inspecting the
database directly after a run.

**Storage is not environment-switched like the transport is, and this is a
real constraint, not a choice**: `UseEntityFramework<T>()` is defined
*inside* the dialect-specific storage package (`DotNetCore.CAP.SqlServer`
here) and is not dialect-agnostic — it always issues SQL Server syntax
against `TDbContext`'s connection, regardless of what provider `TDbContext`
itself is configured with. This was verified directly: pointing an
EF-integrated `NotificationDbContext` at a Sqlite connection string while
only `DotNetCore.CAP.SqlServer` was referenced still logged
`### CAP started!`, but every background processor then failed
(`Anahtar sözcük desteklenmiyor: 'datasource'` — CAP was handing the Sqlite
connection string to `SqlConnection`). Separately, `DotNetCore.CAP.SqlServer`
and `DotNetCore.CAP.Sqlite` both define `UseEntityFramework<T>()` with the
identical signature, so referencing both in one project is a **compile-time
ambiguity** (CS0121) on top of that runtime problem — there is no way to
support both dialects from one compiled module even if the first issue
did not exist.

**Consequence for the module CAP is anchored to**: its DbContext must use
SqlServer in every environment, including Testing — ADR-012's
Sqlite-for-tests switch does not apply to it. See the Amendment below for
which module that ended up being and why every module's integration tests
now use LocalDB rather than only the anchor's.

Publishing follows CAP's documented transactional pattern:

```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync(capPublisher, autoCommit: true);
// ... business writes via the normal Unit of Work ...
await capPublisher.PublishAsync(topic, integrationEvent);
```

wrapped behind a small `IIntegrationEventPublisher` abstraction
(`SharedKernel.Abstractions`) so Application-layer handlers never touch
`ICapPublisher`, `DbContext`, or CAP types directly (AGENTS.md §7/§18).

Consumers implement `ICapSubscribe` with `[CapSubscribe("topic")]`
methods; CAP handles at-least-once delivery and consumer-side dedup via
its own `cap.received` table, so hand-rolled idempotency tracking is not
needed for messages CAP itself delivers.

## Why

* MIT license, no commercial-tier shadow like MediatR/MassTransit.
* Purpose-built for exactly this problem (Outbox + message queue), so the
  hard parts (transactional publish, retry, consumer dedup) are not
  hand-rolled and re-tested from scratch.
* `Savorboard.CAP.InMemoryMessageQueue` gives integration tests a way to
  exercise the *real* publish → transport → consume → side-effect pipeline
  without a running RabbitMQ broker (storage still needs a real SQL Server/
  LocalDB, per the constraint above — this only removes the broker
  dependency, not the database one).
* EF Core integration means the outbox row and the business data change
  commit in the same database transaction, with no separate polling
  processor to write and maintain.

## Consequences

### Positive

* Significantly less custom infrastructure code than a hand-rolled Outbox
  processor + consumer host.
* Outbox/consumer tables live inside the anchor module's own database
  automatically — no manual migration work to add them (see the Amendment
  below: this turned out to mean *one* module's database, not each one).
* The RabbitMQ broker dependency is removable for tests (in-memory
  transport) without Docker in this environment.

### Negative

* Adds a dependency the team did not previously have experience with;
  CAP's own conventions (topic naming, its internal table schema) become
  something contributors need to learn.
* The module CAP is anchored to (Identity, see the Amendment below)
  cannot use ADR-012's Sqlite-for-tests switch — its tests need a real
  SQL Server/LocalDB instead, because of the storage-package constraint
  documented above. This was not anticipated when CAP was chosen and was
  only discovered by actually running it against Sqlite; it is a genuine
  limitation, not a configuration mistake.
* CAP supports exactly one instance per process (also only discovered by
  running it, not documented up front) - it cannot give each module its
  own isolated outbox database the way this ADR originally assumed. See
  the Amendment below.
* If CAP itself ever moves to a commercial model, the same
  re-evaluation this ADR describes for MassTransit would need to happen
  again. Given its current MIT license, broad adoption, and lack of any
  announced commercial plan, this risk is assessed as low today, but
  should be re-checked before any major version upgrade (AGENTS.md §48).
* Production deployments require an actual RabbitMQ broker
  (Development/Production environments) — this was already a requirement
  of ADR-006 and is not new, but is now concretely exercised.

## Amendment (2026-09-16): CAP is a single instance per process

While wiring Identity as a second `AddCap()` caller (alongside
Notification, added when this ADR was first written), both modules'
databases were inspected after a run. Only Notification's database had
`cap.Published`/`cap.Received`; Identity's had none, even though its
`AddCap(x => x.UseEntityFramework<IdentityDbContext>()...)` call completed
without error and the app logged `### CAP started!`. A second `AddCap`
call does not add a second, independent CAP instance — it replaces the
first one's configuration. This is consistent with CAP's own
multi-tenancy issue tracker, which describes the architecture as unable
to support per-tenant/per-module database isolation without workarounds
(message-header tenant IDs, subscribe filters) that do not apply here.

This invalidates this ADR's original "each producing module configures
CAP against its own DbContext" design — that was never actually possible
with more than one module, and Notification only appeared to work in
isolation because it was the only module calling `AddCap`.

**Corrected design**: `AddMessaging<TDbContext>()` (the thin wrapper
around `AddCap` in `BuildingBlocks.Infrastructure`) is called **exactly
once**, at the host composition root (`Program.cs`), not from inside each
module's own `AddXModule()`. `IdentityDbContext` is the chosen anchor,
since Identity is the system's first (and, for now, only) publisher -
its Register and password-reset flows are what need transactional outbox
delivery. `IIntegrationEventPublisher` is registered as a plain
(non-keyed) scoped service, since there is only ever one CAP instance to
resolve. Notification's own `AddNotificationModule()` no longer calls
`AddMessaging` at all; its `[CapSubscribe]` consumers are still
discovered because CAP scans the whole DI container for `ICapSubscribe`
implementations, regardless of which module registered them.

**Consequence for `IdentityDbContext`**: like Notification's DbContext
before it, it must use SqlServer in every environment, including Testing
— it lost ADR-012's Sqlite-for-tests switch entirely (there is now no
`Database:Provider` branch in `IdentityModuleServiceCollectionExtensions`
at all; it is unconditionally SqlServer). The Identity integration test
factory (`CustomWebApplicationFactory`) now provisions a throwaway,
per-run LocalDB database for `IdentityDatabase` instead of the Sqlite
shared-cache in-memory database it used before, mirroring the pattern
already used for `NotificationDatabase`. Both are dropped in `Dispose()`.
Notification's own DbContext keeps ADR-012's `Database:Provider` switch
(it is no longer CAP-constrained) - the test factory sets both
connections to LocalDB anyway for simplicity of having one database
technology in one shared test host, not because Notification requires it.

**If a second module needs to publish with its own transactional
guarantee in the future**, this single-anchor design will need revisiting
- CAP itself does not support it. Options at that point: accept
non-atomic best-effort publishing for the second publisher (publish after
its own SaveChanges, outside CAP's transaction), give the second
publisher its own separate outbox-processing mechanism entirely
(defeating the point of standardizing on CAP), or replace CAP with
something that supports multiple isolated instances. No such second
publisher exists yet, so this is not resolved further here.

## Alternatives Considered

### MassTransit

Rejected for the same reason MediatR v13+ was rejected: v8 (free) loses
support at the end of 2026, and v9 requires a commercial license. Not
consistent with the project's stated preference for durably free
dependencies.

### Hand-rolled Outbox table + RabbitMQ.Client + custom background processor

Fully free (RabbitMQ.Client is MPL2/Apache-2, maintained by the broker
vendor, not at commercial-license risk) and zero third-party messaging
framework dependency. Rejected for now due to the amount of custom
infrastructure code and testing it would require (polling processor,
retry/backoff, consumer-side idempotency store) relative to the size of
this task; may be revisited if CAP's dependency footprint or behavior
becomes a problem.
