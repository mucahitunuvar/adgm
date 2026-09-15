# PERFORMANCE.md — Gençlik Merkezi Performans Politikası

## 1. Amaç

Bu doküman, Gençlik Merkezi platformunun performanslı, ölçeklenebilir ve sürdürülebilir şekilde geliştirilmesi için uyulması gereken kuralları tanımlar.

Performans yalnızca response süresinden ibaret değildir.

Aşağıdaki alanların tamamı birlikte değerlendirilir:

* API response time
* Database query performance
* CPU kullanımı
* Memory kullanımı
* Network traffic
* Cache efficiency
* Background processing
* Message processing
* File operations
* Concurrency
* Scalability
* Resource utilization

Temel prensip:

> **Performans uğruna mimari sınırlar bozulamaz.**

---

# 2. Performans Öncelikleri

Performans optimizasyonlarında aşağıdaki öncelik uygulanır:

```text
Correctness
    ↓
Security
    ↓
Architectural Integrity
    ↓
Performance
    ↓
Premature Optimization
```

Ölçülmeyen bir problem için karmaşık optimizasyon yapılmamalıdır.

Temel yaklaşım:

> **Önce ölç, sonra optimize et.**

---

# 3. Genel Performans Prensipleri

Uygulama:

* gereksiz database sorgusu çalıştırmamalı,
* gereksiz veri taşımamalı,
* gereksiz object allocation oluşturmamalı,
* gereksiz serialization yapmamalı,
* uzun süren işlemleri request thread'inde çalıştırmamalı,
* uygun cache mekanizmalarını kullanmalı,
* büyük veri setlerini pagination ile işlemeli,
* database indexlerinden yararlanmalı,
* async API'leri doğru kullanmalıdır.

Ancak performans adına kodun anlaşılabilirliği gereksiz şekilde düşürülmemelidir.

---

# 4. Async Programming

I/O işlemleri asynchronous olarak gerçekleştirilmelidir.

Özellikle:

* Database
* File Storage
* HTTP calls
* RabbitMQ
* External APIs

için async API'ler kullanılmalıdır.

Aşağıdaki kullanımlar yasaktır:

```text
.Result
.Wait()
Thread.Sleep()
```

Async chain mümkün olduğunca korunmalıdır.

CancellationToken desteklenen operasyonlara aktarılmalıdır.

Örneğin:

```csharp
Task<T> Handle(
    Query request,
    CancellationToken cancellationToken)
```

---

# 5. CancellationToken

Uzun süren veya I/O tabanlı işlemler `CancellationToken` desteklemelidir.

Özellikle:

* EF Core queries
* HTTP requests
* File operations
* Background operations

için cancellation desteklenmelidir.

Kullanıcı request'i iptal ettiğinde gereksiz database veya external service işlemlerinin devam etmesi engellenmelidir.

---

# 6. EF Core Query Performance

EF Core sorguları performans açısından dikkatli tasarlanmalıdır.

Temel kurallar:

* Gereksiz `Include` kullanılmamalı.
* Gereksiz entity load edilmemeli.
* Büyük result set doğrudan memory'e alınmamalı.
* Read-only sorgularda uygun durumlarda `AsNoTracking()` kullanılmalı.
* Projection tercih edilmeli.
* Pagination uygulanmalı.
* N+1 query problemi engellenmeli.

---

# 7. Projection

Liste ve read-only endpoint'lerinde ihtiyaç duyulan alanlar doğrudan projection ile alınmalıdır.

Tercih:

```csharp
.Select(x => new CandidateListItemDto
{
    Id = x.Id,
    FullName = x.FullName,
    Status = x.Status
})
```

Yerine gereksiz şekilde tüm entity'nin yüklenmesinden kaçınılmalıdır.

Amaç:

```text
Database
    ↓
Only Required Columns
    ↓
DTO
    ↓
API
```

---

# 8. AsNoTracking

Read-only sorgularda entity tracking gerekmiyorsa:

```csharp
AsNoTracking()
```

kullanılmalıdır.

Özellikle:

* List queries
* Search queries
* Dashboard queries
* Reporting queries
* Read-only detail queries

için değerlendirilmelidir.

Tracking gereken update işlemlerinde gereksiz `AsNoTracking()` kullanılmamalıdır.

---

# 9. N+1 Query Prevention

N+1 query problemi kritik performans problemi olarak kabul edilir.

Örneğin:

```text
1 Candidate query
+
N Education queries
+
N Experience queries
+
N Skill queries
```

gibi bir yapı oluşturulmamalıdır.

Gerekli ilişkiler:

* projection,
* uygun Include,
* split query,
* batch query,
* aggregate query

gibi yöntemlerle kontrollü şekilde alınmalıdır.

---

# 10. Include Kullanımı

`Include` yalnızca gerçekten ihtiyaç duyulan ilişkiler için kullanılmalıdır.

Çok sayıda collection'ın aynı sorguda Include edilmesi:

* Cartesian explosion,
* büyük result set,
* yüksek memory kullanımı

oluşturabilir.

Gerekli durumlarda:

```text
AsSplitQuery()
```

değerlendirilebilir.

Ancak otomatik olarak her sorguya `AsSplitQuery()` eklenmemelidir.

---

# 11. Pagination

Büyük veri setleri hiçbir zaman kontrolsüz şekilde API response'una döndürülmemelidir.

Özellikle:

* Candidates
* Jobs
* Applications
* Interviews
* Employment records
* Notifications
* Audit records
* CMS content
* Event registrations

gibi collection endpoint'lerinde pagination uygulanmalıdır.

Örneğin:

```text
?page=1&pageSize=20
```

veya uygun cursor-based pagination kullanılabilir.

`pageSize` server-side maksimum değer ile sınırlandırılmalıdır.

---

# 12. Search

Search endpoint'leri database performansı dikkate alınarak tasarlanmalıdır.

Wildcard ve substring search işlemleri büyük tablolarda kontrolsüz kullanılmamalıdır.

Search gereksinimi büyüdüğünde:

* uygun database indexleri,
* Full-Text Search,
* ayrı search infrastructure

değerlendirilebilir.

Search altyapısı eklemek mevcut database-per-module mimarisini bozmamalıdır.

---

# 13. Database Index Strategy

Indexler gerçek sorgu pattern'lerine göre tasarlanmalıdır.

Index oluşturulurken:

* WHERE
* JOIN
* ORDER BY
* UNIQUE constraint
* frequent lookup

senaryoları değerlendirilmelidir.

Her kolona index eklenmesi doğru yaklaşım değildir.

Fazla index:

* INSERT performansını,
* UPDATE performansını,
* DELETE performansını,
* storage kullanımını

olumsuz etkileyebilir.

Index değişiklikleri migration ile yönetilmelidir.

---

# 14. Database-per-Module Performance

Her modül kendi database'inin performansından sorumludur.

Örneğin:

```text
Candidate Module
        ↓
Candidate DB

Employer Module
        ↓
Employer DB

Job Module
        ↓
Job DB
```

Bir modülün performans problemi başka modülün database'ine doğrudan erişilerek çözülmemelidir.

Aşağıdaki yaklaşım yasaktır:

```text
Candidate DB
    ↓
Direct SQL
    ↓
Employer DB
```

Cross-module ihtiyaçlar:

* Contract
* Integration Event
* Read Model
* Cache
* uygun Application-level communication

ile çözülmelidir.

---

# 15. CQRS ve Performans

CQRS performans için kullanılmaktadır ancak her işlem gereksiz şekilde ayrıştırılmamalıdır.

Query'ler:

* read optimized,
* projection based,
* pagination aware

olmalıdır.

Command'lar:

* gerekli entity'leri yüklemeli,
* gereksiz data fetch etmemeli,
* transaction scope'u mümkün olduğunca küçük tutmalıdır.

---

# 16. Transaction Scope

Transaction'lar mümkün olduğunca kısa tutulmalıdır.

Transaction içerisinde:

* External API call
* Email gönderimi
* SMS gönderimi
* File upload
* Uzun hesaplama
* Kullanıcı bekleten operasyon

yapılmamalıdır.

Örneğin:

```text
Database Transaction
    ↓
Save business state
    ↓
Commit
    ↓
Outbox
    ↓
Async processing
```

yaklaşımı tercih edilmelidir.

---

# 17. Cross-Module Operations

Bir use case birden fazla modülü ilgilendiriyorsa gereksiz synchronous database chaining yapılmamalıdır.

Örneğin:

```text
Candidate
    ↓
Matching
    ↓
Employer
    ↓
Job
```

işleminde her request'in onlarca database sorgusuna dönüşmesi engellenmelidir.

Gerekli durumlarda:

* Integration Events
* Outbox
* Read Models
* Cache

kullanılmalıdır.

---

# 18. Caching

Cache yalnızca gerçekten fayda sağladığı durumlarda kullanılmalıdır.

İlk aşamada:

```text
IMemoryCache
```

kullanılabilir.

İlerleyen ihtiyaçlarda:

```text
Redis
```

değerlendirilebilir.

Cache için uygun adaylar:

* Reference data
* Frequently accessed configuration
* Public CMS content
* Permission metadata
* Stable lookup data

olabilir.

---

# 19. Cache Rules

Cache'lenen veriler için:

* expiration,
* invalidation,
* cache key strategy,
* stale data davranışı

belirlenmelidir.

Security-sensitive authorization verileri cache'lendiğinde invalidation kritik kabul edilir.

Cache hiçbir zaman ana veri kaynağı olarak kullanılmamalıdır.

---

# 20. Reference Data Cache

`ReferenceData` modülündeki sık kullanılan ve nadiren değişen veriler cache için güçlü adaydır.

Örneğin:

```text
Country
City
District
Sector
Profession
Skill
University
School
```

Ancak cache invalidation mekanizması düşünülmeden kalıcı cache uygulanmamalıdır.

---

# 21. Dashboard Performance

Dashboard endpoint'leri çok sayıda küçük sorgunun birleşiminden oluşmamalıdır.

Örneğin:

```text
Candidate Count
+
Job Count
+
Interview Count
+
Employment Count
+
Advisor Count
```

gibi metrikler için uygun aggregate/query stratejisi kullanılmalıdır.

Dashboard response'ları mümkün olduğunca:

* projection,
* aggregate query,
* cache

kullanılarak optimize edilmelidir.

---

# 22. Reporting

Reporting işlemleri production transaction path'ini gereksiz yere yavaşlatmamalıdır.

Büyük raporlar için:

* background job,
* asynchronous processing,
* precomputed data,
* reporting read model

değerlendirilebilir.

Kullanıcının HTTP request'i içerisinde uzun süren rapor üretimi yapılmamalıdır.

---

# 23. RabbitMQ

RabbitMQ uzun süren ve asynchronous yapılabilecek işlemler için kullanılabilir.

Örneğin:

```text
Candidate Created
        ↓
Event
        ↓
Notification
        ↓
Email / SMS
```

Bu sayede ana request'in response süresi gereksiz yere uzatılmaz.

Ancak RabbitMQ her işlem için kullanılmamalıdır.

Basit synchronous işlemler gereksiz yere event-driven hale getirilmemelidir.

---

# 24. Outbox Performance

Outbox Pattern transaction güvenilirliği için kullanılacaktır.

Outbox tablosunun büyümesi kontrol edilmelidir.

Background processor:

* batch processing,
* retry policy,
* exponential backoff,
* failed message handling

kullanmalıdır.

Başarıyla publish edilen outbox kayıtları retention politikasına göre temizlenmelidir.

Outbox cleanup işlemi production transaction'larını etkilememelidir.

---

# 25. Hangfire

Uzun süren veya scheduled işlemler Hangfire ile background job olarak çalıştırılabilir.

Örneğin:

```text
Scheduled Notifications
Outbox Processing
Cleanup Jobs
Report Generation
Data Maintenance
Periodic Synchronization
```

HTTP request içerisinde uzun süren background iş çalıştırılmamalıdır.

---

# 26. File Processing

Büyük dosyalar application memory'sine tamamen alınmamalıdır.

Mümkün olduğunda:

* streaming,
* chunked processing,
* direct storage upload

yaklaşımları değerlendirilmelidir.

Dosya indirme işlemlerinde de gereksiz memory allocation engellenmelidir.

---

# 27. API Response Performance

API response'ları yalnızca ihtiyaç duyulan veriyi içermelidir.

Aşağıdakilerden kaçınılmalıdır:

```text
Huge JSON response
Unnecessary nested objects
Unused properties
Entire entity graphs
Large binary data inside JSON
```

Binary file içerikleri mümkün olduğunca ayrı file endpoint'leri veya storage mekanizmaları üzerinden sunulmalıdır.

---

# 28. Serialization

API response serialization maliyeti dikkate alınmalıdır.

Circular object graph oluşturulmamalıdır.

Entity'lerin doğrudan API response olarak dönülmesi yerine DTO / response model kullanılmalıdır.

Bu yaklaşım:

* payload boyutunu,
* coupling'i,
* serialization maliyetini

kontrol eder.

---

# 29. HTTP Client Performance

External API çağrılarında uygun `HttpClient` kullanımı sağlanmalıdır.

Her request için yeni HTTP client oluşturulmamalıdır.

External calls için:

* timeout,
* cancellation,
* retry,
* circuit breaker gerektiğinde

uygulanmalıdır.

Retry sonsuz olmamalıdır.

---

# 30. Concurrency

Concurrent işlemlerde data integrity korunmalıdır.

Özellikle:

* Job approval
* Candidate assignment
* Interview scheduling
* Employment creation
* Status transitions

gibi işlemlerde race condition ihtimali değerlendirilmelidir.

Gerekli durumlarda optimistic concurrency kullanılmalıdır.

Aynı kaydın iki kullanıcı tarafından eşzamanlı değiştirilmesi kontrollü şekilde yönetilmelidir.

---

# 31. Background Processing

Background job'lar idempotent tasarlanmalıdır.

Aynı job'ın birden fazla kez çalışması durumunda sistem bozulmamalıdır.

Özellikle:

```text
Notifications
Events
Outbox messages
Employment follow-up
Scheduled tasks
```

için duplicate execution dikkate alınmalıdır.

---

# 32. Memory Management

Uzun yaşayan collection'lar ve gereksiz object allocation'lar kontrol edilmelidir.

Özellikle:

* large file processing,
* bulk imports,
* reporting,
* large queries,
* large JSON payloads

memory açısından izlenmelidir.

Büyük veri setleri mümkün olduğunca stream veya batch şeklinde işlenmelidir.

---

# 33. Bulk Operations

Toplu işlemler gerektiğinde tek tek database round-trip yapılmamalıdır.

Örneğin:

```text
1000 records
    ↓
1000 individual INSERT
```

yerine uygun batch/bulk yaklaşımı değerlendirilmelidir.

Ancak bulk operation kullanımı:

* business rule,
* transaction,
* audit,
* domain event

gereksinimleri ile birlikte değerlendirilmelidir.

Performans uğruna domain kuralları bypass edilmemelidir.

---

# 34. API Timeouts

Uzun süren request'ler kontrol altında tutulmalıdır.

Bir API endpoint'inin uzun sürmesinin nedeni:

* database query,
* external API,
* file operation,
* heavy calculation

ise root cause çözülmelidir.

Timeout değerini sürekli artırmak performans problemini çözmek olarak kabul edilmez.

---

# 35. Observability

Performance sorunları ölçülebilir olmalıdır.

Aşağıdaki metrikler izlenmelidir:

```text
Request duration
Database query duration
Error rate
Throughput
CPU usage
Memory usage
Cache hit/miss
Queue length
Background job duration
External API duration
```

OpenTelemetry ile distributed tracing altyapısı gerektiğinde kullanılmalıdır.

---

# 36. Slow Query Detection

Yavaş sorgular tespit edilebilir ve izlenebilir olmalıdır.

Özellikle:

* yüksek execution time,
* yüksek logical reads,
* büyük result set,
* missing index,
* inefficient join

gibi problemler incelenmelidir.

Bir sorgu yavaşsa ilk çözüm:

```text
"Cache ekleyelim."
```

olmamalıdır.

Önce sorgunun neden yavaş olduğu anlaşılmalıdır.

---

# 37. Performance Testing

Performance testleri yalnızca production sonrasında yapılmamalıdır.

Uygun seviyelerde:

* Unit performance checks
* Integration performance tests
* Load tests
* Stress tests
* API benchmarks

uygulanabilir.

Özellikle kritik endpoint'ler belirlenmelidir.

---

# 38. Critical Performance Areas

Aşağıdaki işlemler performans açısından kritik kabul edilir:

```text
Authentication
Candidate Search
Job Search
Matching
Dashboard
Interview Calendar
Reporting
File Upload
File Download
Notification Processing
Outbox Processing
```

Bu alanlarda performans ölçümleri düzenli olarak takip edilmelidir.

---

# 39. Matching Performance

Matching modülü zaman içerisinde sistemin en yoğun hesaplama yapan modüllerinden biri olabilir.

Basit matching işlemleri request sırasında çalışabilir.

Ancak:

```text
Thousands of candidates
+
Thousands of jobs
+
Complex scoring
+
Multiple skill dimensions
```

gibi senaryolarda hesaplamanın synchronous HTTP request içerisinde yapılmasından kaçınılmalıdır.

Gerektiğinde:

```text
Command
    ↓
Background Job
    ↓
Matching Calculation
    ↓
Persist Result
    ↓
Notify / Publish Event
```

yaklaşımı kullanılabilir.

---

# 40. Performance ve Domain Integrity

Performans optimizasyonları aşağıdaki domain kurallarını bypass edemez:

* Authorization
* Candidate ownership
* Advisor assignment
* Employer isolation
* Job lifecycle
* Interview lifecycle
* Employment lifecycle
* Audit requirements

Örneğin daha hızlı olması için resource authorization kaldırılması kabul edilemez.

---

# 41. Premature Optimization

Aşağıdaki davranışlardan kaçınılmalıdır:

* Ölçmeden cache eklemek
* Her yere Redis eklemek
* Gereksiz async/event kullanmak
* Gereksiz micro-optimization
* Her sorguya AsNoTracking eklemek
* Her sorguya AsSplitQuery eklemek
* Gereksiz database indexleri oluşturmak
* Gereksiz background job oluşturmak
* Karmaşık abstraction'lar oluşturmak

Kod önce doğru ve anlaşılır olmalıdır.

---

# 42. Performance Review Checklist

Yeni bir feature tamamlanmadan önce:

```text id="x0t0bw"
[ ] Gereksiz database query yok
[ ] N+1 problemi kontrol edildi
[ ] Projection değerlendirildi
[ ] AsNoTracking gereksinimi değerlendirildi
[ ] Pagination uygulandı
[ ] Index gereksinimi değerlendirildi
[ ] Response boyutu kontrol edildi
[ ] CancellationToken kullanıldı
[ ] Async I/O kullanıldı
[ ] Uzun işlem background job olarak değerlendirildi
[ ] Cache gereksinimi değerlendirildi
[ ] Cross-module query yapılmadı
[ ] Transaction scope kontrol edildi
[ ] Concurrency değerlendirildi
[ ] Logging/telemetry maliyeti değerlendirildi
[ ] Kritik endpoint için performance test değerlendirildi
```

---

# 43. Claude Performance Rules

Claude aşağıdaki yaklaşımı izlemelidir:

1. Önce mevcut implementasyonu analiz et.
2. Performance problemini ölçülebilir şekilde tanımla.
3. Problemin root cause'unu belirle.
4. En küçük güvenli optimizasyonu uygula.
5. Testleri çalıştır.
6. Önce/sonra davranışı doğrula.
7. Mimari sınırları kontrol et.

Claude yalnızca "daha hızlı olabilir" gerekçesiyle büyük refactor yapmamalıdır.

---

# 44. Performance Stop Conditions

Aşağıdaki durumlarda Claude durmalı ve açıklama istemelidir:

* Database-per-module sınırını aşan optimizasyon
* Cross-module direct database access
* Security kontrolünü kaldırmayı gerektiren optimizasyon
* Büyük ölçekli caching altyapısı gereksinimi
* Redis gibi yeni kritik dependency eklenmesi
* Domain kurallarını bypass eden bulk operation
* Production database üzerinde riskli değişiklik
* Breaking API contract değişikliği
* Major architecture refactor

---

# 45. Golden Performance Rule

Gençlik Merkezi için temel performans prensibi:

> **Önce doğru mimariyi kur, sonra ölç, sonra optimize et.**

İkinci prensip:

> **Performans problemi mimari sınırları bozarak çözülmez.**

Üçüncü prensip:

> **Cache, async processing veya infrastructure eklemek bir performans stratejisi olabilir; ancak ölçülmemiş bir problemin varsayılan çözümü değildir.**

Son prensip:

> **En iyi performans optimizasyonu, gereksiz işi hiç yapmamaktır.**
