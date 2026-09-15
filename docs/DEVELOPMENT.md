# Gençlik Merkezi — Development Guide

## 1. Amaç

Bu doküman, Gençlik Merkezi projesinde günlük yazılım geliştirme süreçlerinin nasıl yürütüleceğini tanımlar.

Bu dokümanın amacı:

* Geliştirme sürecini standartlaştırmak
* Claude ve geliştiricilerin aynı çalışma prensiplerini izlemesini sağlamak
* Feature geliştirme sürecini öngörülebilir hale getirmek
* Gereksiz refactor ve teknik borç oluşmasını engellemek
* Frontend ve backend geliştirme sırasını belirlemek
* Test ve doğrulama süreçlerini standartlaştırmak
* Mimari kuralların geliştirme sırasında korunmasını sağlamak

Temel prensip:

> **Önce gereksinimi anla. Sonra mevcut yapıyı incele. Sonra en küçük doğru çözümü uygula.**

---

# 2. Development Philosophy

Projede geliştirme öncelikleri:

```text
Correctness
    ↓
Security
    ↓
Architectural Integrity
    ↓
Testability
    ↓
Maintainability
    ↓
Performance
    ↓
Simplicity
    ↓
Development Speed
```

Kısa vadede hızlı görünen ancak mimariyi bozan çözümler tercih edilmez.

---

# 3. Source of Truth

Geliştirme sırasında aşağıdaki dokümanlar referans alınır:

```text
AGENTS.md
    ↓
PROJECT.md
    ↓
DOMAIN.md
    ↓
ARCHITECTURE.md
    ↓
DEVELOPMENT.md
    ↓
SECURITY.md
    ↓
PERFORMANCE.md
    ↓
ADR
```

Her dokümanın sorumluluğu farklıdır.

```text
AGENTS.md
→ Kodlama ve mimari çalışma kuralları

PROJECT.md
→ Projenin amacı ve kapsamı

DOMAIN.md
→ İş kuralları ve domain

ARCHITECTURE.md
→ Teknik mimari

DEVELOPMENT.md
→ Geliştirme süreci

SECURITY.md
→ Güvenlik

PERFORMANCE.md
→ Performans

ADR
→ Mimari kararların nedenleri
```

Dokümanlar arasında çelişki görülürse geliştirici/Claude kendi başına karar vermemelidir.

---

# 4. Task Before Code

Hiçbir feature doğrudan kod yazılarak başlanmamalıdır.

İlk olarak:

1. Task okunur.
2. Gereksinim anlaşılır.
3. İlgili modül belirlenir.
4. Domain kuralları incelenir.
5. Mevcut feature'lar aranır.
6. Mevcut abstraction'lar incelenir.
7. Mimari etkiler değerlendirilir.
8. Gerekli değişiklikler planlanır.
9. Sonra kod yazılır.

Temel prensip:

> **Kod yazmak ilk adım değildir.**

---

# 5. Claude Working Protocol

Claude her task için aşağıdaki sırayı izlemelidir.

```text
1. AGENTS.md oku
2. Task'ı analiz et
3. İlgili module'ü belirle
4. İlgili dokümanları oku
5. Mevcut kodu araştır
6. Benzer feature'ları bul
7. Mevcut abstraction'ları bul
8. Etkilenecek database'i belirle
9. Cross-module etkileri belirle
10. Uygulama planını oluştur
11. Küçük ve güvenli değişikliği yap
12. Testleri yaz/uyarla
13. Build al
14. İlgili testleri çalıştır
15. Architecture testlerini çalıştır
16. Değişiklikleri gözden geçir
17. Sonucu raporla
```

---

# 6. Existing Code First

Yeni kod yazmadan önce mevcut kod araştırılmalıdır.

Örneğin yeni bir:

```text
NotificationService
```

oluşturulacaksa önce:

```text
Notification
INotification
Email
SMS
Push
```

ile ilgili mevcut abstraction'lar araştırılmalıdır.

Aynı işi yapan ikinci abstraction oluşturulmamalıdır.

Temel prensip:

> **Önce bul. Sonra kullan. Gerçekten yoksa oluştur.**

---

# 7. No Unrelated Refactoring

Bir feature geliştirirken task kapsamı dışındaki kodlar gereksiz yere değiştirilmemelidir.

Örneğin:

```text
Task:
Aday profil fotoğrafı yükleme
```

sırasında:

```text
❌ UserService refactor
❌ Repository redesign
❌ Authentication redesign
❌ Folder structure redesign
```

yapılmamalıdır.

Ancak mevcut kod doğrudan feature'ın çalışmasını engelliyorsa problem raporlanmalıdır.

---

# 8. Backend-First Development Strategy

Projenin geliştirme stratejisi **ADR-010**'da tanımlandığı şekilde **Backend-First**'tür:

> **Önce backend mimarisini, foundation'ı, Identity/Authorization'ı ve domain modüllerini olgunlaştır; React frontend geliştirmesine backend contract'ları ve temel iş akışları yeterince olgunlaştıktan sonra başla.**

Geliştirme sırası (ADR-010, **ADR-013** ile güncellenmiştir):

```text
1. Backend Architecture
2. Backend Foundation
3. Database-per-Module
4. Identity / Authorization
5. Events / RabbitMQ / Outbox
6. Notification
7. Domain Modules
8. CQRS / MediatR
9. API Contracts
10. Backend Tests
11. Backend Completion
12. React Frontend
```

Notification modülü (ve buna bağlı Outbox/RabbitMQ altyapısı) ADR-010'da
8. sırada iken, Identity'nin email verification ve password reset
akışlarının placeholder olmadan gerçekten çalışabilmesi için Identity'den
hemen sonraya çekilmiştir (bkz. **ADR-013**).

Bu doküman daha önce "Frontend-First" bir strateji tanımlıyordu; bu, projenin erken bir aşamasında değerlendirilip vazgeçilen bir yaklaşımın dokümanda unutulmuş kalıntısıydı ve **AGENTS.md §4 (Backend-First Rule)** ile **ADR-010** ile doğrudan çelişiyordu. Bu bölüm o çelişkiyi gidermek için güncellenmiştir.

Aşağıdaki §9-§12 (Frontend Phase, Mock Data, Flow Completion, Definition of Done) backend foundation ve API contract'ları olgunlaştıktan **sonra** başlayacak frontend fazı için geçerliliğini korur; sadece zamanlaması değişmiştir.

---

# 9. Frontend Phase

Backend foundation ve API contract'ları yeterli olgunluğa ulaştıktan sonra, React uygulamalarının çalışan UI ve kullanıcı akışları oluşturulacaktır.

Frontend:

```text
Public Website
Candidate Portal
Employer Portal
Management Portal
```

için:

* Layout
* Navigation
* Sidebar
* Header
* Pages
* Forms
* Tables
* Modal
* Filters
* Search
* Validation UI
* Loading states
* Empty states
* Error states
* Responsive behavior

oluşturulacaktır.

---

# 10. Frontend Mock Data

Backend hazır değilken frontend mock data kullanabilir.

Örneğin:

```text
mockCandidates
mockJobs
mockEmployers
mockInterviews
mockNotifications
```

Ancak mock data:

* Gerçek API yerine geçiyormuş gibi gizlenmemeli
* API contract'larıyla uyumlu tutulmalı
* Business rule'ların kalıcı implementasyonu haline getirilmemeli

Backend hazır olduğunda mock data kaldırılarak gerçek API bağlantısı yapılmalıdır.

---

# 11. Frontend Flow Completion

Frontend geliştirme sırasında yalnızca ekranların görsel olarak tamamlanması yeterli değildir.

Her kullanıcı akışı çalışır durumda olmalıdır.

Örneğin:

```text
Aday kayıt
    ↓
Profil oluştur
    ↓
CV bilgileri
    ↓
Yetenekler
    ↓
İş ilanları
    ↓
Başvuru
    ↓
Görüşme
    ↓
Sonuç
```

gibi akışlar UI seviyesinde uçtan uca tamamlanmalıdır.

---

# 12. Frontend Definition of Done

Bir frontend feature tamamlanmış sayılabilmesi için:

* Sayfa çalışmalı
* Navigation çalışmalı
* Formlar çalışmalı
* Validation UI bulunmalı
* Loading state bulunmalı
* Empty state bulunmalı
* Error state bulunmalı
* Responsive davranış kontrol edilmeli
* Mock data gerekiyorsa düzgün yapılandırılmalı
* Kullanıcı akışı tamamlanmalı
* Console'da gereksiz error/warning bırakılmamalı

---

# 13. Backend Phase

Backend implementation, §8'de tanımlanan Backend-First stratejisine göre projenin en başında başlar (frontend'i beklemez).

Bir backend modülü içindeki geliştirme sırası:

```text
Domain
   ↓
Database
   ↓
Application
   ↓
Validation
   ↓
API
   ↓
Integration
   ↓
Frontend API Integration
```

---

# 14. Feature Development Flow

Yeni bir backend feature için standart akış:

```text
Requirement
    ↓
Domain Analysis
    ↓
Module Identification
    ↓
Existing Code Analysis
    ↓
Domain Model
    ↓
Command / Query
    ↓
Validator
    ↓
Handler
    ↓
Endpoint
    ↓
Database
    ↓
Tests
    ↓
Integration
```

---

# 15. New Feature Checklist

Yeni feature geliştirilirken:

### Analysis

* [ ] Gereksinim anlaşılmış mı?
* [ ] İlgili module belirlenmiş mi?
* [ ] DOMAIN.md kontrol edilmiş mi?
* [ ] Mevcut feature'lar incelenmiş mi?
* [ ] Mevcut abstraction'lar araştırılmış mı?

### Architecture

* [ ] Module boundary korunuyor mu?
* [ ] Başka module DB erişimi var mı?
* [ ] Yeni dependency gerekiyor mu?
* [ ] Public API contract değişiyor mu?

### Implementation

* [ ] Domain kuralları doğru yerde mi?
* [ ] Command/Query ayrımı doğru mu?
* [ ] Validation mevcut mu?
* [ ] Authorization mevcut mu?
* [ ] Endpoint doğru module'de mi?

### Testing

* [ ] Unit test
* [ ] Integration test
* [ ] Architecture test

gerekiyorsa eklenmiş mi?

---

# 16. Domain First

Business rule'lar UI veya controller içerisinde oluşturulmamalıdır.

Yanlış:

```text
React
  ↓
Controller
  ↓
if (...)
```

Business rule domain/application tarafında bulunmalıdır.

Örneğin:

```text
"Onaylanmamış işveren iş ilanı yayınlayamaz."
```

kuralı frontend'e güvenerek korunmamalıdır.

Frontend yalnızca kullanıcı deneyimini sağlar.

Backend business rule'un gerçek sahibidir.

---

# 17. Command Development

Command veri değiştiren işlemler için kullanılır.

Örnek:

```text
CreateCandidateCommand
UpdateCandidateProfileCommand
ApproveEmployerCommand
CreateJobCommand
ApproveJobCommand
ScheduleInterviewCommand
CreateEmploymentCommand
```

Command:

* Input modelini taşır
* Handler'a gönderilir
* Business workflow'u başlatır

---

# 18. Query Development

Query yalnızca veri okumak için kullanılır.

Örnek:

```text
GetCandidateProfileQuery
GetCandidateJobsQuery
GetEmployerProfileQuery
GetAdvisorCandidatesQuery
GetRecommendedCandidatesQuery
```

Query mümkün olduğunca:

* Read-only
* Projection-based
* Pagination destekli

olmalıdır.

---

# 19. Validation

Her public input doğrulanmalıdır.

Validation:

```text
Endpoint
   ↓
Validator
   ↓
Handler
```

şeklinde çalışabilir.

Validation ile domain business rule birbirine karıştırılmamalıdır.

---

# 20. Authorization

Her protected operation için authorization değerlendirilmelidir.

Örnek:

```text
Candidate.UpdateProfile
Employer.CreateJob
Employer.ReadApplications
CareerAdvisor.ReadAssignedCandidates
Admin.ApproveEmployer
```

Authorization yalnızca frontend menü gizleme yöntemiyle sağlanamaz.

Gerçek authorization backend'de uygulanmalıdır.

---

# 21. Database Changes

Database değişikliği gerektiğinde ilgili module'ün database'i değiştirilir.

Örneğin:

```text
Candidate feature
    ↓
CandidateDbContext
    ↓
Candidate Database
```

Job feature:

```text
Job feature
    ↓
JobDbContext
    ↓
Job Database
```

Başka modülün migration'ı değiştirilmez.

---

# 22. Database Migration Rules

Migration oluşturulmadan önce:

* Model değişikliği kontrol edilmeli
* Data loss riski incelenmeli
* Nullable/non-nullable değişiklikler dikkatle değerlendirilmeli
* Existing production data etkisi değerlendirilmelidir

Destructive migration otomatik olarak uygulanmamalıdır.

Örneğin:

```text
DROP COLUMN
DROP TABLE
DATA DELETE
```

gibi işlemler özel dikkat gerektirir.

---

# 23. Cross-Module Development

Feature birden fazla module etkiliyorsa önce dependency graph çıkarılmalıdır.

Örnek:

```text
Employer
   ↓
PersonnelRequestCreated
   ↓
Matching
   ↓
CandidateRecommendationCreated
```

Her module kendi transaction'ını yönetmelidir.

---

# 24. Cross-Module Database Access

Kesinlikle yasaktır.

Yasak:

```text
Candidate
   ↓
EmployerDbContext
```

Yasak:

```text
Matching
   ↓
CandidateDbContext
```

Yasak:

```text
Job
   ↓
SELECT FROM Candidate DB
```

Bunun yerine:

```text
Contract
Event
Message
Application Boundary
```

kullanılmalıdır.

---

# 25. Event Development

Bir event oluşturulmadan önce şu sorular sorulmalıdır:

1. Bu gerçek bir business event mi?
2. Başka module'ler bu olaya ihtiyaç duyuyor mu?
3. Synchronous communication yeterli mi?
4. Asynchronous communication gerekiyor mu?
5. Event tekrar işlendiğinde sorun olur mu?
6. Idempotency nasıl sağlanacak?

Her işlem event haline getirilmemelidir.

---

# 26. Outbox Development

Database değişikliği ile event publish arasında güvenilirlik gerekiyorsa Outbox kullanılmalıdır.

Akış:

```text
Transaction
   ↓
Business Data
   +
Outbox Message
   ↓
Commit
   ↓
Publisher
   ↓
RabbitMQ
```

Event database commit'ten önce RabbitMQ'ya gönderilmemelidir.

---

# 27. Idempotency

Asynchronous işlemler tekrar çalışabilir.

Bu nedenle consumer'lar idempotent tasarlanmalıdır.

Örneğin aynı:

```text
EmploymentCreated
```

event'i iki kez gelirse sistem iki farklı employment oluşturmamalıdır.

---

# 28. API Development

Yeni API endpoint oluşturulurken:

* HTTP method doğru seçilmeli
* Route standardına uyulmalı
* Request DTO tanımlanmalı
* Response contract belirlenmeli
* Validation yapılmalı
* Authorization yapılmalı
* Error response standardı kullanılmalı
* API versioning korunmalı

Örnek:

```text
/api/v1/candidates
/api/v1/candidates/{candidateId}
/api/v1/jobs
/api/v1/jobs/{jobId}
```

---

# 29. API Contract Stability

Public API contract gereksiz yere değiştirilmemelidir.

Breaking change gerekiyorsa:

```text
v1
v2
```

gibi API versioning değerlendirilmelidir.

Frontend'in mevcut kullanımını sessizce bozacak değişiklik yapılmamalıdır.

---

# 30. Frontend API Integration

Backend hazır olduğunda frontend mock data yerine API kullanmaya geçirilir.

Örnek:

```text
React
   ↓
API Client
   ↓
ASP.NET API
   ↓
Module
```

Frontend içerisinde doğrudan:

```text
fetch("...") 
```

dağınık şekilde kullanılmamalıdır.

API communication merkezi bir client/service yaklaşımı üzerinden yönetilmelidir.

---

# 31. Loading / Error / Empty States

Frontend API integration sırasında üç durum mutlaka düşünülmelidir:

```text
Loading
Success
Error
```

Liste ekranlarında ayrıca:

```text
Empty
```

state bulunmalıdır.

---

# 32. Git Workflow

Feature'lar mümkün olduğunca küçük ve anlamlı commit'lerle geliştirilmelidir.

Örnek:

```text
feat: add candidate profile page
feat: add candidate skills section
fix: correct candidate validation
refactor: simplify job query
test: add candidate profile tests
```

Bir commit içerisinde birbirinden bağımsız birçok feature birleştirilmemelidir.

---

# 33. Commit Principles

Commit:

* Küçük
* Anlamlı
* Tek sorumluluklu
* Açıklayıcı

olmalıdır.

Kaçınılmalı:

```text
update
changes
fixes
stuff
final
final2
final-final
```

gibi anlamsız commit mesajlarından.

---

# 34. Code Review

Code review sırasında öncelikle:

1. Doğruluk
2. Security
3. Architecture
4. Testability
5. Maintainability
6. Performance

kontrol edilir.

Kodun sadece çalışması yeterli değildir.

---

# 35. Testing Before Completion

Bir feature "tamamlandı" kabul edilmeden önce ilgili testler çalıştırılmalıdır.

Minimum:

```text
Build
+
Relevant Unit Tests
```

Feature cross-module ise:

```text
Integration Tests
```

de çalıştırılmalıdır.

Architecture etkisi varsa:

```text
Architecture Tests
```

çalıştırılmalıdır.

---

# 36. Test Pyramid

Testler mümkün olduğunca:

```text
          E2E
        /     \
   Integration
      /       \
     Unit Tests
```

şeklinde dengelenmelidir.

Business rule'lar için unit testler tercih edilir.

Database/integration davranışları integration testlerle doğrulanır.

Integration testlerde SQLite kullanılır (bkz. ADR-012); Production ve Development ortamında her zaman SQL Server kullanılır.

---

# 37. Architecture Tests

Architecture testleri aşağıdaki gibi kuralları korumalıdır:

```text
Domain
  ❌ Infrastructure

Candidate
  ❌ Employer.Infrastructure

Job
  ❌ Candidate.DbContext

Matching
  ❌ Candidate.Database

Module
  ❌ Another Module's Entity
```

Bu testler mimari regresyonları erken yakalamak için kullanılmalıdır.

---

# 38. Build Validation

Değişikliklerden sonra mümkün olduğunca:

```text
dotnet build
```

çalıştırılmalıdır.

Frontend için:

```text
npm run build
```

ve projede tanımlı diğer validation komutları çalıştırılmalıdır.

Komutlar proje configuration'ına göre değişebilir.

---

# 39. No Fake Success

Claude bir komut çalıştırmadıysa çalıştırmış gibi raporlamamalıdır.

Örneğin:

```text
❌ Tests passed
```

ifadesi testler gerçekten çalıştırılmadıysa kullanılmamalıdır.

Bunun yerine:

```text
Tests not run.
```

denmelidir.

---

# 40. Error Handling During Development

Bir test veya build başarısız olursa hata gizlenmemelidir.

Önce:

```text
Root Cause
```

belirlenmelidir.

Ardından düzeltme yapılmalıdır.

Sadece error'u susturmak için:

```text
#pragma warning disable
```

veya benzeri çözümler kullanılmamalıdır.

---

# 41. Package Management

Yeni NuGet veya npm package eklenmeden önce:

1. Gerçekten gerekli mi?
2. .NET/React built-in capability yeterli mi?
3. Projede benzer package var mı?
4. Package aktif olarak destekleniyor mu?
5. Security/reputation problemi var mı?
6. Uzun vadeli dependency oluşturuyor mu?

soruları değerlendirilmelidir.

---

# 42. No Unnecessary Dependencies

Bir package yalnızca "işi kolaylaştırıyor" diye eklenmemelidir.

Öncelik:

```text
Built-in
    ↓
Existing Project Dependency
    ↓
New Dependency
```

şeklindedir.

---

# 43. Configuration

Configuration:

```text
Development
Test
Staging
Production
```

ortamlarına göre yönetilmelidir.

Secret'lar source control'e commit edilmemelidir.

---

# 44. Local Development

Local development ortamında geliştirici:

```text
Frontend
Backend API
Module Databases
RabbitMQ
Hangfire
```

bileşenlerini gerektiğinde çalıştırabilmelidir.

Docker kullanılacaksa geliştirme deneyimini kolaylaştırmak amacıyla kullanılabilir.

Docker mimarinin zorunlu bir parçası değildir.

---

# 45. Debugging

Bir hata oluştuğunda önce:

```text
Frontend
↓
API
↓
Module
↓
Database
↓
External Dependency
```

zinciri kontrol edilmelidir.

Cross-module event kullanılan işlemlerde:

```text
Producer
↓
Outbox
↓
RabbitMQ
↓
Consumer
```

zinciri ayrıca kontrol edilmelidir.

---

# 46. Performance During Development

Development sırasında gereksiz micro-optimization yapılmamalıdır.

Öncelik:

```text
Correct implementation
```

sonrasında:

```text
Measure
↓
Identify bottleneck
↓
Optimize
↓
Measure again
```

şeklindedir.

---

# 47. Security During Development

Security son aşamaya bırakılmamalıdır.

Yeni feature geliştirilirken:

* Authentication
* Authorization
* Input validation
* Resource access
* Sensitive data
* File access
* Logging

başlangıçtan itibaren değerlendirilmelidir.

---

# 48. Sensitive Data

Aday ve işveren verileri kişisel veri içerebilir.

Bu nedenle:

* Gereksiz veri toplanmamalı
* Gereksiz loglanmamalı
* Gereksiz API response'larında gönderilmemeli
* Yetkisiz kullanıcıya gösterilmemeli

ve ilgili güvenlik politikalarına uyulmalıdır.

Detaylar `SECURITY.md` içerisinde tanımlanır.

---

# 49. Feature Scope

Bir task mümkün olduğunca küçük tutulmalıdır.

Örneğin:

```text
Task:
Adayın sertifika ekleyebilmesi
```

şu hale dönüşmemelidir:

```text
Candidate system redesign
+
Profile redesign
+
Matching redesign
+
Database redesign
```

Gerekli mimari ön koşul bulunuyorsa ayrıca belirtilmelidir.

---

# 50. Stop & Ask Rule

Claude aşağıdaki durumlarda kendi başına karar vermemelidir:

* Mimari kural değişmesi gerekiyorsa
* Başka module DB erişimi gerekiyorsa
* Breaking API change gerekiyorsa
* Authentication değişiyorsa
* Authorization modelinin değişmesi gerekiyorsa
* Production data loss riski varsa
* Kritik dependency eklenmesi gerekiyorsa
* Büyük refactor gerekiyorsa
* Domain rule belirsizse
* Birden fazla geçerli mimari seçenek arasında seçim yapılması gerekiyorsa

Bu durumda:

```text
STOP
↓
Explain
↓
Ask for decision
```

uygulanmalıdır.

---

# 51. Definition of Done

Bir backend feature şu şartlar sağlandığında tamamlanmış kabul edilir:

* [ ] Gereksinim karşılanıyor
* [ ] Doğru module içerisinde
* [ ] Domain kuralları doğru yerde
* [ ] Authorization uygulanmış
* [ ] Validation uygulanmış
* [ ] API contract uygun
* [ ] Database değişiklikleri doğru module DB'sinde
* [ ] Cross-module boundary korunmuş
* [ ] Gerekli event'ler doğru şekilde uygulanmış
* [ ] Unit tests yazılmış
* [ ] Gerekli integration tests yazılmış
* [ ] Architecture tests geçiyor
* [ ] Build başarılı
* [ ] Gereksiz dependency yok
* [ ] Gereksiz refactor yok
* [ ] Logging/security kontrolleri yapılmış

---

# 52. Frontend Definition of Done

Frontend feature:

* [ ] UI tamam
* [ ] Navigation tamam
* [ ] User flow tamam
* [ ] Validation tamam
* [ ] Loading state tamam
* [ ] Error state tamam
* [ ] Empty state tamam
* [ ] Responsive kontrol edildi
* [ ] Mock data gerekiyorsa düzenli
* [ ] API integration hazır veya tamam
* [ ] Build başarılı
* [ ] Console error/warning yok

---

# 53. Final Claude Checklist

Claude task tamamlamadan önce:

```text
[ ] AGENTS.md kurallarına uyuldu
[ ] Doğru module kullanıldı
[ ] Domain kuralları korundu
[ ] Cross-module DB erişimi yapılmadı
[ ] Yeni abstraction gereksiz değil
[ ] Yeni package gereksiz değil
[ ] Security kontrol edildi
[ ] Authorization kontrol edildi
[ ] Validation kontrol edildi
[ ] Tests çalıştırıldı
[ ] Build çalıştırıldı
[ ] Architecture tests çalıştırıldı
[ ] Unrelated files değiştirilmedi
[ ] Unrelated refactor yapılmadı
[ ] Production data riski kontrol edildi
[ ] Yapılan değişiklikler raporlandı
```

---

# 54. Development Golden Rules

Bu proje için geliştirme sırasında aşağıdaki kurallar bağlayıcıdır:

### Rule 1

> **Kod yazmadan önce mevcut kodu ve mimariyi oku.**

### Rule 2

> **Var olan abstraction'ı tekrar oluşturma.**

### Rule 3

> **Bir modül başka modülün database'ine erişemez.**

### Rule 4

> **Entity paylaşma, contract paylaş.**

### Rule 5

> **Business rule'u frontend'e bırakma.**

### Rule 6

> **Gereksiz abstraction oluşturma.**

### Rule 7

> **Gereksiz dependency ekleme.**

### Rule 8

> **Task kapsamı dışında refactor yapma.**

### Rule 9

> **Test etmediğin şeyi test edilmiş gibi raporlama.**

### Rule 10

> **Mimariyle çelişen durumda kendi kararını verme; dur ve sor.**

---

# 55. Core Development Motto

> **Önce gereksinimi anla.**
>
> **Sonra mimariyi anla.**
>
> **Sonra mevcut kodu incele.**
>
> **Sonra en küçük doğru değişikliği yap.**
>
> **Sonra test et.**
>
> **Sonra raporla.**
