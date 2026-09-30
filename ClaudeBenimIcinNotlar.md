Başlangıç Notları

1. Backend Architecture        (büyük ölçüde iskelet mevcut)
2. Backend Foundation          (SharedKernel, Contracts, BuildingBlocks.Infrastructure)
3. Database-per-Module kurulumu
4. Identity / Authorization    ← ŞİMDİ BURADAYIZ
5. Domain Modules              (Candidate, Employer, CareerAdvisor, Job, Matching, Interview, Employment, CareerDevelopment, ...)
6. CQRS / MediatR              (Identity'den itibaren pattern olarak yerleşir)
7. Events / RabbitMQ / Outbox
8. Notification / Hangfire / Media
9. API Contracts
10. Backend Tests
11. Backend Completion
12. React Frontend

Mimari çatışma (plan onayında karara bağlandı)
İstek "IQueryable<T> extension'ı SharedKernel'de, Feature handler kullanabilsin" diyordu — bu AGENTS.md §16/§18 ve FeatureDbContextTests'le doğrudan çelişiyordu (SharedKernel Domain tarafından referans ediliyor; ona EF Core eklemek Domain'i kirletirdi). Çözüm: extension BuildingBlocks.Infrastructure'da, sadece repository/reader'lar tarafından çağrılıyor — handler'lar hâlâ yalnızca PagedResult<T> görüyor.

Kırıcı değişiklik (açıkça belirtiliyor)
Identity'nin admin/users ve admin/audit-log endpoint'lerinde page=0/pageSize=500 gibi değerler artık 400 değil, sessizce clamp edilip 200 dönüyor. Sıralama ve mevcut response alan adları değişmedi

---
Görev 5 tamamlandı ve commit edildi (6682fb6). Özet:

Yapılanlar:

IIdentityService'e CreateUserAsync ve DeactivateUserAsync eklendi. CreateUserAsync, Identity'nin mevcut RegisterUserCommand'ını ISender üzerinden tetikliyor (email-verification token, UserRegisteredIntegrationEvent dahil aynı akış — mantığı tekrar yazmak yerine yeniden kullandım). DeactivateUserAsync bilinçli olarak AdminDeactivateUserCommand'ı kullanmıyor — bu sistem-tetiklemeli bir telafi, admin aksiyonu değil; admin audit log'una yanlış şekilde atfedilmemeli.
RegisterCandidateCommandHandler: User oluşturur → CandidateCv+CandidateCvContent persist eder. İkinci adım hata verirse (try/catch), oluşturulan User'ı Deactivate ederek telafi eder ve orijinal hatayı yeniden fırlatır (beklenmeyen bir altyapı hatası olduğu için Result.Failure değil, GlobalExceptionHandler'a bırakılıyor).
Candidate modülü artık Host'a tam bağlı (AddCandidateModule, MediatR/FluentValidation kaydı, CandidateDatabase bağlantı dizesi, POST /api/v1/candidates/register) — ilk gerçek use-case geldiği için artık kayıtsız değil.
Telafi senaryosu testi — gerçek hata ile: Master prompt'un açık talimatı gereği mock kullanmadım. BrokenCandidateDatabaseWebApplicationFactory, sadece Candidate bağlantısını var olmayan bir LocalDB instance'ına yönlendiriyor (Identity/Notification/ReferenceData gerçek kalıyor); böylece CandidateDbContext.SaveChangesAsync gerçekten patlıyor ve compensation'ın gerçekten çalıştığını (User'ın Disabled duruma geçtiğini) doğruluyorum.

Doğrulama: build 0 hata, unit 248/248, architecture 5/5, integration 77/77 (4 yeni Candidate testi + compensation testi dahil) — hepsi yeşil.

Sırada Görev 6 — Profil tamamlanma yüzdesi read-model: CandidateCvUpdated/CandidateCvContentUpdated domain event'leri, event handler ile CompletionPercentage hesaplama. Devam edeyim mi?