# ADR-012 — SQLite for Integration Tests

* **Status:** Accepted
* **Date:** 2026-09-15

## Context

Integration testlerde gerçek SQL çevirisini (query translation), constraint davranışlarını ve ilişkisel bütünlüğü doğrulamak gerekmektedir.

EF Core InMemory provider bu doğrulamayı sağlayamaz; InMemory, gerçek SQL motorlarının reddedeceği bazı sorguları sessizce kabul edebilir, bu da production'da ortaya çıkan hataların testlerde yakalanamamasına yol açar.

Buna karşılık, integration testlerde doğrudan SQL Server'a bağımlı bir test altyapısı kurmak (örn. paylaşılan test veritabanı veya container tabanlı SQL Server) CI/CD ve local development ortamlarında ek operasyonel yük ve kırılganlık oluşturur.

## Decision

Integration testlerde **SQLite** (relational, dosya veya memory tabanlı) kullanılacaktır.

Provider seçimi `Database:Provider` konfigürasyon anahtarı üzerinden ortam bazlı belirlenir:

```text
Development / Production → SqlServer (varsayılan, değişmez)
Testing                  → Sqlite
```

Bu, `WebApplicationFactory<Program>` üzerinden çalıştırılan integration testlerin gerçek bir HTTP pipeline'ı ve gerçek bir relational engine üzerinden uçtan uca doğrulama yapmasını sağlar.

## Why

* Gerçek SQL çevirisi doğrulanır (InMemory'nin sağlayamadığı).
* Testler harici bir servise (SQL Server instance'ı, container vb.) bağımlı olmadan, hızlı ve izole şekilde çalışabilir.
* CI/CD pipeline'ında ek altyapı kurulumu gerekmez.
* Local development'ta ekstra bir bağımlılık (Docker, local SQL Server vb.) zorunlu kılınmaz.

## Consequences

### Positive

* Integration testler hızlı, izole ve deterministiktir.
* Gerçek SQL sorgu hataları erken yakalanır.
* Yeni bir modül eklendiğinde aynı desen (config-driven provider seçimi) tekrar kullanılabilir — her modülde bu tartışma yeniden açılmaz.

### Negative

* SQLite ile SQL Server arasında bazı davranış farkları vardır (örn. case sensitivity, belirli fonksiyonların SQL çevirisi, bazı constraint davranışları). Bu, test-production paritesinde küçük bir risk oluşturur.
* Bu risk, InMemory provider'ın yol açtığı çok daha büyük yanlış-pozitif (testte geçen ama production'da patlayan sorgu) riskine kıyasla kabul edilebilir görülmüştür.
* Gerçekten SQL Server'a özgü bir davranış test edilmesi gerekiyorsa (örn. belirli bir T-SQL fonksiyonu), bu özel durumlar ayrıca değerlendirilmeli ve gerekirse ayrı bir test stratejisi kurulmalıdır.

## Alternatives Considered

### EF Core InMemory Provider

SQL çeviri hatalarını yakalamadığı, gerçek bir relational engine davranışı sergilemediği için reddedildi.

### Testcontainers + Gerçek SQL Server

En yüksek doğruluğu sağlayacak yaklaşım olsa da, mevcut proje aşamasında gerekli operasyonel karmaşıklığı (container yönetimi, CI/CD entegrasyonu, test süresi artışı) haklı çıkaracak bir ihtiyaç görülmemiştir. Proje ölçeklendikçe ve SQL Server'a özgü davranışların test edilmesi ihtiyacı arttıkça yeniden değerlendirilebilir.
