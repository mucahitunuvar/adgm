# ADR-001 — Modular Monolith

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Gençlik Merkezi; Identity, Candidate, Employer, CareerAdvisor, Job, Matching, Interview, Employment, CareerDevelopment, Event, Notification, CMS, Media ve ReferenceData gibi birden fazla iş alanına sahip olacaktır.

Sistem başlangıç aşamasında microservice mimarisinin operasyonel karmaşıklığına ihtiyaç duymamaktadır.

Bununla birlikte domain sınırlarının ileride bağımsız servis haline getirilebilecek şekilde tasarlanması istenmektedir.

## Decision

Sistem **Modular Monolith** olarak geliştirilecektir.

Tüm modüller başlangıçta tek backend uygulaması içerisinde çalışacaktır.

Modüller:

* kendi domain'lerine,
* kendi application logic'lerine,
* kendi infrastructure'larına,
* kendi database'lerine

sahip olacaktır.

Modüller arası iletişim açık contract ve event mekanizmaları üzerinden gerçekleştirilecektir.

## Why

Bu yaklaşım:

* Microservice operasyonel maliyetini azaltır.
* Deployment'ı basitleştirir.
* Domain sınırlarını korur.
* Test edilebilirliği artırır.
* Gerektiğinde modüllerin microservice'e ayrıştırılmasını kolaylaştırır.

## Consequences

### Positive

* Tek deployment
* Daha kolay local development
* Daha düşük operasyonel maliyet
* Net module boundaries
* Gelecekte service extraction imkanı

### Negative

* Module boundary ihlallerinin kod seviyesinde engellenmesi gerekir.
* Tek uygulamanın kaynak tüketimi büyüyebilir.
* Modüller başlangıçta aynı runtime üzerinde çalışır.

## Alternatives Considered

### Microservices

Başlangıç aşamasında gereksiz operasyonel ve altyapısal karmaşıklık oluşturacağı için tercih edilmedi.

### Traditional Monolith

Domain sınırlarını yeterince güçlü korumadığı için tercih edilmedi.
