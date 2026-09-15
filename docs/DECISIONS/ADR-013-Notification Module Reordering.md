# ADR-013 — Notification Module Reordering

* **Status:** Accepted
* **Date:** 2026-09-16
* **Amends:** ADR-010 (development order, steps 5-8)

## Context

ADR-010 placed the Notification module at step 8, after Domain Modules (5),
CQRS/MediatR (6), and Events/RabbitMQ/Outbox (7) — i.e. well after Identity.

Identity's authentication flows, however, already need real notification
delivery to be functionally complete:

* **Email verification** — a user must receive an email with a verification
  link/token after registering.
* **Password reset** — a user must receive an email with a reset link/token
  when they request one.

Until now, `ForgotPasswordCommandHandler` used a placeholder
`LoggingPasswordResetTokenNotifier` that only logs that a reset was
requested, never sending an actual email. This was an explicit, documented
stand-in (see the Identity module closure work) precisely because the
Notification module and its Outbox/RabbitMQ transport did not exist yet at
that point in the original order. `RegisterUser` has no email verification
step at all yet, for the same reason.

Leaving Notification at step 8 means Identity — a module already built and
"closed" per its own task — ships with a security-relevant flow
(password reset) that does not actually work end-to-end, and a second flow
(email verification) that does not exist at all. This is no longer
acceptable: these are not optional nice-to-haves, they are part of
Identity's own definition of done as an authentication module.

## Decision

The Notification module, together with the Outbox/RabbitMQ infrastructure
it depends on, is moved forward in the development order to immediately
after Identity/Authorization, before Domain Modules.

Updated order:

```text
1. Backend Architecture
2. Backend Foundation
3. Database-per-Module
4. Identity / Authorization
5. Events / RabbitMQ / Outbox
6. Notification
7. Domain Modules
8. CQRS / MediatR
9. API Contracts
10. Backend Tests
11. Backend Completion
12. React Frontend
```

CQRS/MediatR remains listed separately because it is a project-wide
pattern re-affirmed/extended as each subsequent module is built, not
because it is postponed — Identity already uses it. Domain Modules
(Candidate, Employer, Job, ...) still come after Notification, since none
of them currently have a hard runtime dependency on notification delivery
the way Identity's auth flows do.

This does not change AGENTS.md §4 (Backend-First Rule) or the overall
Backend-before-Frontend priority from ADR-010 — only the relative order of
two steps within the backend phase.

## Why

* Identity's email verification and password reset flows become real,
  working features instead of shipping with a logging placeholder.
* The Outbox/RabbitMQ infrastructure this requires is general-purpose
  (AGENTS.md §29, ADR-006) and every later module that needs asynchronous
  cross-module communication benefits from it existing sooner rather than
  later.
* Avoids a second module (Domain Modules) being built on top of an
  Identity module that still has an unfinished flow underneath it.

## Consequences

### Positive

* Password reset and email verification work end-to-end against a real
  message broker and a real email provider, not a stub.
* Outbox/RabbitMQ plumbing (transactional publish, consumer hosting,
  idempotent processing) is established and testable before more modules
  start depending on it.

### Negative

* Slightly more upfront infrastructure work before starting the next
  Domain Module (Candidate), since RabbitMQ + an SMTP provider must be
  configured for the two Identity flows to be genuinely verifiable
  end-to-end (see the ADR documenting the specific Outbox/messaging
  library choice, written alongside the Notification module skeleton).
* The Notification module ships with a narrow initial scope (transactional
  auth emails only); the broader notification catalog described in
  DOMAIN.md §30 (job status changes, interview scheduling, etc.) is still
  built out later, once the corresponding Domain Modules exist.

## Alternatives Considered

### Keep Notification at step 8, ship Identity with the logging placeholder indefinitely

Rejected — a password reset flow that never sends an email is not a
finished feature, and leaving it as permanent placeholder contradicts
AGENTS.md §55 (Definition of Done).

### Build a one-off, Identity-only email sender instead of the Notification module

Rejected — this would duplicate work once the Notification module is
built anyway (per ARCHITECTURE.md §8.10) and would violate AGENTS.md §9 by
having Identity own a cross-cutting notification concern that other
modules will also need (Job, Interview, Employment, etc. per DOMAIN.md
§30).
