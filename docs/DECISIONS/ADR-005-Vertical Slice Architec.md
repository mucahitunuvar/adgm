# ADR-005 — Vertical Slice Architecture

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Feature sayısının büyümesiyle klasik katman bazlı klasör yapısında ilgili kodların farklı klasörlere dağılması beklenmektedir.

## Decision

Application feature'ları **Vertical Slice Architecture** yaklaşımıyla organize edilecektir.

Örneğin:

```text id="8y8kqf"
Features/
└── CreateCandidate/
    ├── CreateCandidateCommand.cs
    ├── CreateCandidateHandler.cs
    ├── CreateCandidateValidator.cs
    └── CreateCandidateEndpoint.cs
```

Her feature kendi use case'ine ait kodu mümkün olduğunca birlikte tutmalıdır.

## Why

* Feature cohesion artırılır.
* Değişiklik yapılacak alan daha kolay bulunur.
* Büyük service sınıfları engellenir.
* Feature bazlı test kolaylaşır.

## Consequences

Bazı küçük tekrarlar kabul edilebilir.

DRY prensibi uğruna feature'lar arasında gereksiz abstraction oluşturulmamalıdır.
