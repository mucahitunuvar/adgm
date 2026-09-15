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

Each producing module configures CAP against its **own** DbContext via
`x.UseEntityFramework<TModuleDbContext>()`, which creates CAP's outbox/inbox
tables (`cap.Published` / `cap.Received`) inside that same module's
database — verified by inspecting the database directly after a run:
`cap.Published`/`cap.Received` land next to the module's own tables, not in
a shared location. This satisfies Database-per-Module (AGENTS.md §9).

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

**Consequence for modules that register messaging**: their DbContext must
use SqlServer in every environment, including Testing — ADR-012's
Sqlite-for-tests switch does not apply to them. The Notification module
(this ADR) and, once Identity starts publishing integration events
(email verification / password reset), `IdentityDbContext` in any test
that exercises that path, use a SQL Server LocalDB database instead of
Sqlite. ADR-012 continues to apply unchanged to every module/test that
never touches the outbox (e.g. Identity's existing
`AuthenticationFlowTests`, which has nothing to do with messaging and is
unaffected).

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
* Outbox/consumer tables live inside each module's own database
  automatically — no manual migration work to add them.
* The RabbitMQ broker dependency is removable for tests (in-memory
  transport) without Docker in this environment.

### Negative

* Adds a dependency the team did not previously have experience with;
  CAP's own conventions (topic naming, its internal table schema) become
  something contributors need to learn.
* Messaging-registered modules cannot use ADR-012's Sqlite-for-tests
  switch — their tests need a real SQL Server/LocalDB instead, because of
  the storage-package constraint documented above. This was not
  anticipated when CAP was chosen and was only discovered by actually
  running it against Sqlite; it is a genuine limitation, not a
  configuration mistake.
* If CAP itself ever moves to a commercial model, the same
  re-evaluation this ADR describes for MassTransit would need to happen
  again. Given its current MIT license, broad adoption, and lack of any
  announced commercial plan, this risk is assessed as low today, but
  should be re-checked before any major version upgrade (AGENTS.md §48).
* Production deployments require an actual RabbitMQ broker
  (Development/Production environments) — this was already a requirement
  of ADR-006 and is not new, but is now concretely exercised.

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
