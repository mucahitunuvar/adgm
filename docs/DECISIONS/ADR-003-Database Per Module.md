# ADR-003 — Database Per Module

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Gençlik Merkezi Modular Monolith olarak geliştirilecek ancak modüllerin veri sahipliğinin net olması istenmektedir.

Tek ortak database kullanımı modüller arasında zaman içerisinde güçlü coupling oluşturabilir.

## Decision

Her business module kendi MSSQL database'ine sahip olacaktır.

Örneğin:

```text id="m6v7jq"
Identity       → Identity DB
Candidate      → Candidate DB
Employer       → Employer DB
Job            → Job DB
Interview      → Interview DB
Employment     → Employment DB
```

## Rules

Bir modül:

* başka modülün DbContext'ine erişemez,
* başka modülün entity'sini kullanamaz,
* başka modül database'ine doğrudan SQL gönderemez,
* cross-database foreign key oluşturmamalıdır.

## Why

Bu karar:

* data ownership'i netleştirir,
* module isolation sağlar,
* güvenlik sınırlarını güçlendirir,
* gelecekte microservice extraction'ı kolaylaştırır.

## Consequences

### Positive

* Strong data ownership
* Daha düşük coupling
* Bağımsız schema evolution
* Gelecekte service extraction kolaylığı

### Negative

* Cross-module query daha karmaşık hale gelir.
* Eventual consistency gerekebilir.
* Reporting için özel read model/query stratejileri gerekebilir.
* Database sayısı artar.

## Alternatives Considered

### Single Shared Database

Başlangıçta daha basit görünse de module boundaries'in zaman içerisinde zayıflama riski nedeniyle tercih edilmedi.

### Separate Microservice Database + Microservices

Operasyonel karmaşıklık nedeniyle başlangıç aşamasında tercih edilmedi.
