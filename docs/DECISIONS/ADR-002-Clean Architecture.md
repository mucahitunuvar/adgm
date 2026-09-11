# ADR-002 — Clean Architecture

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Uzun ömürlü bir backend oluşturulması ve business logic'in framework/infrastructure detaylarından bağımsız tutulması gerekmektedir.

## Decision

Her modül Clean Architecture prensipleri doğrultusunda tasarlanacaktır.

Temel katmanlar:

```text id="u2c5q6"
Domain
Application / Features
Infrastructure
```

Domain katmanı infrastructure bağımlılığı taşımamalıdır.

Application katmanı use case'leri ve application orchestration'ı yönetmelidir.

Infrastructure teknik implementasyonları sağlamalıdır.

## Why

Bu yapı:

* Business logic'in korunmasını,
* test edilebilirliği,
* dependency inversion'ı,
* infrastructure değişikliklerinin kolaylaşmasını

sağlar.

## Consequences

Katmanlar arası dependency kurallarının Architecture Tests ile korunması gerekir.

Gereksiz abstraction oluşturulmamalıdır.

## Alternatives Considered

### Traditional Layered Architecture

Basit CRUD uygulamalarında yeterli olabilir; ancak Gençlik Merkezi'nin domain ve module karmaşıklığı için yeterince güçlü sınırlar sağlamaz.
