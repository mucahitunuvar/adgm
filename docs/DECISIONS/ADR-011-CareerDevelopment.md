# ADR-011 — CareerDevelopment as Separate Module

* **Status:** Accepted
* **Date:** 2026-09-15

## Context

Kariyer danışmanının aday için belirlediği skill gap, kariyer hedefi, gelişim planı ve eğitim/danışman önerileri başlangıçta Candidate modülünün bir parçası sayılmıştı.

Ancak bu sorumluluk (adayın kariyer gelişim sürecinin danışman eşliğinde yönetilmesi) Candidate'ın kendi kimlik/profil sorumluluğundan bağımsız, kendi başına büyüyen bir domaindir; ayrıca Candidate modülüyle değil CareerAdvisor'ın değerlendirme akışıyla daha güçlü ilişkilidir.

Bu sorumluluğu Candidate modülü içinde tutmak, ADR-001 (Modular Monolith) ve ADR-003 (Database Per Module) kapsamında modülün "meaningful business ownership" sınırını bulanıklaştırır.

## Decision

CareerDevelopment ayrı bir modül ve ayrı bir database olarak tanımlanır (bkz. ARCHITECTURE.md §8, §10.1).

Sorumlulukları:

```text
CareerDevelopment
    │
    ├── SkillGap
    ├── CareerGoal
    ├── DevelopmentPlan
    ├── TrainingRecommendation
    └── AdvisorRecommendation
```

Önemli sınır:

```text
CareerDevelopment.TrainingRecommendation
        ≠
Website.Training
```

`Website.Training` eğitimin kendisidir (tarih, yer, içerik, kontenjan) ve Website modülünün sahipliğindedir.

`TrainingRecommendation` ise "bu adaya, şu SkillGap nedeniyle, şu eğitim önerildi" kaydıdır ve CareerDevelopment modülünün sahipliğindedir. CareerDevelopment, Website.Training'e yalnızca contract/ID referansı ile bağlanır; Website'in database'ine doğrudan erişmez veya Training verisini kopyalamaz (bkz. ADR-003, ADR-008).

Akış:

```text
Candidate
   ↓
CareerAdvisor Değerlendirmesi
   ↓
SkillGap
   ↓
DevelopmentPlan
   ↓
TrainingRecommendation
   ↓
Training (Website)
   ↓
Skill Improvement
   ↓
Matching
```

CareerDevelopment, Matching'in kendisini gerçekleştirmez; Matching, CareerDevelopment sürecinin ürettiği güncellenmiş yetkinlik/hedef bilgisini girdi olarak kullanır.

## Why

* Candidate modülü kendi kimlik/profil sorumluluğuna odaklanmış kalır; kariyer gelişim süreci ayrı, net bir bounded context olur.
* CareerAdvisor'ın değerlendirme/öneri akışı ile doğal olarak birlikte evrilen bir domain, kendi database'inde izole edilmiş olur (ADR-003).
* Training'in kendisi ile "bu adaya önerilen training" kaydı arasındaki sahiplik netleşir; Website modülü training içeriğinin tek sahibi kalır.
* Gelecekte CareerDevelopment sürecinin (ör. AI destekli skill-gap analizi) bağımsız ölçeklenmesi gerekirse, modül sınırı zaten microservice extraction'a uygun şekilde çizilmiş olur (ADR-001).

## Consequences

### Positive

* Net domain ownership: SkillGap/DevelopmentPlan/TrainingRecommendation/AdvisorRecommendation Candidate'ın değil CareerDevelopment'ın sorumluluğundadır.
* Website.Training ile TrainingRecommendation arasındaki fark dokümante edilmiş olur; gelecekteki karışıklıklar (ör. training verisinin CareerDevelopment'a kopyalanması) önlenir.
* Candidate modülü daha küçük ve odaklı kalır.

### Negative

* Candidate → CareerDevelopment arasında ek bir cross-module iletişim (contract/ID referansı veya integration event) gerekir; tek modülde tutmaya kıyasla ek karmaşıklık oluşur.
* Bir DevelopmentPlan'ın tam görünümü için (Candidate bilgisi + CareerDevelopment bilgisi + Website.Training bilgisi) birden fazla modülden veri birleştirilmesi gerekebilir; bu genellikle read model/aggregation ile çözülür.

## Alternatives Considered

### CareerDevelopment'ı Candidate Modülü İçinde Tutmak

Başlangıçta daha az modül/database anlamına gelse de, Candidate'ın sorumluluk alanını gereğinden fazla genişletir ve CareerAdvisor'ın değerlendirme akışıyla olan güçlü ilişkiyi modül sınırlarında görünmez kılar. Bu nedenle reddedildi.

### CareerDevelopment'ı CareerAdvisor Modülü İçinde Tutmak

CareerAdvisor modülü danışmanın kendisini, atamalarını ve iş yükünü yönetir; SkillGap/DevelopmentPlan gibi adaya ait, zaman içinde biriken veriler CareerAdvisor'ın yaşam döngüsünden bağımsızdır (bir danışman değişse dahi adayın gelişim geçmişi kalıcıdır). Bu nedenle ayrı bir modül olarak tutulması tercih edildi.
