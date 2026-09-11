# ADR-008 — ReferenceData as Separate Module

* **Status:** Accepted
* **Date:** 2026-09-10

## Context

Platformda Country, City, District, School, University, Sector, Profession ve Skill gibi ortak kullanılan referans verileri bulunmaktadır.

Bu verilerin SharedKernel içerisine taşınması domain boundaries'i zayıflatabilir.

## Decision

Reference data ayrı bir **ReferenceData module/database** içerisinde tutulacaktır.

Örnek:

```text id="x6s4s5"
ReferenceData
├── Country
├── City
├── District
├── Sector
├── Profession
├── Skill
├── University
└── School
```

## Important Distinction

```text id="g2qf1n"
ReferenceData.Skill
        ≠
Candidate.CandidateSkill
```

`ReferenceData.Skill` standart skill tanımıdır.

`CandidateSkill` ise adayın bu skill ile ilişkisini/proficiency bilgisini temsil eder.

## Why

SharedKernel yalnızca gerçekten ortak teknik/domain primitive'leri içermelidir.

Business reference records SharedKernel içerisinde tutulmamalıdır.

## Consequences

Reference data için module communication ve cache stratejisi gerekir.

Sık kullanılan reference data cache için uygun adaydır.
