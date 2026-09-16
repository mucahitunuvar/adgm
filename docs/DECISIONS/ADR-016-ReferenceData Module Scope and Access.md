# ADR-016 — ReferenceData Module: Scope, Seed/Admin-Managed Split, and Cross-Module Access

* **Status:** Accepted — Decision 2 (Option C: in-process Contracts interface) and Decision 3
  (generic commands/queries over `DbSet<TLookup>()`) approved as recommended, 2026-09-16.
* **Date:** 2026-09-16
* **Amends:** ADR-008 (ReferenceData as Separate Module) — ADR-008 decided the module should
  exist and named example lookups, but explicitly left "module communication ve cache stratejisi"
  as unresolved future work. This ADR resolves that, plus the seed/admin-managed split ADR-008
  never addressed.
* **Related:** ADR-003 (Database Per Module), ADR-009 (No Generic Repository),
  ARCHITECTURE.md §12-14 (Cross-Module Data/References/ReferenceData Access), §62 (Future
  Microservice Extraction)

## Context

Candidate's CV form (`docs/db/Candidate.md`) and Employer's job-posting/personnel-request forms
(`docs/db/Employer.md`) both need a large set of `Select`-driven lookup values: country, province
("İl" — Candidate.md and Employer.md use "Şehir"/"İl" interchangeably for the same concept),
district, sector, position, department, work location type, employment type, education level,
gender, military status, driver's license type, language + language level, experience level,
nationality, disability category, diploma grading system, reference type, currency, skill, school
category, and tax office (scoped to a province). Neither Candidate nor Employer exists yet as a
real module (both are still empty `.gitkeep` skeletons per ADR-008/current repo state) — this work
front-loads the lookup infrastructure both will depend on.

These lookups split into two genuinely different lifecycles:

* **SEED** — data that does not change through the application's UI at all: `Country`,
  `Province` (81 Turkish provinces), `District` (province-scoped), `Language`. These are closed,
  well-known, externally-defined lists (ISO country codes, TÜİK province/district codes, BCP-47
  languages). Letting an admin edit "Turkey"'s name or delete a province is not a feature anyone
  needs and is an easy way to silently corrupt every downstream form that references it by ID.
* **ADMIN-MANAGED** — `Sector`, `Position`, `Department`, `WorkLocationType`, `EmploymentType`,
  `EducationLevel`, `Gender`, `MilitaryStatus`, `DriversLicenseType`, `LanguageLevel`,
  `ExperienceLevel`, `Nationality`, `DisabilityCategory`, `DiplomaGradingSystem`, `ReferenceType`,
  `Currency`, `Skill`, `SchoolCategory`, `TaxOffice`. These ship with sensible starting data (a
  migration seed) but the business will genuinely need to add/retire values over time (a new
  `Sector`, a renamed `Position`) without a code deploy.

## Decision 1 — Module Scope and the Seed/Admin-Managed Split

ReferenceData owns all of the above, each as its own EF entity/table in ReferenceData's own
database (`GenclikMerkezi.ReferenceData`, `ConnectionStrings:ReferenceDataDatabase`) — no shared
table with a discriminator column, since not every lookup shares the same shape (`TaxOffice` needs
a `ProvinceId` foreign key that no other admin-managed lookup has; `District`/`TaxOffice` need a
parent reference that `Country`/`Language` don't).

SEED tables get **no CRUD endpoint of any kind** beyond `GET` (list/detail) — they are populated
exclusively by EF Core migrations and are immutable at runtime. This is enforced by never wiring a
command/mutation endpoint for them, not by a runtime permission check.

ADMIN-MANAGED tables get starting data via migration seed **and** `Admin`-only `POST`/`PUT` plus a
soft-delete (`IsActive = false`, never a real `DELETE`) — because Candidate/Employer records (once
those modules exist) will hold these lookups' IDs as business references (ADR's own Decision 2
below), and a hard delete would silently orphan every record that referenced it.

## Decision 2 — Cross-Module Access Pattern

**This is the one the request asked to have decided explicitly here, not silently in code.**

### Options considered

**A. Real-time network call (HTTP/gRPC) from Candidate/Employer to ReferenceData's API.**
Rejected. This is a Modular Monolith today — every module runs in the *same process*
(`Program.cs` wires `AddIdentityModule`/`AddNotificationModule`/(future)`AddReferenceDataModule`
into one `WebApplication`). A module calling another module over HTTP would mean the process
calling itself over the network (a loopback round-trip) purely to cross an in-process boundary
that a plain method call could cross for free. That trade only starts paying for itself once
modules are actually deployed as separate services (ARCHITECTURE.md §62), which none are yet.

**B. ID + denormalized snapshot, kept in sync via integration events.**
Rejected for this data specifically. The pattern fits data that a consuming module must keep
working with even if the source module is briefly unavailable, or that changes in ways worth
recording historically (e.g., "the Employer's name *at the time* the Job was posted"). Reference
lookups don't need that: they change rarely, and — critically — a snapshot doesn't remove the need
for a live check anyway, because Candidate/Employer still have to validate "is this `SectorId` a
real, currently-active value" at write time (when a CV or job posting is submitted), which requires
asking ReferenceData directly regardless of whether a display-name copy is also kept. Adding
event-driven snapshot sync (a consumer + local cache table per lookup type, per consuming module)
is real, ongoing infrastructure for a problem (staleness, source unavailability) that a rarely-
changing, always-in-process dataset does not actually have.

**C. In-process synchronous call through a published Contracts interface. (Recommended)**
ReferenceData publishes read-only interfaces in `GenclikMerkezi.Contracts` (the same shared
assembly `IntegrationEventTopics` and the integration event records already live in) — e.g.
`IReferenceDataLookupReader` with methods like
`Task<bool> ExistsAndActiveAsync(ReferenceDataLookupType type, Guid id, CancellationToken ct)` and
`Task<IReadOnlyList<LookupItemSummary>> ListAsync(ReferenceDataLookupType type, bool activeOnly, CancellationToken ct)`.
The implementation lives in `ReferenceData.Infrastructure`, is registered once at the Host
composition root (same pattern already used for `IIntegrationEventPublisher`), and is injected
directly into Candidate/Employer's command/query handlers — a plain in-process method call, not a
network request. Candidate/Employer depend only on the `Contracts` interface, never on
`ReferenceDataDbContext` — satisfying ARCHITECTURE.md §14's existing rule exactly as written.
Because this data is small and rarely written, the implementation caches inside ReferenceData
itself (`IMemoryCache`, AGENTS.md §33) and invalidates the relevant lookup type's cache entry on
every admin CRUD write — so every consumer gets fast, always-current reads without needing its own
cache or sync logic. If a module is ever extracted into its own process (§62), only this one
interface's *implementation* changes (in-process DI registration → an HTTP/gRPC client behind the
same interface) — no consumer code changes, because they were never coupled to "in-process" as a
detail in the first place.

### Recommendation

**Option C.** Simplest option that satisfies every existing rule (§9 database isolation, §12/§14
contract-not-entity sharing) without inventing synchronization machinery for data that does not
have a staleness or availability problem to solve. Revisit only if/when a module holding
ReferenceData IDs is actually extracted to a separate deployable — even then, the fix is swapping
one implementation behind the interface, not a redesign.

## Decision 3 — Generic Admin-Managed CRUD, Reconciled with ADR-009

ADR-009 forbids a generic **repository abstraction** (`IRepository<T>`) that re-implements what EF
Core's `DbContext`/`DbSet<T>` already provides, because it hides EF Core behind a second,
redundant abstraction layer for no benefit. It explicitly endorses using `DbContext` directly at
appropriate boundaries.

The 18 admin-managed lookups share an identical shape and identical CRUD semantics (create, soft-
delete via `IsActive`, list with an active/inactive filter, admin-only). Writing 18 near-identical
Command/Query/Handler/Endpoint sets would violate AGENTS.md §51 (no scattering the same logic
everywhere) far more than a shared generic pattern violates ADR-009 — provided that pattern is
built the way ADR-009 itself endorses:

* A shared abstract `LookupItem` **domain base class** (in `ReferenceData.Domain`, not
  SharedKernel — AGENTS.md §35 forbids module-specific concepts there) with `Code`, `DisplayName`,
  `IsActive`, `SortOrder`, and an `Activate()`/`Deactivate()` behavior pair — each concrete lookup
  (`Sector`, `Position`, ...) is its own class, its own EF-mapped table, no shared table/
  discriminator.
* **Generic MediatR command/query types** — `CreateLookupItemCommand<TLookup>`,
  `DeactivateLookupItemCommand<TLookup>`, `GetLookupItemsQuery<TLookup>` — whose handlers operate
  directly on `ReferenceDataDbContext.Set<TLookup>()`. This is EF Core's own generic accessor, not
  a hand-rolled `IRepository<T>` sitting in front of it; nothing here is a *repository*
  abstraction, so ADR-009 is not violated, only the "write one handler per lookup type" tax is
  avoided.
* One generic endpoint-mapping helper (`LookupEndpoints.Map<TLookup>(app, routeSegment)`) registers
  the GET/POST/PUT/soft-delete routes for a given `TLookup`, called once per lookup type from the
  module's endpoint extension — the routes themselves stay explicit and inspectable in the
  generated OpenAPI document (AGENTS.md §23), this only removes the boilerplate of writing each
  route by hand.

SEED types (`Country`, `Province`, `District`, `Language`) do **not** use the mutating half of this
pattern — only the generic list/query side, since they have no CRUD endpoints by design (Decision
1).

### Recommendation

Proceed with the generic Command/Query-over-`DbSet<TLookup>()` pattern described above. Flagging
it here (rather than just building it) because "generic" and "ADR-009 forbids generic
repositories" are adjacent enough in wording that it deserves an explicit, on-the-record check
that this is a different thing — generic **use-case orchestration over EF Core's own generic
accessor**, not a generic **repository abstraction wrapping EF Core**.

## Consequences

### Positive

* Candidate/Employer get a single, consistent, already-cached way to resolve/validate every
  lookup type, with zero per-consuming-module synchronization code.
* Adding a 19th admin-managed lookup type later is: one entity class, one EF configuration, one
  `LookupEndpoints.Map<TNewLookup>()` call — not a new Command/Query/Handler/Endpoint set.
* The seed/admin-managed split is enforced structurally (no mutation endpoint exists for SEED
  types) rather than by a permission check that could be forgotten on a new endpoint.

### Negative

* `IReferenceDataLookupReader` becomes a dependency every future Candidate/Employer feature that
  touches a lookup field needs to know about and mock in tests — an unavoidable cost of centralizing
  the data at all (ADR-008's decision, not new here).
* If ReferenceData is ever split into its own deployable service, the in-process call becomes a
  network call and every consumer's effective latency/availability profile changes — deferred
  cost, accepted per Decision 2's reasoning, revisited only if/when extraction actually happens.
* 18 admin-managed lookups is a lot of tables in one schema; if any one of them grows real
  business behavior beyond "a named, orderable, soft-deletable value" (e.g., `Skill` eventually
  needing a taxonomy/hierarchy), it graduates out of the generic pattern into its own bespoke
  entity/feature — the generic pattern is a starting point for simple lookups, not a permanent home
  for anything that outgrows it.

## Alternatives Considered

Covered inline under Decision 2 (cross-module access) and Decision 3 (generic CRUD vs ADR-009)
above, since both were genuine multi-option decisions rather than a single obvious path.
