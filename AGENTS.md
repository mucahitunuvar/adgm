# AGENTS.md

> **Scope:** These instructions apply to the entire repository unless a more specific `AGENTS.md` exists deeper in the directory tree.
>
> **Mandatory:** Read this file before analyzing, generating, moving, refactoring, or deleting code.
>
> Do not treat existing architectural violations as patterns to copy.
>
> **Core rule:** Do not bypass architecture to finish faster.

---

# 1. Mission

This repository follows a production-oriented **.NET Web API + Clean Architecture + Modular Monolith + DDD + CQRS + Vertical Slice** architecture.

The purpose of this file is to define the non-negotiable engineering and Codex execution rules.

Detailed product behavior belongs in:

```text
PROJECT.md
DOMAIN.md
```

Detailed architecture belongs in:

```text
ARCHITECTURE.md
```

Development workflow belongs in:

```text
DEVELOPMENT.md
```

Security requirements belong in:

```text
SECURITY.md
```

Performance requirements belong in:

```text
PERFORMANCE.md
```

Architectural decisions belong in:

```text
docs/adr/
```

AGENTS.md defines **how Codex must work**, not every detail of the system.

---

# 2. Mandatory Documentation Reading

Before meaningful implementation work, Codex must read:

1. `AGENTS.md`
2. `PROJECT.md`
3. `DOMAIN.md`
4. `ARCHITECTURE.md`
5. `DEVELOPMENT.md`
6. `SECURITY.md`
7. `PERFORMANCE.md`
8. Relevant ADR files under `docs/adr/`

If the task affects a specific area, read the corresponding documentation before coding.

Examples:

```text
Authentication
→ SECURITY.md
→ relevant ADRs

Database
→ ARCHITECTURE.md
→ PERFORMANCE.md
→ relevant ADRs

Business workflow
→ DOMAIN.md
→ PROJECT.md

Messaging
→ ARCHITECTURE.md
→ relevant messaging ADR

API
→ ARCHITECTURE.md
→ DEVELOPMENT.md
→ SECURITY.md
```

Do not repeatedly load unrelated documentation when the task does not require it.

---

# 3. Non-Negotiable Architecture

The backend uses:

* ASP.NET Core Web API
* Clean Architecture
* Domain-Driven Design
* Modular Monolith
* Vertical Slice Architecture
* CQRS
* MediatR
* Aggregate-specific repositories
* Unit of Work
* Database-per-Module
* RabbitMQ
* Transactional Outbox Pattern
* FluentValidation
* Authentication / Authorization
* Global Exception Handling
* OpenAPI
* API Versioning
* Health Checks
* OpenTelemetry
* Structured Logging
* Caching
* Background processing
* Notification abstractions
* File Storage abstraction

These technologies and patterns are complementary.

They must not be implemented independently in ways that violate the overall architecture.

---

# 4. Backend-First Rule

The backend is the source of truth for:

* Domain rules
* Business workflows
* Authorization
* Data ownership
* API contracts
* Validation
* Persistence
* Integration events
* Background processing

Backend architecture and foundation must be completed before production-oriented React frontend implementation begins.

The frontend must not drive backend architecture decisions merely to make UI implementation easier.

Frontend development may begin after backend contracts and required business flows are sufficiently mature.

---

# 5. Stop Before Coding

Before implementing a meaningful feature, Codex must:

1. Identify the bounded context/module.
2. Identify the actor.
3. Identify the use case.
4. Identify the aggregate(s).
5. Identify business invariants.
6. Identify authorization requirements.
7. Identify persistence requirements.
8. Identify external side effects.
9. Identify whether messaging/outbox is required.
10. Identify audit requirements.
11. Identify relevant documentation and ADRs.
12. Prepare a concise implementation plan.

For broad changes, Codex must list:

```text
Files to create
Files to modify
Files to move
Files to delete
```

before implementation.

---

# 6. Architectural Conflict Rule

If an implementation requirement conflicts with:

* Clean Architecture
* Module boundaries
* Security
* Domain invariants
* Database ownership
* Data isolation
* Performance constraints
* Existing ADR decisions

Codex must **stop before coding**.

It must explain:

```text
Conflict
Why it conflicts
Affected architecture rule
Possible alternatives
Recommended solution
```

Codex must not silently invent a workaround.

> **Bu projede kod yazmadan önce bu mimari kuralları oku. Mimariyle çelişen bir implementasyon gerekiyorsa kendin karar verme; dur ve gerekçeyi belirt.**

---

# 7. Clean Architecture

Dependency direction must point inward.

```text
Presentation
      ↓
Application
      ↓
Domain

Infrastructure
      ↓
Application
      ↓
Domain
```

The API/Host acts as the composition root.

## Domain

Domain must not depend on:

* EF Core
* ASP.NET Core
* MediatR
* RabbitMQ
* HTTP clients
* SMTP
* File systems
* Infrastructure
* Database APIs
* Serialization frameworks
* Environment-specific services

Domain contains business behavior and invariants.

## Application

Application contains:

* Commands
* Queries
* Handlers
* Validators
* Application contracts
* Authorization policies/abstractions
* Persistence abstractions
* External service abstractions
* Application orchestration

Application must not depend on concrete Infrastructure implementations.

## Infrastructure

Infrastructure contains:

* EF Core
* DbContexts
* Repository implementations
* Unit of Work implementation
* RabbitMQ implementation
* File storage implementation
* Email/SMS providers
* External API clients
* Caching implementation
* Background infrastructure
* Persistence configuration

## Presentation

Presentation contains:

* API endpoints
* HTTP contracts
* Request binding
* Response mapping
* HTTP status handling

Presentation must remain thin.

---

# 8. Modular Monolith

The application is a **Modular Monolith**.

Each module represents a bounded context.

A module owns:

* Its domain
* Its application features
* Its infrastructure
* Its persistence
* Its business rules
* Its public integration contracts

Modules must not become a disguised shared application layer.

Example conceptual structure:

```text
Modules/
├── Identity
├── Candidate
├── Employer
├── CareerAdvisor
├── Job
├── Matching
├── Interview
├── Employment
├── CareerDevelopment
├── Notification
├── CMS
├── Event
├── Support
└── ReferenceData
```

Actual module names must come from `PROJECT.md` and `DOMAIN.md`.

Do not create modules merely because a folder seems convenient.

A module should represent meaningful business ownership.

---

# 9. Database-per-Module

Each business module owns its own database.

Example:

```text
Candidate
    ↓
Candidate Database

Employer
    ↓
Employer Database

Job
    ↓
Job Database
```

## Forbidden

A module must never:

* access another module's DbContext,
* access another module's repository,
* query another module's tables directly,
* execute SQL against another module's database,
* create cross-module foreign keys,
* import another module's internal persistence types.

## Allowed

Cross-module communication may use:

* Explicit contracts
* Integration events
* RabbitMQ
* Approved application-level interfaces
* Read models
* Cache
* Other explicitly documented mechanisms

Never solve a cross-module problem by directly querying another database.

---

# 10. Domain-Driven Design

Business rules belong in the Domain.

## Aggregate rules

* Aggregate roots protect their invariants.
* Child entities must not independently violate aggregate invariants.
* Use controlled behavior methods.
* Avoid public setters for mutable business state.
* Constructors/factories must prevent invalid state.
* Repositories persist aggregate roots.

## Value Objects

Use value objects where the concept has meaningful:

* validation,
* equality,
* invariants,
* domain semantics.

Do not create value objects only for architectural appearance.

## Domain Services

Domain services are allowed only when business behavior does not naturally belong to:

* an entity,
* aggregate root,
* value object.

Do not create generic domain services or managers.

---

# 11. One Type Per File

This is mandatory.

Every manually created:

* Entity
* Aggregate
* Enum
* Value Object
* Domain Event
* Integration Event
* Command
* Query
* Handler
* Validator
* DTO
* Request
* Response
* Repository
* Interface
* Service
* Configuration
* Endpoint
* Consumer
* Worker
* Exception

must have its own file.

The filename must exactly match the type.

Examples:

```text
CandidateStatus.cs
CandidateProfile.cs
CreateCandidateCommand.cs
CreateCandidateCommandHandler.cs
CreateCandidateCommandValidator.cs
CandidateProfileConfiguration.cs
```

## Forbidden bucket files

Never create:

```text
Enums.cs
Entities.cs
Models.cs
Dtos.cs
Requests.cs
Responses.cs
Contracts.cs
Helpers.cs
Utilities.cs
Common.cs
SharedModels.cs
```

Do not group types together merely because it is faster.

Generated files such as EF migrations are exempt from this manual organization rule.

---

# 12. File Naming

Naming must be explicit.

```text
Command
→ CreateCandidateProfileCommand.cs

Handler
→ CreateCandidateProfileCommandHandler.cs

Query
→ GetCandidateProfileQuery.cs

Validator
→ CreateCandidateProfileCommandValidator.cs

Request
→ CreateCandidateProfileRequest.cs

Response
→ CreateCandidateProfileResponse.cs

Repository
→ CandidateRepository.cs

Repository interface
→ ICandidateRepository.cs

Configuration
→ CandidateProfileConfiguration.cs

Domain event
→ CandidateProfileCreatedDomainEvent.cs

Integration event
→ CandidateProfileCreatedIntegrationEvent.cs
```

Do not shorten names for convenience.

---

# 13. CQRS

Commands change state.

Queries read state.

A query must not perform business state mutation.

A command must not become a generic query mechanism.

Commands and queries must be organized by use case.

---

# 14. MediatR

MediatR is used for application-level:

* Commands
* Queries
* Notifications where appropriate

MediatR must not be injected into Domain entities.

MediatR must not become a replacement for domain behavior.

Pipeline behaviors may handle cross-cutting concerns such as:

* Validation
* Authorization
* Logging
* Performance measurement
* Transaction boundaries

Business rules remain in Domain.

---

# 15. Vertical Slice Architecture

Application code is organized by feature/use case.

Preferred:

```text
Features/
└── Candidate/
    └── CreateCandidateProfile/
        ├── CreateCandidateProfileCommand.cs
        ├── CreateCandidateProfileCommandHandler.cs
        ├── CreateCandidateProfileCommandValidator.cs
        └── CreateCandidateProfileResult.cs
```

Do not create global horizontal folders such as:

```text
Commands/
Queries/
Handlers/
Validators/
```

containing every feature in the entire system.

Feature-specific types remain close to their use case.

---

# 16. Repository Pattern

Repositories are aggregate/domain specific.

Preferred:

```text
ICandidateRepository
CandidateRepository
IEmployerRepository
EmployerRepository
```

Generic repositories such as:

```text
IRepository<T>
GenericRepository<T>
```

must not be introduced as the default architecture.

Repositories must express meaningful persistence operations.

Repositories must not become generic CRUD wrappers around EF Core.

Repositories must not expose `IQueryable` outside Infrastructure.

---

# 17. Unit of Work

Unit of Work owns persistence commit behavior.

Repositories must never independently call:

```csharp
SaveChanges();
SaveChangesAsync();
```

A command use case normally has one transaction boundary.

Queries do not commit transactions.

Transaction behavior must be explicit and predictable.

---

# 18. EF Core Rules

EF Core belongs in Infrastructure.

`DbContext` must remain internal to Infrastructure.

Forbidden outside Infrastructure persistence code:

```text
DbContext
DbSet<T>
IQueryable<T>
SQL
Dapper
ADO.NET
DatabaseConnection
```

Application handlers must not directly access EF Core.

Presentation must never access EF Core.

## Query performance

Read-only queries should use:

```csharp
AsNoTracking()
```

when tracking is unnecessary.

Prefer projection:

```csharp
.Select(...)
```

over loading unnecessary entity graphs.

Avoid:

* N+1 queries
* uncontrolled Include usage
* unnecessary entity loading
* huge result sets
* lazy loading

Pagination must be applied to large collections.

---

# 19. FluentValidation

Transport/application input validation should use FluentValidation where appropriate.

Validation must not replace Domain invariants.

The distinction is:

```text
FluentValidation
→ Input / request validation

Domain
→ Business invariants
```

Never rely on frontend validation for security or business correctness.

---

# 20. API Rules

Endpoints must remain thin.

Endpoint responsibilities:

1. Receive HTTP request.
2. Bind request.
3. Perform transport-level concerns.
4. Map request to Command/Query.
5. Send through MediatR.
6. Map result to HTTP response.

Endpoints must not:

* contain business logic,
* access DbContext,
* access repositories,
* publish RabbitMQ directly,
* send email,
* implement complex authorization rules.

---

# 21. API Contracts

Do not expose Domain entities directly through the API.

Use explicit:

* Request models
* Response models
* DTOs
* Results

API contracts must not leak persistence implementation details.

Changing an internal entity must not automatically break an API contract.

---

# 22. API Versioning

API versioning must be explicit.

New breaking API contracts require a new version according to project policy.

Do not silently break existing clients.

Current API versioning strategy must be documented in `ARCHITECTURE.md`.

---

# 23. OpenAPI

The API must expose an up-to-date OpenAPI specification.

OpenAPI is the source of truth for API contract discovery.

Swagger UI may be used as the interactive presentation of the OpenAPI document.

Do not manually maintain duplicated API documentation when it can be generated from the actual API contract.

---

# 24. Global Exception Handling

Unhandled exceptions must be handled centrally.

Use standardized API error responses, preferably:

```text
ProblemDetails
```

Do not duplicate large try/catch blocks inside every endpoint.

Do not expose:

* stack traces,
* database errors,
* internal implementation details,
* secrets

to clients.

Domain/application exceptions must map to appropriate HTTP responses.

---

# 25. Authentication

Authentication establishes identity.

Authorization determines what an authenticated identity may do.

Authentication must be implemented independently from frontend UI state.

Frontend route protection is not a security mechanism.

Backend must always verify authentication.

---

# 26. Authorization

Authorization must support:

```text
Role
+
Permission
+
Resource
+
Business Rules
```

Roles alone are not sufficient for sensitive operations.

Resource ownership and assignment must be enforced server-side.

Examples:

```text
Employer
→ Own company resources

CareerAdvisor
→ Assigned candidates/employers

Candidate
→ Own profile and authorized resources

Admin
→ Administrative permissions
```

Actual business scope comes from `DOMAIN.md`.

Never trust a client-provided user ID, company ID, candidate ID, or advisor ID as proof of authorization.

---

# 27. Security Rules

Security requirements in `SECURITY.md` are mandatory.

Never:

* log passwords,
* log access tokens,
* log refresh tokens,
* log reset tokens,
* expose secrets,
* trust client authorization,
* bypass resource authorization,
* expose sensitive personal data unnecessarily.

Security must be enforced server-side.

---

# 28. RabbitMQ

RabbitMQ is an Infrastructure concern.

Domain entities never publish RabbitMQ messages.

Application code communicates through messaging abstractions.

RabbitMQ is primarily used for asynchronous integration and side-effect processing.

Consumers must be idempotent.

Messages must have:

* explicit contracts,
* versioning strategy,
* stable identifiers,
* correlation information where appropriate.

Do not use Domain entities as integration message contracts.

---

# 29. Outbox Pattern

External side effects related to committed business transactions must use the transactional Outbox Pattern where reliability is required.

Preferred flow:

```text
Command
    ↓
Domain state change
    ↓
Unit of Work
    ├── Business data
    └── Outbox record
    ↓
COMMIT
    ↓
Outbox Processor
    ↓
RabbitMQ / Worker
    ↓
External Side Effect
```

Do not:

```text
Save business data
    ↓
Send email
    ↓
Transaction fails
```

or:

```text
Send RabbitMQ message
    ↓
Database transaction fails
```

without an explicit documented reason.

---

# 30. Notification Architecture

Notifications must be abstracted.

Supported channels may include:

```text
Email
SMS
Push
```

Application code depends on notification abstractions.

Infrastructure implements providers.

HTTP requests must not wait unnecessarily for:

* Email delivery
* SMS delivery
* Push delivery

Long-running notification work should be asynchronous.

---

# 31. Email

Email sending must not occur directly inside the HTTP request for normal transactional notifications.

Preferred:

```text
Business Event
    ↓
Outbox
    ↓
Background Processor
    ↓
Email Provider
```

Failed delivery must be retryable.

Consumers must be idempotent.

---

# 32. Hangfire / Background Jobs

Hangfire may be used for:

* scheduled jobs,
* recurring jobs,
* maintenance,
* notifications,
* report generation,
* cleanup,
* follow-up workflows,
* periodic processing.

Background jobs must be idempotent where duplicate execution is possible.

Do not place long-running work directly inside HTTP requests.

Do not use Hangfire for every asynchronous operation automatically.

Choose RabbitMQ vs Hangfire according to the nature of the operation.

---

# 33. Caching

Initial caching infrastructure:

```text
IMemoryCache
```

Redis may be introduced later if distributed caching becomes necessary.

Caching must never become the source of truth.

Every cache must have a deliberate:

* key strategy,
* expiration policy,
* invalidation strategy,
* stale-data policy.

Do not add caching simply because a query exists.

Measure before introducing complex caching.

---

# 34. File Storage

File storage must be abstracted behind an application-owned contract.

Example conceptual abstraction:

```text
IFileStorage
```

Infrastructure implements the actual provider.

The application must not become coupled to a specific storage provider.

File storage selection must comply with:

* KVKK,
* data residency requirements,
* security requirements,
* backup requirements,
* retention policies.

If an external provider cannot satisfy required data residency/legal constraints, an approved physical/local storage solution may be required.

Do not make provider-specific assumptions without documenting them.

---

# 35. Shared Kernel

Shared Kernel must remain intentionally small.

Suitable content may include genuinely universal primitives such as:

* Entity base abstractions
* Aggregate base abstractions
* Domain event abstractions
* Result/Error primitives
* Strongly typed IDs when truly shared
* Other explicitly approved cross-cutting domain primitives

Do not use Shared Kernel as a dumping ground.

Do not place:

* module-specific DTOs,
* module-specific entities,
* module-specific enums,
* feature-specific services,
* unrelated helper classes,
* business rules belonging to one module

inside Shared Kernel.

If ownership is clear, keep the type inside the owning module.

---

# 36. Health Checks

Health checks must distinguish between:

```text
Liveness
Readiness
```

A health check must not expose sensitive infrastructure details publicly.

Health endpoints must not become an information disclosure mechanism.

External dependencies should be checked according to the application's deployment requirements.

---

# 37. OpenTelemetry and Observability

Observability must be built into the backend.

Where configured, collect:

* traces,
* metrics,
* structured logs,
* request duration,
* database duration,
* external service duration,
* background job duration,
* message processing information.

Correlation IDs / trace IDs should be preserved across asynchronous workflows where possible.

Do not log sensitive data merely to improve observability.

---

# 38. Logging

Use structured logging.

Logs should help answer:

```text
Who?
What?
When?
Where?
Why?
Correlation?
Outcome?
```

Never log:

* passwords,
* tokens,
* secrets,
* full CV contents,
* unnecessary sensitive personal information.

Sensitive operations should be auditable according to `SECURITY.md`.

---

# 39. Performance

Performance rules are defined in `PERFORMANCE.md`.

Core principles:

```text
Correctness
    ↓
Security
    ↓
Architecture
    ↓
Performance
```

Do not break architecture for performance.

Always prefer:

```text
Measure
    ↓
Identify root cause
    ↓
Optimize
    ↓
Measure again
```

Avoid premature optimization.

Do not automatically add:

* Redis,
* caching,
* background jobs,
* RabbitMQ,
* complex indexes,
* query hacks

without a demonstrated need.

---

# 40. Database Performance

When writing queries:

* Prefer projection.
* Use `AsNoTracking()` for read-only operations where appropriate.
* Avoid N+1 queries.
* Avoid unnecessary Include chains.
* Apply pagination.
* Use appropriate indexes.
* Avoid loading entire aggregates for simple read projections.
* Pass `CancellationToken`.
* Avoid uncontrolled cross-module data access.

Never solve a performance problem by violating module ownership.

---

# 41. Transaction Performance

Transactions must remain as small as practical.

Do not perform:

* Email
* SMS
* HTTP calls
* File uploads
* Long calculations
* External integrations

inside database transactions unless explicitly required and documented.

Use Outbox/background processing for external side effects.

---

# 42. Async Programming

I/O operations must use asynchronous APIs.

Forbidden:

```text
.Result
.Wait()
Thread.Sleep()
```

Use:

```text
async
await
CancellationToken
```

CancellationToken should flow through:

```text
Endpoint
→ MediatR
→ Handler
→ Repository
→ Infrastructure
→ External dependency
```

where supported.

---

# 43. Domain Events

Domain events represent facts that occurred inside a bounded context.

Domain entities may raise domain events.

Domain entities must never:

* publish RabbitMQ,
* send email,
* call HTTP,
* access databases,
* access Infrastructure.

Domain events must remain infrastructure-independent.

---

# 44. Integration Events

Integration events are contracts between bounded contexts or external systems.

They must:

* be explicit,
* be versionable,
* avoid Domain entity coupling,
* contain only required integration information,
* remain stable.

Never expose internal Domain entities as integration contracts.

---

# 45. Testing Architecture

The system must contain appropriate levels of tests.

## Domain

Test:

* invariants,
* aggregate behavior,
* value objects,
* domain events.

## Application

Test:

* handlers,
* validation,
* authorization,
* orchestration,
* failure paths.

## Integration

Test:

* EF Core mappings,
* repositories,
* Unit of Work,
* transactions,
* Outbox,
* messaging integration.

## API / Functional

Test:

* authentication,
* authorization,
* HTTP contracts,
* ProblemDetails,
* validation,
* security boundaries.

## Architecture

Test:

* dependency direction,
* module boundaries,
* database access rules,
* Domain independence,
* naming/file rules where practical.

---

# 46. Docker

Docker is not mandatory merely for the sake of containerization.

Do not introduce Docker solely because it is considered modern.

Docker may be introduced when there is a concrete requirement such as:

* deployment,
* reproducible environment,
* CI/CD,
* local infrastructure orchestration,
* production standardization.

Until then, the application must remain fully developable without unnecessary container complexity.

If Docker is introduced, document the decision.

---

# 47. Dependency Management

Do not add a package merely because it solves a small inconvenience.

Before adding a dependency, consider:

* Does the framework already provide this capability?
* Is the dependency maintained?
* Is its license compatible?
* Does it increase operational complexity?
* Does it violate architecture?
* Is it necessary?

Infrastructure dependencies must not leak into Domain.

---

# 48. MediatR and Third-Party License Guard

Do not upgrade critical third-party dependencies blindly.

Before changing a dependency with licensing implications:

1. Check current project policy.
2. Check current license.
3. Check compatibility.
4. Document the architectural decision when necessary.

Do not introduce alternative frameworks merely to avoid understanding the existing architecture.

---

# 49. No AutoMapper by Default

Mapping should be explicit by default.

Do not introduce AutoMapper merely to reduce a few lines of mapping code.

If a mapping library is proposed, document:

* Why it is required,
* Licensing implications,
* Performance impact,
* Architectural impact.

Explicit mapping remains the default.

---

# 50. SOLID

SOLID principles are mandatory but must not be abused to create unnecessary abstractions.

## SRP

Each class must have a clear responsibility.

## OCP

Business variation should not require uncontrolled modification of stable code.

## LSP

Implementations must preserve contract semantics.

## ISP

Prefer focused interfaces.

Do not create:

```text
IApplicationManager
IRepositoryManager
IGeneralService
ICommonService
```

merely to group unrelated operations.

## DIP

Inner layers depend on abstractions.

Infrastructure implements those abstractions.

Do not instantiate Infrastructure dependencies directly inside Domain/Application code.

---

# 51. No God Objects

Avoid:

```text
MegaService
ApplicationService
CommonService
Manager
Helper
Utility
RepositoryManager
```

when they contain unrelated responsibilities.

If a class grows too large, identify the business responsibilities and split them according to real boundaries.

Do not split classes artificially merely to satisfy line-count rules.

---

# 52. No Shortcut Coding

Speed is never a justification for violating architecture.

Never:

* Put multiple entities in one file.
* Put enums inside entity files.
* Put all DTOs in one file.
* Put all commands in one file.
* Inject DbContext into endpoints.
* Access another module's database.
* Add generic repositories merely for convenience.
* Put business rules inside handlers.
* Send email directly from endpoints.
* Publish RabbitMQ directly from Domain.
* Bypass authorization.
* Return Domain entities directly.
* Suppress failing tests.
* Comment out broken code and call the task complete.

---

# 53. Existing Violations

Existing violations do not authorize new violations.

When a task touches an existing violation:

1. Identify it.
2. Fix it if reasonably within scope.
3. Preserve behavior.
4. Move types to correct files/folders.
5. Update references.
6. Run tests.

Do not refactor the entire repository without authorization.

If a violation is outside scope, report it explicitly.

---

# 54. Codex Implementation Workflow

## Before implementation

```text
1. Read AGENTS.md.
2. Read relevant project documentation.
3. Read relevant ADRs.
4. Identify module.
5. Identify actor.
6. Identify use case.
7. Identify aggregate.
8. Identify invariants.
9. Identify authorization.
10. Identify persistence.
11. Identify side effects.
12. Plan files.
```

## During implementation

```text
1. Keep the change scoped.
2. Preserve module boundaries.
3. Preserve dependency direction.
4. Put business rules in Domain.
5. Put orchestration in Application.
6. Keep endpoints thin.
7. Keep Infrastructure concerns in Infrastructure.
8. Use explicit contracts.
9. Follow one-type-per-file.
10. Preserve tests.
```

## Before completion

```text
1. Review changed files.
2. Verify filenames match types.
3. Verify no grouped type files were created.
4. Verify module boundaries.
5. Verify authorization.
6. Verify database ownership.
7. Verify transaction boundaries.
8. Verify outbox requirements.
9. Verify async/cancellation.
10. Verify performance considerations.
11. Run format/analyzers if configured.
12. Run build.
13. Run relevant tests.
14. Report exact validation results.
```

---

# 55. Definition of Done

A feature is not complete merely because it compiles.

The implementation must satisfy:

```text
[ ] Correct module
[ ] Correct bounded context
[ ] Correct domain ownership
[ ] Correct dependency direction
[ ] Correct authorization
[ ] Correct validation
[ ] Correct persistence boundary
[ ] Correct transaction boundary
[ ] Outbox considered
[ ] Audit considered
[ ] Error handling implemented
[ ] Performance considered
[ ] Tests added/updated
[ ] One-type-per-file rule satisfied
[ ] No architecture shortcuts
[ ] Build successful
[ ] Relevant tests successful
```

If an item is not applicable, state why.

---

# 56. Completion Report

Every completed coding task should report:

```text
Summary
- What changed.
- Why it changed.

Architecture
- Module/bounded context affected.
- Domain/Application/Infrastructure/Presentation changes.
- Database affected.
- Integration events/outbox behavior.
- Authorization scope.

Validation
- Commands executed.
- Build result.
- Test result.
- Analyzer/formatter result.

Remaining Issues
- Existing violations.
- Deferred work.
- Risks.
- Assumptions.
```

Do not claim a command succeeded if it was not actually executed.

---

# 57. New Codex Session Rule

At the beginning of a new Codex session:

> Read `AGENTS.md` completely before doing anything.

Then read only the project documents relevant to the requested task.

Do not immediately start coding.

First inspect the repository and existing implementation.

Do not assume that existing code is architecturally correct.

---

# 58. Final Principle

The system must be developed according to the following hierarchy:

```text
Business Requirements
        ↓
Domain Rules
        ↓
Architecture
        ↓
Security
        ↓
Data Ownership
        ↓
Application Use Case
        ↓
Infrastructure
        ↓
Performance Optimization
        ↓
Implementation Convenience
```

Implementation convenience is always the lowest priority.

The objective is not merely to make the code work.

The objective is to build a system that remains:

* Correct
* Secure
* Maintainable
* Testable
* Observable
* Performant
* Modular
* Evolvable
* Understandable
