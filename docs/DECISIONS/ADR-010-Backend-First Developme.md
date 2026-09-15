# ADR-010 — Backend-First Development

* **Status:** Accepted (development order steps 5-8 amended by ADR-013)
* **Date:** 2026-09-10

## Context

Gençlik Merkezi'nin frontend'i dört farklı React uygulamasından oluşacaktır.

Backend ise tüm business rules, domain logic, authorization, data ownership ve API contract'larını belirleyen temel sistemdir.

Frontend geliştirmesine backend contract'ları olgunlaşmadan başlanması, UI tarafında speculative development ve sonradan büyük değişiklikler oluşturabilir.

## Decision

Geliştirme süreci **Backend-First** olarak yürütülecektir.

Öncelik:

```text id="v8qg8k"
1. Backend Architecture
2. Backend Foundation
3. Database-per-Module
4. Identity / Authorization
5. Domain Modules
6. CQRS / MediatR
7. Events / RabbitMQ / Outbox
8. Notification / Hangfire / Media
9. API Contracts
10. Backend Tests
11. Backend Completion
12. React Frontend
```

React frontend geliştirmesi backend foundation ve API contract'ları yeterli olgunluğa ulaştıktan sonra başlayacaktır.

Frontend daha sonra kullanıcı ile birlikte tasarlanıp geliştirilecektir.

## Why

* Backend domain kuralları önce netleşir.
* API contract'ları stabil hale gelir.
* Frontend'in speculative geliştirilmesi engellenir.
* React uygulamaları gerçek backend davranışları üzerine inşa edilir.
* UI ve backend arasında gereksiz rework azaltılır.

## Consequences

Frontend geliştirmesi projenin erken aşamalarında yapılmayacaktır.

Backend tamamlanmadan frontend için büyük miktarda production-oriented UI kodu oluşturulmamalıdır.

Frontend geliştirme aşamasına geçildiğinde:

```text id="x8qk9f"
API Contracts
+
Authentication
+
Authorization
+
Real Domain Flows
+
Backend Tests
```

temel alınacaktır.
