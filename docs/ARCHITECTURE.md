# Gençlik Merkezi — Architecture

## 1. Amaç

Bu doküman, Gençlik Merkezi platformunun teknik mimarisini, modül sınırlarını, veri erişim kurallarını, uygulama katmanlarını, modüller arası iletişim yöntemlerini, güvenlik sınırlarını, test yaklaşımını ve ölçeklenebilirlik prensiplerini tanımlar.

Bu dokümanın temel amacı:

* Sistemin sürdürülebilir olmasını sağlamak
* Modüller arasındaki bağımlılıkları kontrol altında tutmak
* Veri izolasyonunu sağlamak
* Yeni özelliklerin mevcut sistemi bozmadan eklenebilmesini sağlamak
* Claude ve geliştiriciler için net mimari sınırlar oluşturmak
* Gelecekte gerektiğinde belirli modüllerin bağımsız servislere ayrılabilmesini mümkün kılmak

Temel prensip:

> **Önce mimariyi anla. Sonra kodu yaz.**

---

# 2. Architectural Style

Gençlik Merkezi başlangıç aşamasında **Modular Monolith** mimarisi kullanacaktır.

Sistem:

* Tek ASP.NET Core Web API uygulaması
* Birden fazla bağımsız domain/application modülü
* Her modül için ayrı MSSQL database
* Modül sınırlarını koruyan Clean Architecture prensipleri
* Feature bazlı Vertical Slice yaklaşımı
* CQRS
* MediatR
* Domain Events
* Integration Events
* Outbox Pattern
* RabbitMQ
* Hangfire

kullanacaktır.

## 2.1. Modular Monolith

Modüller aynı uygulama içerisinde çalışır ancak birbirlerinden mantıksal ve veri seviyesinde izole edilir.

Önemli:

> Modular Monolith, Microservice değildir.

İlk aşamada:

```text
                    ASP.NET Core API
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
    Identity           Candidate          Employer
        │                  │                  │
     Identity DB       Candidate DB       Employer DB
```

şeklinde tek uygulama içerisinde çalışacağız.

İleride operasyonel veya ölçekleme gereksinimi oluşması halinde belirli bir modül bağımsız servise dönüştürülebilir.

Bu nedenle modül sınırları başlangıçtan itibaren microservice sınırlarına mümkün olduğunca uygun tasarlanmalıdır.

---

# 3. Genel Sistem Mimarisi

Sistem dört ayrı frontend uygulaması ve tek backend API üzerine kurulacaktır.

```text
                    ┌────────────────────────────┐
                    │       Public Website       │
                    │   genclikmerkezi.com       │
                    └─────────────┬──────────────┘
                                  │
                    ┌─────────────▼──────────────┐
                    │       Candidate Portal      │
                    │ aday.genclikmerkezi.com     │
                    └─────────────┬──────────────┘
                                  │
                    ┌─────────────▼──────────────┐
                    │       Employer Portal       │
                    │ isveren.genclikmerkezi.com  │
                    └─────────────┬──────────────┘
                                  │
                    ┌─────────────▼──────────────┐
                    │      Management Portal      │
                    │ yonetim.genclikmerkezi.com  │
                    └─────────────┬──────────────┘
                                  │
                                  ▼
                    ┌────────────────────────────┐
                    │      ASP.NET Core API       │
                    │       Modular Monolith      │
                    └─────────────┬──────────────┘
                                  │
       ┌──────────────┬───────────┼───────────┬──────────────┐
       ▼              ▼           ▼           ▼              ▼
   Identity       Candidate    Advisor    Career Dev.       Job
      DB             DB           DB          DB             DB

       ▼              ▼           ▼           ▼              ▼
   Matching        Employer   Interview   Employment    Notification
      DB             DB           DB          DB             DB

       ▼              ▼           ▼
    Website         Support   ReferenceData
      DB             DB           DB
```

---

# 4. Frontend Architecture

Frontend tarafında dört bağımsız React uygulaması bulunacaktır.

## 4.1. Public Website

```text
genclikmerkezi.com
```

Kurumsal ve kamuya açık içerikleri sunar.

Sorumlulukları:

* Kurumsal içerikler
* Haberler
* Duyurular
* Etkinlikler
* Projeler
* İş ilanları
* Başarı hikayeleri
* Eğitim ve atölyeler
* İletişim
* Genel arama
* Aday kayıt
* İşveren kayıt
* Giriş

---

## 4.2. Candidate Portal

```text
aday.genclikmerkezi.com
```

Adayların kariyer süreçlerini yönettiği uygulamadır.

Temel alanlar:

* Profil
* CV
* Eğitim
* Deneyim
* Yetenekler
* Sertifikalar
* Yabancı dil
* Kariyer hedefleri
* İş tercihleri
* İş ilanları
* Başvurular
* Öneriler
* Görüşmeler
* Eğitimler
* Etkinlikler
* Kariyer danışmanı
* İstihdam durumu

---

## 4.3. Employer Portal

```text
isveren.genclikmerkezi.com
```

İşverenlerin şirket ve personel ihtiyaçlarını yönettiği uygulamadır.

Temel alanlar:

* Firma profili
* Firma çalışan/yetkili bilgileri
* İş ilanları
* Personel ihtiyaçları
* Aday önerileri
* Başvurular
* Görüşmeler
* Aday değerlendirmeleri
* İşe alım bilgileri

---

## 4.4. Management Portal

```text
yonetim.genclikmerkezi.com
```

Admin ve Kariyer Danışmanı tarafından kullanılacaktır.

Aynı frontend içerisinde role/permission tabanlı erişim uygulanacaktır.

Örneğin:

```text
Admin
 ├── Kullanıcı yönetimi
 ├── Firma yönetimi
 ├── Aday yönetimi
 ├── Danışman yönetimi
 ├── İçerik yönetimi
 ├── İş ilanları
 ├── Raporlama
 └── Sistem yönetimi

CareerAdvisor
 ├── Kendisine atanan adaylar
 ├── Kendisine atanan firmalar
 ├── Aday değerlendirmeleri
 ├── Personel ihtiyaçları
 ├── İş ilanları
 ├── Eşleştirmeler
 ├── Görüşmeler
 └── İstihdam takibi
```

Frontend ayrımı güvenlik sınırı değildir.

Asıl güvenlik backend tarafındaki:

* Authentication
* Role
* Permission
* Resource authorization

mekanizmaları ile sağlanır.

---

# 5. Backend Architecture

Backend:

```text
ASP.NET Core Web API
```

üzerinde çalışacaktır.

Ana mimari prensip:

```text
Clean Architecture
        +
Modular Monolith
        +
Vertical Slice Architecture
        +
CQRS
```

---

# 6. Solution Structure

Önerilen genel solution yapısı:

```text
src/
├── Host/
│   └── GenclikMerkezi.Api/
│
├── BuildingBlocks/
│   ├── SharedKernel/
│   ├── Infrastructure/
│   └── Contracts/
│
└── Modules/
    ├── Identity/
    ├── Candidate/
    ├── CareerAdvisor/
    ├── CareerDevelopment/
    ├── Job/
    ├── Matching/
    ├── Employer/
    ├── Interview/
    ├── Employment/
    ├── Notification/
    ├── Website/
    ├── Support/
    └── ReferenceData/
```

Testler:

```text
tests/
├── UnitTests/
├── IntegrationTests/
└── ArchitectureTests/
```

---

# 7. Module Architecture

Her modül kendi iç mimarisine sahip olacaktır.

Örnek:

```text
Modules/
└── Candidate/
    ├── Domain/
    │   ├── Candidate.cs
    │   ├── CandidateProfile.cs
    │   ├── CandidateStatus.cs
    │   └── ...
    │
    ├── Features/
    │   ├── CreateCandidate/
    │   │   ├── CreateCandidateCommand.cs
    │   │   ├── CreateCandidateHandler.cs
    │   │   ├── CreateCandidateValidator.cs
    │   │   └── CreateCandidateEndpoint.cs
    │   │
    │   ├── GetCandidateProfile/
    │   └── UpdateCandidateProfile/
    │
    ├── Infrastructure/
    │   ├── CandidateDbContext.cs
    │   └── ...
    │
    └── Contracts/
        └── ...
```

Feature'lar mümkün olduğunca kendi davranışlarını bir arada tutmalıdır.

---

# 8. Module List

Başlangıç mimarisinde aşağıdaki modüller bulunmaktadır (bkz. §6 Solution Structure ile aynı sıra).

## 8.1. Identity

Sorumlulukları:

* Authentication
* User
* Role
* Permission
* Login
* Registration
* Password management
* Token management
* Account status
* Security-related identity operations

---

## 8.2. Candidate

Sorumlulukları:

* Candidate
* CandidateProfile
* CV
* Personal information
* Candidate preferences
* Candidate status
* Candidate profile lifecycle

---

## 8.3. CareerAdvisor

Sorumlulukları:

* CareerAdvisor
* Candidate assignment
* Employer assignment
* Advisor workload
* Advisor notes
* Advisor activities

---

## 8.4. CareerDevelopment

Sorumlulukları:

* SkillGap
* CareerGoal
* DevelopmentPlan
* TrainingRecommendation
* AdvisorRecommendation

CareerDevelopment ayrı bir modül olarak Candidate ve CareerAdvisor'dan bağımsızdır (bkz. ADR-011). `TrainingRecommendation`, eğitimin kendisini (bkz. 8.11 Website) değil, adaya yapılan öneriyi temsil eder; Website.Training verisini kopyalamaz, yalnızca contract/ID referansı tutar.

---

## 8.5. Job

Sorumlulukları:

* Job
* Job listing
* Job lifecycle
* Job approval
* Publication
* Job requirements

Job lifecycle:

```text
Draft
  ↓
Submitted
  ↓
UnderReview
  ├── Approved
  │      ↓
  │   Published
  │
  └── Rejected
         ↓
  RevisionRequested
```

---

## 8.6. Matching

Sorumlulukları:

* Candidate matching
* Employer matching
* Job matching
* Skill matching
* Recommendation
* Match score
* Matching criteria

Matching algoritmaları ileride AI/ML tabanlı hale getirilebilir.

---

## 8.7. Employer

Sorumlulukları:

* Employer
* Company
* CompanyProfile
* Company approval
* Company users
* Company status

---

## 8.8. Interview

Sorumlulukları:

* Interview request
* Scheduling
* Interview participants
* Interview status
* Interview result
* Interview notes
* Candidate/employer feedback

---

## 8.9. Employment

Sorumlulukları:

* Employment
* Hiring information
* Employment status
* Start date
* Employment continuity
* Employment follow-up
* Employment outcome

---

## 8.10. Notification

Sorumlulukları:

* Email
* SMS
* Push notification
* In-app notification
* Notification templates
* Notification preferences

Notification, ADR-013 gereği ADR-010'daki sıradan öne çekilmiştir: Identity'nin
email doğrulama ve şifre sıfırlama akışları placeholder olmadan gerçekten
çalışabilsin diye, Domain Modules'ten önce hayata geçirilmiştir.

Kendi database'inde (`GenclikMerkezi.Notification`) `EmailNotification`
aggregate'i ile hangi e-postanın kime, ne zaman, hangi sonuçla (Sent/Failed)
gönderildiğini kaydeder — bu, gönderim başarısız olduğunda görünürlük
sağlayan bir audit trail'dir (AGENTS.md §38).

Diğer modüllerin Outbox'ına (ADR-006) yazdığı integration event'leri
CAP tabanlı `[CapSubscribe]` consumer'larla tüketir (ADR-014). Kendi
HTTP endpoint'i yoktur; tetiklendiği tek yer, event'i üreten modülün
kendi akışıdır (örn. Identity'nin Register/ForgotPassword'ü).

---

## 8.11. Website

Website, önceki tasarımda ayrı modüller olarak planlanan **CMS**, **Event** ve **Media** sorumluluklarını tek bir modülde birleştirir (yönetim panelinin tek bir modülde toplanması kararı).

Sorumlulukları:

* Public website content: News, Announcements, Projects, Activities, Success stories, Static pages, Menus, SEO metadata
* Events: Training events, Workshops, Event registration, Event attendance
* Website'e ait medya/dosya metadata'sı: Upload yönetimi, dosya referansları, dosya erişim politikaları

Dosyanın fiziksel olarak nerede tutulduğu Website modülünün business logic'ine gömülmemelidir; depolama sağlayıcısına özgü detaylar AGENTS.md §34'teki `IFileStorage` soyutlaması arkasında tutulur. Bu, her modülün kendi dosyalarını (ör. Candidate'ın CV dosyaları) aynı paylaşılan storage abstraction'ı kullanarak, ancak kendi veri sahipliğinde tutmasını engellemez — Website yalnızca kendi içerik/etkinlik/medya verisinin sahibidir, sistemdeki her dosyanın değil.

---

## 8.12. Support

Sorumlulukları:

* Müşteri desteği / destek talebi (ticket) yönetimi
* Support ticket lifecycle (açık, işlemde, çözüldü, kapatıldı)
* Candidate/Employer ile Admin/CareerAdvisor arasındaki destek iletişimi
* SSS/bilgi tabanı (ileride genişletilebilir)

---

## 8.13. ReferenceData

Sistem içerisinde birçok modül tarafından kullanılabilecek standart referans verilerini yönetir.
Kapsam, seed/admin-managed ayrımı ve cross-module erişim yöntemi **ADR-016**'da karara bağlanmıştır.

İki kategori (ADR-016 Decision 1):

```text
SEED (migration ile gelir, admin CRUD yok, sadece GET)
├── Country
├── Province   ("İl")
├── District   ("İlçe", Province'e bağlı)
└── Language

ADMIN-MANAGED (seed başlangıç verisiyle gelir, admin CRUD var)
├── Sector, Position, Department
├── WorkLocationType, EmploymentType
├── EducationLevel, SchoolCategory, DiplomaGradingSystem
├── Gender, MilitaryStatus, DriversLicenseType
├── LanguageLevel, ExperienceLevel, Nationality
├── DisabilityCategory, ReferenceType
├── Currency, Skill (bu ikisi hariç hepsi başlangıç seed verisiyle gelir - Skill,
│   Candidate modülü geldiğinde doldurulacak şekilde bilinçli olarak boş bırakılmıştır)
└── TaxOffice  (Province'e bağlı, ProvinceId taşıdığı için kendi bespoke feature'ına
    sahiptir - diğerleri gibi tamamen generic CRUD pattern'ini kullanmaz)
```

ReferenceData, SharedKernel değildir.

---

# 9. SharedKernel

SharedKernel yalnızca gerçekten ortak olan teknik/domain yapılarını içermelidir.

Örnek:

```text
SharedKernel/
├── Domain/
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   ├── ValueObject.cs
│   └── DomainEvent.cs
│
├── Results/
│   ├── Result.cs
│   ├── Error.cs
│   ├── PagedRequest.cs
│   └── PagedResult.cs
│
└── Abstractions/
    └── ...
```

## 9.1. Pagination Convention

Her sayfalanmış liste sorgusu (mevcut ve gelecekteki tüm modüller — Identity'nin admin kullanıcı
listesi/audit log'u ve ReferenceData'nın 23 lookup tipi ilk kullanıcılarıdır, ama module-specific
değildir) aynı iki tipi kullanır:

* **`PagedRequest`** (`SharedKernel.Results`) — `Page` (1-index, min 1) ve `PageSize` (varsayılan
  20, min 1, max 100). Sınır dışı bir değer **hata fırlatmaz, sessizce clamp edilir**
  (PERFORMANCE.md §11'in "pageSize server-side maksimum değerle sınırlandırılmalı" kuralı böyle
  uygulanır). `sealed` değildir: bir feature'ın kendi Query/Request tipi, ek filtre alanları
  taşımak için ondan türeyebilir — filtreler `PagedRequest`'e değil, o feature'ın kendi tipine
  eklenir (aşağıdaki örnek).
* **`PagedResult<TItem>`** (`SharedKernel.Results`) — `Items`, `TotalCount`, `Page`, `PageSize`,
  hesaplanan `TotalPages`, `HasNextPage`, `HasPreviousPage`. Küçük bir liste (örn. 10 kayıtlı bir
  lookup) de aynı şekli döner — sadece `TotalPages=1` olur; tüketen taraf tek bir sözleşmeye
  güvenebilir.

Sorguyu gerçek DB verisine çeviren taraf **`QueryablePagingExtensions.ToPagedResultAsync`**
(`GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence`) — bir `IQueryable<T>` üzerinde
`Count` + `Skip`/`Take` uygulayıp `PagedResult<T>` döndürür. Bu extension **SharedKernel'de değil**,
bilinçli olarak `BuildingBlocks.Infrastructure`'dadır: SharedKernel her modülün Domain'i tarafından
referans edilir, ve AGENTS.md §16/§18 `IQueryable<T>`'i ve EF Core'u bir modülün kendi
`Infrastructure`'ı dışında yasaklar (`FeatureDbContextTests` bunu otomatik doğrular) — SharedKernel'e
EF Core bağımlılığı eklemek bu kuralı proje genelinde ihlal ederdi. Bu yüzden `ToPagedResultAsync`
her zaman bir **repository/reader**'ın (Feature handler'ın değil) içinde çağrılır; handler'a yalnızca
materialize edilmiş `PagedResult<T>` döner (bkz. `UserRepository.SearchAsync`,
`ReferenceDataLookupReader.ListByParentAsync`).

**Yeni bir modülün liste endpoint'i yazarken** (`GetDistrictsQuery` — ReferenceData'nın District'e
`ProvinceId` filtresi eklediği feature — tam olarak bu örnektir):

```csharp
// Features/GetBlogPosts/GetBlogPostsQuery.cs
public sealed record GetBlogPostsQuery(Guid? CategoryId, string? SearchTerm)
    : PagedRequest, IRequest<Result<PagedResult<BlogPostSummary>>>;

// Features/GetBlogPosts/GetBlogPostsQueryHandler.cs
public sealed class GetBlogPostsQueryHandler(IBlogPostRepository repository)
    : IRequestHandler<GetBlogPostsQuery, Result<PagedResult<BlogPostSummary>>>
{
    public async Task<Result<PagedResult<BlogPostSummary>>> Handle(
        GetBlogPostsQuery request, CancellationToken cancellationToken) =>
        Result.Success(await repository.SearchAsync(request, cancellationToken));
}

// Infrastructure/Persistence/BlogPostRepository.cs (Infrastructure - EF Core/IQueryable serbest)
public async Task<PagedResult<BlogPostSummary>> SearchAsync(
    GetBlogPostsQuery filter, CancellationToken cancellationToken)
{
    var query = dbContext.BlogPosts.AsNoTracking();

    if (filter.CategoryId is not null)
    {
        query = query.Where(p => p.CategoryId == filter.CategoryId);
    }

    return await query
        .OrderByDescending(p => p.PublishedAtUtc)
        .Select(p => new BlogPostSummary(p.Id, p.Title, p.PublishedAtUtc))
        .ToPagedResultAsync(filter, cancellationToken);
}
```

Endpoint tarafı, `page`/`pageSize`'ı diğer query parametreleriyle birlikte okuyup Query'yi object
initializer ile kurar (bkz. `AdminGetUsersEndpoint`, `GetDistrictsEndpoint`):

```csharp
var query = new GetBlogPostsQuery(categoryId, searchTerm) { Page = page ?? 1, PageSize = pageSize ?? PagedRequest.DefaultPageSize };
```

SharedKernel içerisine:

* Candidate
* Employer
* Job
* Country
* City
* School
* Skill
* TaxOffice
* Business-specific service

eklenmemelidir.

Temel prensip:

> **SharedKernel ortak kullanılan her şey değildir; gerçekten ortak olan temel yapıların evidir.**

---

# 10. Database Architecture

## 10.1. Database-per-Module

Her modül kendi MSSQL database'ine sahip olacaktır.

Örnek:

```text
GenclikMerkezi.Identity
GenclikMerkezi.Candidate
GenclikMerkezi.CareerAdvisor
GenclikMerkezi.CareerDevelopment
GenclikMerkezi.Job
GenclikMerkezi.Matching
GenclikMerkezi.Employer
GenclikMerkezi.Interview
GenclikMerkezi.Employment
GenclikMerkezi.Notification
GenclikMerkezi.Website
GenclikMerkezi.Support
GenclikMerkezi.ReferenceData
```

## 10.2. Database Isolation

Bir modül başka modülün database'ine doğrudan erişemez.

Yasak:

```text
Candidate
   ↓
EmployerDbContext
```

Yasak:

```text
Job
   ↓
SELECT FROM CandidateDatabase
```

Yasak:

```text
Matching
   ↓
CandidateDbContext
```

Bunun yerine:

```text
Matching
   ↓
Candidate Contracts
   ↓
Candidate API/Application boundary
```

veya uygun durumda:

```text
Domain Event
       ↓
Integration Event
       ↓
RabbitMQ
       ↓
Matching
```

kullanılır.

---

# 11. DbContext Strategy

Her modül kendi DbContext'ine sahip olacaktır.

Örnek:

```text
CandidateDbContext
EmployerDbContext
JobDbContext
MatchingDbContext
InterviewDbContext
EmploymentDbContext
```

Bir modül yalnızca kendi DbContext'ini kullanabilir.

Örnek:

```csharp
public sealed class CandidateDbContext : DbContext
{
}
```

Başka modülün DbContext'i dependency olarak inject edilmemelidir.

## 11.1. Keyed IUnitOfWork (önemli, tekrar tekrar keşfedilmesin)

Her modül kendi `SharedKernel.Abstractions.IUnitOfWork`'ünü **keyed service**
olarak kaydetmelidir (`AddKeyedScoped<IUnitOfWork>(ModuleMarker.UnitOfWorkKey, ...)`),
handler'lar da `[FromKeyedServices(ModuleMarker.UnitOfWorkKey)]` ile
enjekte etmelidir — düz (unkeyed) `AddScoped<IUnitOfWork>(...)` **kullanılmamalıdır**.

Bunun nedeni bir varsayım değil, gerçekte yaşanmış bir hatadır: Identity ve
Notification aynı anda düz `IUnitOfWork` kaydettiğinde, DI container'ın
"son kayıt kazanır" davranışı yüzünden Identity'nin handler'ları sessizce
`NotificationDbContext`'i almaya başladı — Register/Login gibi endpoint'ler
200/201 döndürmeye devam etti ama hiçbir şey kalıcı olarak kaydedilmedi.
Bu, entegrasyon testinde "Register sonrası Login başarısız oluyor" şeklinde
ortaya çıktı ve keyed service'e geçilerek düzeltildi. Yeni bir modül
eklerken bu deseni takip etmemek aynı sessiz veri kaybını yeniden üretir.

---

# 12. Cross-Module Data

Modüller birbirlerinin entity'lerini doğrudan kullanamaz.

Örneğin Job modülü:

```text
Candidate entity
```

kullanmak yerine:

```text
CandidateId
```

veya gerekli durumda:

```text
CandidateSummary
```

gibi contract/read model kullanmalıdır.

Temel prensip:

> **Entity paylaşma, contract paylaş.**

---

# 13. Cross-Module References

Modüller arasında mümkün olduğunca fiziksel foreign key oluşturulmayacaktır.

Örneğin:

```text
Job.CandidateId
```

Candidate database'ine fiziksel FK oluşturmak zorunda değildir.

Bu ID bir business reference olarak tutulabilir.

Böylece modüllerin database bağımsızlığı korunur.

---

# 14. ReferenceData Access

ReferenceData merkezi bir modüldür.

Örneğin Candidate:

```text
CityId
DistrictId
SchoolId
SkillId
```

gibi referanslar tutabilir.

Ancak Candidate doğrudan:

```text
ReferenceDataDbContext
```

kullanmamalıdır.

**Somut çözüm (ADR-016 Decision 2):** `GenclikMerkezi.Contracts.ReferenceData` içinde yayınlanan
`IReferenceDataLookupReader` arayüzü - `ExistsAndActiveAsync` (yazma-zamanı validasyon için),
`ListAsync` ve `ListByParentAsync` (District/TaxOffice gibi Province'e bağlı olanlar için).
Implementasyonu `ReferenceData.Infrastructure`'da yaşar ve host composition root'ta bir kez
register edilir; tüketen modüller bunu doğrudan enjekte eder - **network çağrısı değil, in-process
bir metot çağrısıdır** (her modül aynı process içinde çalışır, ID+snapshot pattern'i de
kullanılmaz - ADR-016'da gerekçesiyle birlikte reddedilmiştir). ReferenceData modülü kendi ayrıca
extraction edilirse, değişmesi gereken tek şey bu arayüzün implementasyonudur.

ReferenceData verileri yüksek oranda cache'lenebilir - `ListAsync`/`ListByParentAsync` zaten
`IMemoryCache` ile cache'lidir, admin-managed CRUD her mutation'da ilgili cache key'i invalidate
eder.

Örnek:

```text
Country
City
District
Sector
Profession
Skill
```

---

# 15. Application Layer

Application katmanı:

* Use case
* Command
* Query
* Handler
* Validation
* Authorization
* DTO
* Application contract

işlerinden sorumludur.

Application katmanı business workflow'u orkestre eder.

Örnek:

```text
CreateCandidateCommand
        ↓
CreateCandidateHandler
        ↓
Candidate Aggregate
        ↓
CandidateDbContext
```

---

# 16. Domain Layer

Domain katmanı business kurallarının evidir.

Domain:

* Entity
* Aggregate
* Value Object
* Domain Event
* Business Rule

içerir.

Domain katmanı:

* EF Core
* RabbitMQ
* HTTP
* SMTP
* File system
* Hangfire
* Redis

gibi infrastructure teknolojilerine bağımlı olmamalıdır.

Temel kural:

> Domain, infrastructure'ı bilmez.

---

# 17. Infrastructure Layer

Infrastructure:

* EF Core
* MSSQL
* RabbitMQ
* Email
* SMS
* File Storage
* External API
* Cache
* Logging
* Persistence

gibi teknik implementasyonları içerir.

Infrastructure business rule barındırmamalıdır.

---

# 18. Vertical Slice Architecture

Feature'lar mümkün olduğunca dikey dilimler halinde organize edilir.

Örneğin:

```text
Features/
└── CreateCandidate/
    ├── CreateCandidateCommand.cs
    ├── CreateCandidateHandler.cs
    ├── CreateCandidateValidator.cs
    └── CreateCandidateEndpoint.cs
```

Devasa:

```text
Services/
Repositories/
Dtos/
Validators/
Controllers/
```

klasörleri altında bütün sistemi toplamak tercih edilmez.

---

# 19. CQRS

Sistemde Command ve Query ayrımı uygulanacaktır.

## Command

Veriyi değiştirir.

Örnek:

```text
CreateCandidate
UpdateCandidateProfile
ApproveEmployer
CreateJob
ApproveJob
ScheduleInterview
CreateEmployment
```

## Query

Veri okur.

Örnek:

```text
GetCandidateProfile
GetCandidateJobs
GetEmployerProfile
GetAdvisorCandidates
GetRecommendedCandidates
```

Query'ler mümkün olduğunca read-only olmalıdır.

---

# 20. MediatR

Application seviyesinde Command ve Query dispatch işlemleri için MediatR kullanılabilir.

Örnek:

```text
Endpoint
   ↓
MediatR
   ↓
Command / Query
   ↓
Handler
```

MediatR bir business layer değildir.

Business logic Handler'a kontrolsüz şekilde yığılmamalıdır.

---

# 21. Repository Strategy

Generic Repository kullanılmayacaktır.

Yasak:

```text
IRepository<T>
```

Yasak:

```text
GenericRepository<T>
```

Repository gerekiyorsa domain/use-case ihtiyacına göre oluşturulmalıdır.

Örnek:

```text
ICandidateRepository
IJobRepository
```

Ancak EF Core DbContext'in zaten yeterli abstraction sağladığı durumlarda gereksiz repository oluşturulmamalıdır.

## 21.1 Value Object'ler Üzerinde Filtreleme/Arama (EF Core Gotcha)

Bir property `HasConversion(vo => vo.Value, ...)` ile bir Value Object'e (örn. `Email`) map
edildiğinde:

* `u.Email == someEmail` gibi tam eşitlik karşılaştırmaları düzgün translate edilir.
* `u.Email.Value` gibi converted property üzerine encaklenmiş bir member access **translate
  edilmez** — `Where` içinde "could not be translated" hatası, `Select` içinde ise
  `InvalidCastException` fırlatır (bkz. `IUserRepository.SearchAsync`'in `AdminGetUsers` için
  yazılan implementasyonu).

Bunun yerine: diğer (translate edilebilir) filtreler DB tarafında uygulanır, ardından `u.Id` +
`u.Email` (whole property, normal converter path) projekte edilip materialize edilir, substring
eşleşmesi client-side yapılır, ve eşleşen id'ler `Contains` ile son sorguya (count + pagination)
geri beslenir. `EF.Property<string>(...)` bir `Where` predicate'inde translate olur ama
materialization sırasında (`Select`) aynı `InvalidCastException`'ı verir — güvenilir bir çözüm
değildir.

Temel prensip:

> **Abstraction ihtiyaçtan doğar; alışkanlıktan oluşturulmaz.**

---

# 22. Domain Events

Domain Events aynı bounded module içerisindeki business reaksiyonları için kullanılabilir.

Örnek:

```text
CandidateAssessmentCompleted
        ↓
SkillGapDetected
        ↓
TrainingRecommendationCreated
```

Domain event'ler domain business olaylarını ifade eder.

---

# 23. Integration Events

Modüller arasında veya external system'lere bilgi aktarmak için Integration Event kullanılabilir.

Örnek:

```text
JobApproved
CandidateRecommended
InterviewScheduled
EmploymentCreated
```

Integration Event'ler module boundary'lerini korumalıdır.

---

# 24. RabbitMQ

RabbitMQ asynchronous integration communication için kullanılacaktır.

Örnek:

```text
Candidate Module
      ↓
CandidateProfileCompleted
      ↓
Outbox
      ↓
RabbitMQ
      ↓
Matching Module
```

RabbitMQ:

* Modüller arası asynchronous communication
* Notification
* Background processing
* External integrations

gibi ihtiyaçlarda kullanılabilir.

Her işlem için RabbitMQ kullanmak zorunlu değildir.

Basit ve aynı transaction içerisinde çözülebilecek işlemler gereksiz şekilde asynchronous yapılmamalıdır.

---

# 25. Outbox Pattern

Database transaction ile event publication arasında güvenilirlik sağlamak için Outbox Pattern kullanılacaktır.

Örnek:

```text
Business Transaction
       │
       ├── Domain Data
       │
       └── Outbox Message
                ↓
          Commit Transaction
                ↓
        Background Publisher
                ↓
            RabbitMQ
```

Amaç:

> Database commit oldu fakat event yayınlanamadı problemini önlemek.

Outbox mesajları idempotent şekilde işlenmelidir.

---

# 26. Background Jobs

Zamanlanmış ve uzun süren işler için Hangfire kullanılabilir.

Örnek:

* Notification gönderimleri
* Reminder
* Outbox processing
* Periodic reports
* Data synchronization
* Scheduled cleanup
* Follow-up reminders

Background job'lar idempotent tasarlanmalıdır.

---

# 27. Authentication

Authentication merkezi olarak Identity modülünde yönetilecektir.

Frontend'ler:

```text
Public Website
Candidate Portal
Employer Portal
Management Portal
```

aynı backend authentication altyapısını kullanabilir.

Authentication ile Authorization birbirinden ayrılmalıdır.

## 27.1 Access Token Ömrü ve Deactivate Sonrası Erişim Penceresi

Access token (JWT) ömrü 15 dakikadır (`Jwt:AccessTokenExpirationMinutes`), refresh token ömrü
ayrıdır ve değişmemiştir. JWT stateless olduğundan, bir kullanıcı Admin tarafından deactivate
edildiğinde elindeki access token — henüz süresi dolmamışsa — en fazla 15 dakika daha geçerli
kalabilir (bkz. SECURITY.md, "Bilinen Sınırlar"). Bu, bilinçli olarak kabul edilmiş bir sınırdır;
refresh token tarafı zaten `Status != Active` kontrolüyle kapalıdır (`Login`,
`RefreshAccessToken`), yani erişim en geç bir refresh denemesinde veya access token'ın doğal
süresi dolduğunda kesilir. Kullanıcı sayısı/ihtiyaç büyürse (örn. anlık erişim kesme zorunluluğu
doğarsa) bir revocation mekanizması (security stamp, token blacklist, kısa ömürlü + sık
refresh gibi) eklenebilir; şu an için bu karmaşıklık gerekli görülmemiştir.

---

# 28. Authorization

Authorization:

```text
Role
+
Permission
+
Resource Authorization
```

üzerinden uygulanmalıdır.

Örnek:

```text
Admin
Candidate
Employer
CareerAdvisor
```

Roller temel erişim modelidir.

Permission sistemi daha detaylı yetkilendirme sağlar.

Örnek:

```text
Candidate.Read
Candidate.Update
Employer.Read
Employer.Approve
Job.Create
Job.Approve
Interview.Manage
Employment.Read
```

---

# 29. Resource Authorization

Sadece role bakmak yeterli değildir.

Örneğin CareerAdvisor:

```text
Candidate.Read
```

yetkisine sahip olabilir.

Ancak yalnızca kendisine atanmış adayları görmesi gerekiyorsa resource authorization uygulanmalıdır.

```text
CareerAdvisor
       ↓
Assigned Candidate
       ↓
Access Granted
```

Başka danışmanın adayına erişim verilmemelidir.

---

# 30. API Architecture

API:

```text
/api/v1/...
```

formatında versioned olacaktır.

Örnek:

```text
/api/v1/candidates
/api/v1/employers
/api/v1/jobs
/api/v1/interviews
/api/v1/employments
```

API response'larında uygun HTTP status code kullanılmalıdır.

Hatalar standardize edilmiş ProblemDetails formatında dönmelidir.

---

# 31. Endpoint Organization

Controller'lar devasa business layer haline getirilmemelidir.

Tercih edilen yapı:

```text
Endpoint
   ↓
Command / Query
   ↓
Handler
   ↓
Domain / Infrastructure
```

Minimal API veya Controller yaklaşımı proje standardına göre kullanılabilir.

Önemli olan endpoint'in ince tutulmasıdır.

---

# 32. Validation

Input validation için FluentValidation kullanılacaktır.

Validation:

```text
Request
   ↓
Validator
   ↓
Handler
```

şeklinde çalışmalıdır.

Validation business rule ile karıştırılmamalıdır.

Örneğin:

```text
Email boş mu?
```

validation'dır.

```text
Bu aday bu statüdeyken işe alınabilir mi?
```

domain business rule'dur.

---

# 33. Exception Handling

Global exception handling uygulanacaktır.

API:

```text
Exception
    ↓
Global Exception Handler
    ↓
ProblemDetails
    ↓
HTTP Response
```

kullanmalıdır.

Stack trace production response içerisinde gösterilmemelidir.

---

# 34. Caching

Başlangıçta:

```text
IMemoryCache
```

kullanılabilir.

Cache için uygun adaylar:

* ReferenceData
* Country
* City
* District
* Sector
* Profession
* Skill
* Website content
* Frequently accessed configuration

Gerektiğinde Redis'e geçilebilir.

Redis ilk günden zorunlu değildir.

## 34.1. Caching Convention

Yukarıdaki `IMemoryCache` prensibinin somut, tek soyutlaması (ADR-017): her modül, kendi cache
ihtiyacı için doğrudan `IMemoryCache` enjekte etmek yerine `SharedKernel.Abstractions.ICacheService`
kullanır.

```text
SharedKernel/
└── Abstractions/
    ├── ICacheService.cs          (GetOrCreateAsync/Remove/RemoveByPrefix — global/module-level cache)
    ├── ICurrentUserContext.cs    (Guid? UserId — sadece IUserScopedCacheService'in ihtiyacı kadar)
    └── IUserScopedCacheService.cs (GetOrCreateForCurrentUserAsync — kullanıcıya-özel cache)
```

İmplementasyonları (`MemoryCacheService`, `UserScopedCacheService`, `HttpContextCurrentUserContext`)
`BuildingBlocks.Infrastructure`'da yaşar, Host composition root'ta `AddCaching(configuration)` ile
bir kez register edilir (`Program.cs`) — hiçbir modül kendi `IMemoryCache`/`ICacheService` kaydını
yapmaz. **Redis'e geçiş ihtiyacı doğarsa** sadece `MemoryCacheService`'in yerini `RedisCacheService`
gibi başka bir implementasyon alır; `ICacheService`'i tüketen hiçbir kod değişmez (`IReferenceDataLookupReader`in
ADR-016 Decision 2'deki "sadece implementasyon değişir" deseninin aynısı).

**Ne zaman global (`ICacheService`), ne zaman kullanıcıya-özel (`IUserScopedCacheService`):**

```text
Veri her kullanıcı için aynıysa (referans veri, herkese açık liste, konfigürasyon)
    → ICacheService doğrudan

Veri çağıran kullanıcıya özelse (örn. "bu adayın kendi önerilen iş ilanları",
"bu işverenin kendi bekleyen başvuruları")
    → IUserScopedCacheService (key'i otomatik "user:{userId}:{key}" yapar)
```

`IUserScopedCacheService`, mevcut kullanıcı yoksa (anonim istek) **cache'lemeden** doğrudan factory'yi
çağırır — bir anonim isteğin sonucu başka bir anonim çağırana asla sızmaz.

**Örnek (gerçek kullanım — `ReferenceDataLookupReader.ListAsync`):**

```csharp
public Task<PagedResult<LookupItemSummary>> ListAsync(
    ReferenceDataLookupType type, PagedRequest paging, bool activeOnly, CancellationToken cancellationToken)
{
    var cacheKey = ReferenceDataCacheKeys.List(type, activeOnly, paging); // page/pageSize key'e dahil
    var ttl = IsSeedType(type) ? TimeSpan.FromHours(24) : null;           // null → servisin configure edilebilir default'u

    return cacheService.GetOrCreateAsync(cacheKey, async ct => await QueryFromDbAsync(...), ttl, cancellationToken);
}
```

Sayfalanmış bir sonucu cache'lerken **sayfa/filtre parametreleri key'e dahil edilmelidir** (yukarıdaki
gibi) — aksi halde farklı `page`/`pageSize`/filtre kombinasyonları birbirinin cache girdisinin üstüne
yazar. Bir mutation sonrası **o tipin tüm key'lerini** temizlemek için `ICacheService.RemoveByPrefix`
kullanılır (`LookupCacheInvalidator.Invalidate` örneği) — tek tek her sayfa/filtre kombinasyonunun
key'ini bilmeye gerek yoktur.

---

# 35. File Storage

Dosya sistemi business logic içerisine gömülmemelidir.

Abstraction:

```text
IFileStorage
```

üzerinden oluşturulabilir.

Örneğin:

```text
Media
  ↓
IFileStorage
  ↓
Local / S3 / Azure / Cloudflare / Other Provider
```

Provider değişikliği business logic'i etkilememelidir.

Dosya erişimi authorization kurallarına tabi olmalıdır.

---

# 36. Notification Architecture

Notification modülü aşağıdaki kanalları destekleyecek şekilde tasarlanacaktır:

```text
Email
SMS
Push
In-App
```

Örnek:

```text
Job Approved
      ↓
Notification Event
      ↓
Notification Module
      ├── Email
      ├── SMS
      └── In-App
```

Notification provider'ları abstraction arkasında tutulmalıdır.

---

# 37. Observability

Sistem gözlemlenebilir olacak şekilde tasarlanacaktır.

Kullanılacak temel yapı:

* Structured Logging
* OpenTelemetry
* Trace
* Metrics
* Health Checks

Health endpoints:

```text
/health/live
/health/ready
```

Live:

> Uygulama çalışıyor mu?

Ready:

> Uygulama gerekli dependency'ler ile çalışmaya hazır mı?

---

# 38. Logging

Loglarda:

* User password
* Access token
* Refresh token
* Sensitive personal data
* Secret
* API key

gibi bilgiler kesinlikle yazılmamalıdır.

Structured logging tercih edilmelidir.

Her önemli işlem mümkün olduğunca correlation/trace bilgisi taşımalıdır.

---

# 39. Security

Security sistemin sonradan eklenecek bir özelliği değildir.

Security by Design uygulanacaktır.

Temel prensipler:

* Least privilege
* Input validation
* Authentication
* Authorization
* Resource authorization
* Secure password storage
* Secret management
* Rate limiting
* File access control
* Audit logging
* Secure headers
* HTTPS
* Sensitive data protection

---

# 40. Secrets

Secret değerler source code içerisine yazılmayacaktır.

Yasak:

```csharp
const string Password = "...";
```

Yasak:

```text
appsettings.json
```

içerisinde production secret tutulması.

Environment variables veya uygun secret management sistemi kullanılmalıdır.

---

# 41. Audit

Aşağıdaki gibi kritik işlemler audit edilebilir:

* Employer approval
* Candidate approval
* Job approval/rejection
* Permission changes
* Employment creation
* Interview result changes
* User status changes
* Administrative operations

Audit mekanizması gerektiğinde merkezi veya ilgili modül içerisinde uygulanabilir.

Identity modülünün admin işlemleri (`ManuallyUnlock`, `Deactivate`, `Reactivate`, `ChangeUserRole`)
artık `AdminAuditLog` tablosuna (Identity'nin kendi şemasında) kalıcı, sorgulanabilir bir kayıt
yazar — bkz. **ADR-015**. Yazma işlemi, ilgili komut handler'larına serpiştirilmek yerine, o
handler'ların zaten fırlattığı domain event'lere reaksiyon veren dört ayrı
`INotificationHandler<DomainEventNotification<T>>` üzerinden yapılır (Identity'nin ilk domain
event handler'ları). `GET /api/v1/auth/admin/audit-log` (Admin-only, sayfalı, TargetUserId/
ActionType/tarih aralığına göre filtrelenebilir) bu kaydı okur. SECURITY.md §8/§22'nin admin
işlemleri için istediği audit gereksinimi bu dört işlem için artık karşılanmıştır; yalnızca
`ILogger` structured log'a dayanan eski yaklaşım (hâlâ ek olarak üretilir) tek başına yeterli
değildi.

---

# 42. Transaction Strategy

Transaction sınırı mümkün olduğunca modül içerisindedir.

Örnek:

```text
Candidate DB Transaction
```

içerisinde:

```text
Candidate
CandidateProfile
CandidateSkill
```

gibi Candidate modülüne ait veriler birlikte güncellenebilir.

Modüller arasında distributed transaction tercih edilmez.

---

# 43. Distributed Transaction

Aşağıdaki yaklaşım tercih edilmez:

```text
Candidate DB
     +
Employer DB
     +
Job DB
```

tek transaction içerisinde commit edilmeye çalışılmamalıdır.

Bunun yerine:

```text
Local Transaction
       ↓
Domain Event
       ↓
Outbox
       ↓
RabbitMQ
       ↓
Other Module
```

kullanılmalıdır.

---

# 44. Consistency Model

Modül içerisindeki transaction'larda strong consistency hedeflenir.

Modüller arasındaki communication için gerektiğinde eventual consistency kabul edilir.

Örneğin:

```text
Employer Approved
       ↓
Event
       ↓
Matching
```

Matching verisinin birkaç saniye sonra güncellenmesi kabul edilebilir.

---

# 45. Matching Architecture

Matching sistemin önemli domain'lerinden biridir.

Matching:

```text
Candidate
+
Skills
+
Experience
+
Education
+
Career Goals
+
Location
+
Work Preferences
+
Employer Requirements
+
Job Requirements
```

gibi verileri kullanabilir.

Ancak Candidate veya Employer database'lerine doğrudan erişemez.

İhtiyaç duyduğu verileri contract/read model/event mekanizmaları üzerinden almalıdır.

Gelecekte:

```text
Rule Based Matching
        ↓
Scoring
        ↓
Advanced Matching
        ↓
AI Assisted Matching
```

şeklinde geliştirilebilir.

AI entegrasyonu domain modeline zorunlu bağımlılık haline getirilmemelidir.

---

# 46. Reporting

Reporting başlangıçta ilgili modüllerden gerekli verileri okuyacak şekilde tasarlanabilir.

Reporting için production transactional database'lere ağır rapor sorguları yüklenmemelidir.

İhtiyaç büyüdüğünde:

```text
Operational DB
      ↓
Events
      ↓
Reporting Read Model
```

veya ayrı analytical database yaklaşımına geçilebilir.

---

# 47. Testing Architecture

Üç temel test seviyesi kullanılacaktır.

```text
UnitTests
IntegrationTests
ArchitectureTests
```

## Unit Tests

Test eder:

* Domain rules
* Business logic
* Validators
* Matching logic
* Application handlers

---

## Integration Tests

Test eder:

* API
* Database
* EF Core
* Authentication
* Authorization
* RabbitMQ integrations
* Outbox
* External integrations

---

## Architecture Tests

Mimari kuralları otomatik olarak kontrol eder.

Örnek:

```text
Domain
  ❌ Infrastructure dependency

Candidate
  ❌ Employer.Infrastructure dependency

Job
  ❌ Candidate.DbContext dependency
```

Architecture testleri bu ihlalleri build/test aşamasında yakalamalıdır.

---

# 48. Dependency Rules

Temel bağımlılık yönü:

```text
Domain
   ↑
Application
   ↑
Infrastructure
   ↑
Host
```

Daha doğru ifadeyle:

```text
Domain
  ← Application
  ← Infrastructure
  ← Host
```

Domain dış dünyayı bilmez.

---

# 49. Module Dependency Rules

Modüller birbirlerinin implementation detaylarına bağımlı olamaz.

Yasak:

```text
Candidate
    ↓
Employer.Infrastructure
```

Yasak:

```text
Matching
    ↓
Candidate.Infrastructure
```

Yasak:

```text
Job
    ↓
CandidateDbContext
```

İzin verilen:

```text
Module
   ↓
Contract
```

ve:

```text
Module
   ↓
Event
```

---

# 50. Contracts

Contracts modüller arası iletişim için kullanılabilir.

Örneğin:

```text
Candidate.Contracts
Employer.Contracts
Job.Contracts
Interview.Contracts
Employment.Contracts
```

Contract'lar implementation detaylarını expose etmemelidir.

Entity paylaşmak yerine DTO/contract paylaşılmalıdır.

---

# 51. Naming Rules

Her dosya kendi sorumluluğunu ve tipini ifade etmelidir.

Tercih edilen:

```text
UserRole.cs
UserPermission.cs
CandidateStatus.cs
CreateCandidateCommand.cs
CreateCandidateHandler.cs
CreateCandidateValidator.cs
CreateCandidateEndpoint.cs
```

Kaçınılacak:

```text
Enums.cs
Dtos.cs
Models.cs
Entities.cs
Services.cs
Helpers.cs
Utilities.cs
Common.cs
```

Tek bir dosya onlarca farklı type içeren bir "toplama dosya" haline getirilmemelidir.

---

# 52. Dependency Injection

Dependency Injection kullanılacaktır.

Service Locator yaklaşımı kullanılmayacaktır.

Yasak:

```text
IServiceProvider.GetService(...)
```

uygulama business logic'i içerisinde kontrolsüz şekilde kullanılmamalıdır.

Dependency'ler constructor üzerinden açıkça ifade edilmelidir.

---

# 53. Async Programming

I/O işlemleri asynchronous yapılmalıdır.

Tercih:

```csharp
Task
Task<T>
CancellationToken
```

Yasak:

```csharp
.Result
.Wait()
Thread.Sleep()
```

Async zinciri gereksiz şekilde synchronous hale getirilmemelidir.

---

# 54. CancellationToken

Uzun süren ve I/O yapan işlemlerde CancellationToken desteklenmelidir.

Özellikle:

* Database
* HTTP
* File operations
* External API
* Background operations

için cancellation propagation korunmalıdır.

---

# 55. API → Module Flow

Örnek:

```text
HTTP Request
     ↓
Endpoint
     ↓
Command / Query
     ↓
MediatR
     ↓
Handler
     ↓
Domain
     ↓
DbContext
     ↓
Module Database
```

---

# 56. Cross-Module Flow

Örnek:

```text
Employer
   ↓
PersonnelRequestCreated
   ↓
Employer Transaction
   ↓
Outbox
   ↓
RabbitMQ
   ↓
Matching
   ↓
Candidate Recommendations
```

---

# 57. Example: Job Approval Flow

```text
Employer
   ↓
Submit Job
   ↓
Job Module
   ↓
UnderReview
   ↓
CareerAdvisor
   ↓
Review
   ├── Approve
   │     ↓
   │   Published
   │
   └── Reject
         ↓
   RevisionRequested
```

Onay sonrası ilgili integration event'ler publish edilebilir.

---

# 58. Example: Employment Flow

```text
Candidate
     ↓
Recommendation / Application / Matching
     ↓
Interview
     ↓
Positive Result
     ↓
Employment
     ↓
EmploymentCreated
     ↓
Employment Follow-up
```

İstihdam sonrasında takip süreci ayrı bir lifecycle olarak yönetilir.

---

# 59. Performance Principles

Performance optimizasyonu gereksiz erken optimizasyon şeklinde yapılmamalıdır.

Öncelik:

```text
Correctness
>
Security
>
Architecture
>
Testability
>
Maintainability
>
Performance
>
Simplicity
>
Development Speed
```

Performans problemleri ölçülerek çözülmelidir.

---

# 60. Database Performance

Database sorgularında:

* Gereksiz Include kullanılmamalı
* Pagination uygulanmalı
* Projection tercih edilmeli
* N+1 sorguları önlenmeli
* Uygun index'ler oluşturulmalı
* Büyük veri setleri için uygun query stratejileri kullanılmalı

Liste endpoint'leri pagination olmadan sınırsız veri döndürmemelidir.

---

# 61. Scalability

Sistem başlangıçta tek API instance ile çalışabilir.

İhtiyaç arttığında:

```text
                    Load Balancer
                         │
             ┌───────────┼───────────┐
             ↓           ↓           ↓
           API 1       API 2       API 3
             │           │           │
             └───────────┼───────────┘
                         │
                      RabbitMQ
                         │
                    Module DBs
```

şeklinde yatay ölçeklenebilir.

Stateless API yaklaşımı tercih edilmelidir.

---

# 62. Future Microservice Extraction

Modüler mimarinin önemli avantajlarından biri ileride modül ayırabilmektir.

Örneğin Matching çok büyürse:

```text
Modular Monolith
       ↓
Matching Module
       ↓
Independent Matching Service
```

haline getirilebilir.

Bu dönüşümün kolay olması için:

* Database isolation
* Contract isolation
* Event-based communication
* Implementation isolation

başlangıçtan itibaren korunmalıdır.

---

# 63. Deployment

Başlangıçta deployment mümkün olduğunca sade tutulmalıdır.

Docker zorunlu değildir.

Deployment altyapısı:

```text
Frontend Applications
       +
ASP.NET Core API
       +
Module Databases
       +
RabbitMQ
       +
Hangfire
```

bileşenlerinden oluşur.

Containerization ileride eklenebilir.

---

# 64. Configuration

Environment'a göre configuration ayrılmalıdır.

Örnek:

```text
Development
Test
Staging
Production
```

Production secret'ları source control içerisinde tutulmamalıdır.

---

# 65. Health & Readiness

API'nin dependency durumunu kontrol etmek için health checks kullanılacaktır.

Örnek:

```text
/health/live
/health/ready
```

Readiness gerektiğinde:

* Database
* RabbitMQ
* External services

gibi dependency'leri kontrol edebilir.

---

# 66. Documentation

Dokümantasyon aşağıdaki şekilde organize edilir:

```text
docs/
├── PROJECT.md
├── ARCHITECTURE.md
├── DOMAIN.md
├── DEVELOPMENT.md
├── SECURITY.md
├── PERFORMANCE.md
└── DECISIONS/
```

Dokümanların görevleri:

```text
PROJECT.md
→ Ne yapıyoruz?

DOMAIN.md
→ İş kurallarımız ve domain nedir?

ARCHITECTURE.md
→ Teknik mimarimiz nasıl?

DEVELOPMENT.md
→ Günlük geliştirme süreci nasıl?

SECURITY.md
→ Güvenlik kurallarımız neler?

PERFORMANCE.md
→ Performans prensiplerimiz neler?

DECISIONS/
→ Bu mimari kararları neden aldık?
```

---

# 67. Architecture Decision Records

Önemli mimari kararlar ADR olarak tutulmalıdır.

Örnek:

```text
ADR-001-Modular-Monolith.md
ADR-002-Database-Per-Module.md
ADR-003-Frontend-Separation.md
ADR-004-Event-Driven-Communication.md
ADR-005-CQRS.md
```

ADR'ler:

* Kararı
* Alternatifleri
* Neden seçildiğini
* Trade-off'ları
* Sonuçlarını

açıklamalıdır.

---

# 68. Claude Execution Protocol

Claude herhangi bir kod değişikliği yapmadan önce aşağıdaki süreci izlemelidir.

### 1. AGENTS.md oku

Repository'nin genel kurallarını öğren.

### 2. İlgili modülü belirle

Task'ın hangi bounded module'e ait olduğunu belirle.

### 3. İlgili dokümanları oku

Gerekiyorsa:

```text
DOMAIN.md
ARCHITECTURE.md
SECURITY.md
PERFORMANCE.md
DEVELOPMENT.md
```

dokümanlarını incele.

### 4. Mevcut kodu incele

Kod yazmadan önce:

* Existing feature
* Existing abstraction
* Existing handler
* Existing validator
* Existing contract
* Existing database configuration

incelenmelidir.

### 5. Mevcut yapıyı tekrar kullan

Zaten bulunan bir abstraction'ın aynısı oluşturulmamalıdır.

### 6. En küçük güvenli değişikliği yap

Task kapsamı dışında refactor yapılmamalıdır.

### 7. Testleri çalıştır

İlgili:

```text
Unit Tests
Integration Tests
Architecture Tests
```

çalıştırılmalıdır.

### 8. Mimari sınırları kontrol et

Yeni dependency'lerin mimari kuralları ihlal etmediği doğrulanmalıdır.

### 9. Sonucu raporla

Yapılan değişiklikler, test sonuçları ve varsa riskler açıkça raporlanmalıdır.

---

# 69. Mandatory Stop Conditions

Claude aşağıdaki durumlardan biriyle karşılaşırsa kendi başına mimari karar vermemelidir.

İşlem durdurulmalı ve gerekçe açıklanmalıdır.

### 69.1. Architecture Conflict

İstenen özellik mevcut mimari kurallarla çelişiyorsa.

### 69.2. Cross-Module Database Access

Bir modül başka modülün database'ine doğrudan erişmek zorunda kalıyorsa.

### 69.3. Public Contract Change

Mevcut API contract'ının breaking change gerektirmesi halinde.

### 69.4. Authentication Change

Authentication veya authorization davranışını değiştirecek işlem gerekiyorsa.

### 69.5. Sensitive Data

Kişisel veya hassas verilerin işlenmesiyle ilgili önemli yeni bir gereksinim varsa.

### 69.6. Production Data Risk

Migration veya değişiklik production verisini kaybetme riski taşıyorsa.

### 69.7. Major Refactor

Task için büyük çaplı mimari refactor gerekiyorsa.

### 69.8. Critical External Dependency

Yeni ve kritik bir external dependency eklenmesi gerekiyorsa.

Bu durumlarda:

> **Kendi kararını verme. Dur, problemi açıkla ve onay bekle.**

---

# 70. Anti-Patterns

Aşağıdaki yaklaşımlar kullanılmamalıdır.

## Generic Repository

```text
IRepository<T>
```

## Giant Services

```text
CandidateService
```

içerisinde onlarca unrelated operation.

## Giant Controllers

Business logic'in controller'a taşınması.

## Shared Database Access

Bir modülün diğer modülün tablolarına erişmesi.

## Shared Entity

Bir modül entity'sinin başka modül tarafından doğrudan kullanılması.

## Shared DbContext

Modüllerin ortak DbContext kullanması.

## Giant Common Folder

Her türlü kodun:

```text
Common/
Helpers/
Utils/
Shared/
```

altına atılması.

## Premature Abstraction

Henüz ihtiyaç oluşmadan generic abstraction oluşturulması.

## Unnecessary Package

Built-in .NET capability varken gereksiz NuGet package eklenmesi.

## Hidden Dependency

Bir servisin ihtiyaç duyduğu dependency'nin Service Locator ile gizlenmesi.

---

# 71. Architectural Principles

Gençlik Merkezi mimarisinin temel prensipleri:

1. **Modular Monolith first**
2. **Database per module**
3. **Strict module boundaries**
4. **No cross-module database access**
5. **Contracts over entities**
6. **Domain isolation**
7. **Feature-oriented development**
8. **CQRS where appropriate**
9. **Events for cross-module integration**
10. **Outbox for reliable event publishing**
11. **Least privilege**
12. **Security by design**
13. **Test architectural boundaries**
14. **Avoid unnecessary abstractions**
15. **Prefer simple solutions**
16. **Measure before optimizing**
17. **Keep APIs versioned**
18. **Keep modules independently understandable**
19. **Make future extraction possible**
20. **Do not sacrifice architecture for short-term development speed**

---

# 72. Core Architecture Rule

Bu projenin en önemli teknik kuralı:

> **Bir modül kendi verisinin sahibidir.**

Bir modül:

* Kendi database'inin sahibidir.
* Kendi domain modelinin sahibidir.
* Kendi business rule'larının sahibidir.
* Kendi application workflow'larının sahibidir.

Diğer modüller bu veriye doğrudan erişmez.

İletişim:

```text
Contract
Event
Query
API
```

gibi kontrollü boundary'ler üzerinden gerçekleştirilir.

---

# 73. Final Architecture

Gençlik Merkezi'nin hedef mimarisi:

```text
┌───────────────────────────────────────────────────────────────┐
│                         FRONTENDS                             │
│                                                               │
│ Public │ Candidate │ Employer │ Management                    │
└──────────────────────────────┬────────────────────────────────┘
                               │
                               ▼
┌───────────────────────────────────────────────────────────────┐
│                    ASP.NET CORE WEB API                        │
│                                                               │
│                     MODULAR MONOLITH                          │
│                                                               │
│ Identity │ Candidate │ CareerAdvisor │ CareerDevelopment │ Job │
│ Matching │ Employer │ Interview │ Employment │ Notification   │
│ Website │ Support │ ReferenceData                              │
└──────────────────────────────┬────────────────────────────────┘
                               │
                ┌──────────────┼──────────────┐
                │              │              │
                ▼              ▼              ▼
          Module DBs       RabbitMQ       Background Jobs
                │              │              │
                │              │          Hangfire
                │              │
                ▼              ▼
             Outbox       Integration Events
                │
                ▼
          External Systems
```

Bu mimarinin temel hedefi:

> **İlk günden gereksiz microservice karmaşıklığına girmeden, modülerlik ve veri izolasyonunu sağlayarak; gerektiğinde bağımsız servis mimarisine evrilebilecek sağlam bir temel oluşturmak.**

---

# 74. Final Rule

Claude ve geliştiriciler için son ve bağlayıcı kural:

> **Bu projede kod yazmadan önce bu mimari kuralları oku. Mimariyle çelişen bir implementasyon gerekiyorsa kendin karar verme; dur ve gerekçeyi belirt.**

Mimari:

> **Correctness > Security > Architectural Integrity > Testability > Maintainability > Performance > Simplicity > Development Speed**

şeklinde önceliklendirilir.

**Önce mimariyi anla. Sonra kodu yaz.**
