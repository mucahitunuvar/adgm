# ADR-006 — RabbitMQ and Outbox Pattern

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Modüller arasında güvenilir asynchronous communication gerekmektedir.

Database transaction ile message publish işleminin birbirinden bağımsız başarısız olması veri tutarsızlığı oluşturabilir.

## Decision

Asynchronous integration communication için **RabbitMQ** kullanılacaktır.

Reliable event publishing için **Outbox Pattern** uygulanacaktır.

Temel akış:

```text id="7gqs0z"
Business Transaction
        ↓
Business Data
+
Outbox Record
        ↓
Commit
        ↓
Outbox Processor
        ↓
RabbitMQ
```

## Why

Bu yaklaşım:

* event kaybı riskini azaltır,
* transaction ile event üretimini ilişkilendirir,
* modüller arası coupling'i azaltır,
* eventual consistency'yi destekler.

## Consequences

Consumer'lar idempotent olmalıdır.

Retry ve dead-letter/error handling stratejileri uygulanmalıdır.

Her operasyon event-driven yapılmamalıdır.

## Alternatives Considered

### Direct synchronous module communication

Basit işlemler için kullanılabilir ancak asynchronous integration ihtiyacını karşılamaz.

### Distributed Transaction

Operasyonel ve teknik karmaşıklığı yüksek olduğu için tercih edilmedi.
