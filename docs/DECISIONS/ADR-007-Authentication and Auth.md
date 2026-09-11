# ADR-007 — Authentication and Authorization

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Platform farklı kullanıcı rollerine ve resource ownership/assignment kurallarına sahiptir.

Sadece authentication yeterli değildir.

## Decision

Authorization aşağıdaki katmanlardan oluşacaktır:

```text id="c9g3h2"
Authentication
        ↓
Role
        ↓
Permission
        ↓
Resource Authorization
        ↓
Business Rules
```

Temel roller:

* Admin
* Employer
* Candidate
* CareerAdvisor

Resource authorization zorunludur.

Örneğin CareerAdvisor yalnızca kendisine atanmış Candidate kaynaklarına erişebilir.

Employer yalnızca kendi company scope'u içerisindeki kaynaklara erişebilir.

## Why

Role-based authorization tek başına yeterli değildir.

ID enumeration, unauthorized resource access ve privilege escalation risklerini azaltmak için resource-level authorization gereklidir.

## Consequences

Authorization logic merkezi ve tekrar kullanılabilir şekilde tasarlanmalıdır.

Frontend authorization yalnızca UI davranışıdır; gerçek güvenlik backend'de uygulanır.
