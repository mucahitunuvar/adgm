# ADR-004 — CQRS and MediatR

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Platformda çok sayıda farklı use case bulunmaktadır.

Command ve query sorumluluklarının ayrılması application layer'ın daha düzenli ve test edilebilir olmasını sağlayacaktır.

## Decision

Application layer'da **CQRS** kullanılacaktır.

* Commands → state-changing operations
* Queries → read operations

CQRS orchestration'ında MediatR kullanılacaktır.

## Why

* Use case'lerin ayrışmasını sağlar.
* Feature-based organizasyonu destekler.
* Pipeline behaviors kullanılabilir.
* Validation, logging ve transaction davranışları merkezi şekilde uygulanabilir.
* Test edilebilirliği artırır.

## Consequences

Her işlem için gereksiz abstraction oluşturulmamalıdır.

Basit bir işlem sırf CQRS kullanılıyor diye gereksiz karmaşıklığa dönüştürülmemelidir.

## Alternatives Considered

### Traditional Service Layer

Büyük service sınıflarının zaman içerisinde God Object haline gelme riski nedeniyle tercih edilmedi.
