# ADR-015 — Admin Audit Log

* **Status:** Accepted
* **Date:** 2026-09-16
* **Related:** SECURITY.md §8 (Admin Yetkileri), SECURITY.md §22 (Audit Logging),
  ARCHITECTURE.md §41 (Audit)

## Context

SECURITY.md §8 requires that admin operations — role changes, account
activation/deactivation among them — be audited. ARCHITECTURE.md §41
already flags that Identity's admin actions (`ChangeUserRole`,
`ManuallyUnlock`, `Deactivate`, `Reactivate`) currently produce only a
structured `ILogger` entry (who/what/target), not a durable, queryable
audit trail, and names a dedicated ADR as the way to close that gap.

A structured log line satisfies AGENTS.md §38 (structured logging) but not
SECURITY.md §22, which expects audit records to be "düşünülmelidir ayrı"
(kept conceptually separate from normal application logs) and queryable by
who/what/when/target — logs are typically rotated, not indexed by
`TargetUserId`, and are not a place an admin/compliance reviewer can query
"show me everything done to this account."

## Decision

Add a durable `AdminAuditLog` table, owned by the Identity module's own
schema (no cross-module access — AGENTS.md §9), populated via **domain
event handlers** rather than inline in each command handler:

* `AdminUserRoleChangedAuditLogHandler` reacts to `UserRoleChangedDomainEvent`
* `AdminUserManuallyUnlockedAuditLogHandler` reacts to `UserManuallyUnlockedDomainEvent`
* `AdminUserDeactivatedAuditLogHandler` reacts to `UserDeactivatedDomainEvent`
* `AdminUserReactivatedAuditLogHandler` reacts to `UserReactivatedDomainEvent`

Each handler resolves the acting admin from `ICurrentUserService` (the
domain events themselves stay free of that concept — a domain event
records a fact about the `User` aggregate, not who is allowed to see it;
threading "who called this" into `User`'s domain methods would put an
Application-layer concept, current-identity, inside the Domain, which
AGENTS.md §7 forbids) and writes one `AdminAuditLogEntry` row per event.
This keeps the write out of the five admin command handlers themselves —
they stay focused on the state change — and out of any controller/endpoint.

A new `GET /api/v1/auth/admin/audit-log` endpoint (Admin-only, paginated,
filterable by `TargetUserId` / `ActionType` / date range) exposes the log.

## Why

* **Domain event handlers, not inline writes**: the same mechanism the
  codebase already has (`DomainEventDispatcher`, unused until now) for
  "something else needs to react to a state change without the state
  change itself needing to know about it." Adding a fifth admin action in
  the future means adding one more handler, not touching four unrelated
  command handlers each time.
* **Own schema, not a cross-cutting table**: consistent with
  database-per-module (ADR-003) — Identity owns the record of what
  happened to Identity's own aggregates. A future module with its own
  admin actions (e.g. Employer approval) gets its own audit table in its
  own schema, not a row in Identity's.
* **`ICurrentUserService` in the handler, not the domain event**: keeps
  Domain Events reusable and independent of "who is asking" — a
  requirement Domain already has to satisfy (AGENTS.md §7, §43).

## Consequences

### Positive

* SECURITY.md §8/§22's audit requirement is now actually met for these four
  admin actions, not just logged.
* The pattern generalizes: any future domain event that needs a
  side-effect which is not itself a business rule (audit, notification,
  cache invalidation) can react the same way, without editing the
  aggregate or its command handlers.

### Negative

* The audit write happens in a **separate `SaveChangesAsync` call** from
  the business state change, because `IdentityDbContext.SaveChangesAsync`
  dispatches domain events only *after* the primary commit succeeds (so
  that a failed dispatch cannot roll back a state change that already
  happened). This means the audit write is not atomic with the action it
  records — a failure between the two (e.g. a transient DB error on the
  second call) leaves the action applied but unaudited. Given the low
  volume of admin actions and that this is an audit trail rather than a
  ledger of record with legal/financial consequences, this is an accepted
  trade-off rather than something addressed with the full transactional
  Outbox machinery (ADR-006), which exists for cross-module and
  external-side-effect delivery, not same-database follow-up writes.
* One more table in Identity's schema and migration to maintain.

## Alternatives Considered

### Write the audit entry inline in each admin command handler

Rejected — this is exactly what the task description (and AGENTS.md §51,
no scattering of a cross-cutting concern across every call site) asks to
avoid; every future admin action would need to remember to add the same
three lines.

### A cross-module/shared audit table owned by a new "Audit" module

Rejected for now — over-engineering for four admin actions in one module.
Revisit if/when a second module (e.g. Employer approval) needs the same
capability and duplicating a per-module audit table stops being the
simpler option.

### MediatR pipeline behavior instead of domain event handlers

A `IPipelineBehavior<TRequest, TResponse>` around the four admin commands
was considered, but it would need to know about all four command types
explicitly (or infer "this is an audited admin action" some other way),
whereas reacting to the domain events the commands already raise needs no
new coupling between the audit feature and the commands at all.
