# ADR-009 — No Generic Repository

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

EF Core zaten Unit of Work ve repository benzeri abstraction'ları DbContext üzerinden sağlamaktadır.

Generic repository abstraction çoğu durumda EF Core'un özelliklerini tekrar eden bir abstraction katmanı oluşturur.

## Decision

Genel amaçlı:

```csharp
IRepository<T>
```

ve benzeri Generic Repository abstraction kullanılmayacaktır.

EF Core DbContext doğrudan uygun application/infrastructure sınırlarında kullanılabilir.

Gerçekten ihtiyaç olduğunda domain/feature-specific repository oluşturulabilir.

Örneğin:

```text id="eqd1is"
ICandidateRepository
IJobRepository
```

gibi repository'ler yalnızca gerçek bir domain ihtiyacı varsa oluşturulmalıdır.

## Why

* Gereksiz abstraction engellenir.
* EF Core özellikleri kaybedilmez.
* Query'lerin açıkça görülmesi sağlanır.
* Kod miktarı azaltılır.

## Consequences

Her modül kendi data access yaklaşımını bilinçli olarak tasarlamalıdır.

Repository kullanmak bir zorunluluk değildir.
