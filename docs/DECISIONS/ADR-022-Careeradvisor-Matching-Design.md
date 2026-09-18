# ADR-022: CareerAdvisor ve Matching Modülleri — Atama, Havuz ve Bildirim Tasarımı

## Durum

Kabul edildi.

## Bağlam

CareerAdvisor modülü Faz 1 tasarımı sırasında kapsam, danışman-aday/firma atamasının ötesine genişledi:

- Danışmanların adaya ve firmaya not düşebilmesi
- Firmanın personel ihtiyacının, sahibi danışmanın kendi adayları arasında karşılık bulamaması durumunda **genel havuza** açılması ve diğer danışmanların bu ihtiyaca kendi adaylarını önerebilmesi
- Havuzdaki bir ihtiyacın, onu ilk açan danışmandan **farklı** bir danışman tarafından karşılanıp kapatılabilmesi (yıl sonu raporlaması için doğru attribution gerekiyor)
- Adayın kendi danışmanından görüşme talep edebilmesi
- "İş görüşmesi" tipinde not girildiğinde adaya otomatik bildirim + mail gitmesi

Bu genişleme, tasarımı tek bir modülün (CareerAdvisor) sınırlarının dışına taşıdı: havuz/öneri mekanizması aynı anda Employer (ihtiyaç sahibi), Candidate (önerilen aday) ve CareerAdvisor (öneren/kapatan danışman) verisine dokunuyor.

Ayrıca ADR-018'de tespit edilen kısıt geçerliliğini koruyor: CAP, process başına tek DbContext'e (IdentityDbContext) sabit olduğu için, Identity dışındaki hiçbir modül kendi Outbox'ı üzerinden güvenilir event yayınlayamıyor. Bu nedenle modüller arası tutarlılık gereken her noktada senkron public-contract çağrısı kullanılıyor (ADR-018'in genişletilmiş hali).

## Karar

### 1. Modül bağımlılık yönü — tek yönlü, döngüsüz

Aşağıdaki bağımlılık grafiği korunur, hiçbir modül kendisine bağımlı olan bir modüle geri çağrı yapmaz:

```
Candidate ──────► CareerAdvisor ──────► Notification
Employer  ──────► CareerAdvisor ──────► Notification
Matching  ──────► Candidate, Employer, CareerAdvisor ──────► Notification
```

**Reddedilen alternatif:** CareerAdvisor'ın, danışman deaktive edildiğinde Candidate modülünün public contract'ını doğrudan çağırıp yeniden atama yapması (`CareerAdvisor → Candidate`). Bu, `Candidate → CareerAdvisor` (aktif danışman listesi çekmek için) yönüyle birlikte bir döngü oluşturur ve ArchitectureTests'teki tek-yön kuralını ihlal eder.

**Kabul edilen çözüm — Host-seviyesi orkestrasyon:** Danışman deaktivasyonu ve yeniden atama, iki modülün de dışında, Host/API katmanında sıralı iki komutla yürütülür:

1. `DeactivateCareerAdvisorCommand` → CareerAdvisor modülü (yalnızca kendi verisini günceller, dışarı çağrı yapmaz)
2. `ReassignOrphanedCandidatesCommand` → Candidate modülü (kendi `CandidateCv.CareerAdvisorId` verisi üzerinde en-az-yüklü seçimini tekrar çalıştırır)

Aynı prensip, Candidate için tanımlanan kural firma tarafı için de geçerli olduğundan (`ReassignOrphanedCompaniesCommand`), Employer modülü için de uygulanır.

Bu iki komut da Host katmanındaki tek bir admin endpoint'inin handler'ından sırayla `IMediator.Send` ile tetiklenir; iki modül birbirini hiç bilmez. Atomiklik garantisi zaten yoktu (database-per-module), bu değişiklik bir garanti kaybettirmiyor, sadece çağrı yönünü düzeltiyor.

### 2. Otomatik atama (Faz 1 — mevcut karardan değişmedi)

- Yeni kayıt: `Candidate` modülü, `CareerAdvisor` modülünün public contract'ından aktif danışman listesini senkron çeker; kendi `CandidateCv.CareerAdvisorId` gruplamasına bakarak en az yüklü olana atar. Aynı akış Employer modülünde firma için tekrarlanır.
- Algoritma: en az yüklü (en az aday/firma sayısına sahip) danışman.
- Eşzamanlı kayıtlarda hafif dengesizlik riski kabul edilmiştir (Faz 1 ölçeğinde önemsiz); ihtiyaç olursa ileride `SERIALIZABLE` izolasyon veya advisory lock ile sıkılaştırılabilir.

### 3. Not sistemi

`CareerAdvisor` modülünde iki simetrik entity:

- `CandidateNote` — `CandidateCvId`, `CareerAdvisorId`, `NoteType`, `Content`, `CreatedAtUtc`
- `CompanyNote` — `CompanyId`, `CareerAdvisorId`, `NoteType`, `Content`, `CreatedAtUtc`

`NoteType` enum: `Genel`, `IsGorusmesi` (ileride genişletilebilir). Puanlama/kategori alanı Faz 1'de eklenmiyor — serbest metin yeterli görülmüştür; PROJECT.md §8.5'teki yapılandırılmış görüşme sonucu (Olumlu/Olumsuz/Beklemede/Tekrar Görüşme) ayrı bir kavramdır ve ileride tam kapsamlı Interview modülüne aittir, bu ADR kapsamına girmez.

`NoteType = IsGorusmesi` ile not oluşturulduğunda, CareerAdvisor'ın komut handler'ı kendi transaction'ı commit olduktan **sonra** `Notification` modülünün public contract'ını senkron çağırır (in-app bildirim + mail). Outbox kullanılmaz (bkz. Bağlam); bu bir "en iyi çaba" (best-effort) bildirimdir, kritik veri tutarlılığı gerektirmez.

### 4. Aday-başlatmalı görüşme talebi (MeetingRequest)

Yeni aggregate, `CareerAdvisor` modülünde (üçüncü bir modüle dokunmadığı için burada kalması doğal):

- `CandidateCvId`, `CareerAdvisorId` (adayın kendi danışmanı — otomatik çözülür)
- `Status`: `TalepEdildi` → `TarihOnerildi` → `Onaylandi` / `Reddedildi`
- `ProposedDateTimeUtc`, `ConfirmedAtUtc`

Akış: Aday talep açar (mevcut `Candidate → CareerAdvisor` yönünü kullanır) → danışman tarih/saat önerir → adaya senkron bildirim gider → aday onaylar (yine `Candidate → CareerAdvisor`). Yeni bir modül bağımlılığı açmaz.

### 5. Genel Havuz ve öneri mekanizması

**Employer modülü** — mevcut `PersonnelNeed` (bkz. PROJECT.md §6.3) aggregate'ine eklenen alanlar:

| Alan | Açıklama |
|---|---|
| `Status` | `Taslak` / `KendiHavuzunda` / `GenelHavuzda` / `Karsilandi` |
| `PooledByAdvisorId` | İhtiyacı havuza atan danışman |
| `PooledAtUtc` | |
| `ClosedByAdvisorId` | İhtiyacı fiilen karşılayan danışman (ihtiyaç sahibi danışmanla aynı olmak zorunda değil) |
| `ClosedAtUtc` | |
| `FulfilledByCandidateCvId` | nullable, cross-module ID referansı |

`ClosedByAdvisorId`, "kim ne iş yapmış" yıl sonu raporlamasının temel veri kaynağıdır; raporlama ekranı Faz 1 kapsamında değildir ama veri şimdiden yakalanır.

**Matching modülü (yeni — mimari listede zaten planlıydı, bu ADR ile Faz 1'e alınıyor):**

`CandidateSuggestion` aggregate:

- `PersonnelNeedId` (Employer'a cross-module ID referansı)
- `CandidateCvId` (Candidate'a cross-module ID referansı)
- `SuggestingAdvisorId` (CareerAdvisor'a cross-module ID referansı)
- `Status`: `Onerildi` / `KabulEdildi` / `Reddedildi`
- `CreatedAtUtc`

Matching modülü Candidate, Employer ve CareerAdvisor'ın public contract'larına bağımlıdır (tek yön); hiçbiri Matching'e bağımlı değildir.

**Akış:**

1. Danışman A, firmasının ihtiyacında kendi adayları içinde uygun bulamaz → "Havuza Ata" → `Employer.PersonnelNeed.Status = GenelHavuzda`, `PooledByAdvisorId = A`.
2. Employer, komut transaction'ı commit olduktan sonra `Notification` modülünün public contract'ını senkron çağırır: tüm aktif danışmanlara broadcast bildirim ("Danışman A, X firmasının ihtiyacını havuza atmıştır").
3. Danışman B, Genel Havuz sayfasında ihtiyacı görür, kendi adayı için `Matching.CandidateSuggestion` oluşturur (`Status = Onerildi`).
4. Öneri kabul edilip yerleştirme gerçekleşince, Matching modülü `Employer` modülünün public contract'ını senkron çağırarak `PersonnelNeed`'i kapatır: `Status = Karsilandi`, `ClosedByAdvisorId = B`, `FulfilledByCandidateCvId` set edilir.

### 6. Bildirim çağrıları — özet

Aşağıdaki tüm çağrılar senkron public-contract çağrısıdır, Outbox kullanılmaz (ADR-018 kısıtı gereği):

- CareerAdvisor → Notification: tekli bildirim (`IsGorusmesi` notu, görüşme talebi onay/öneri adımları)
- CareerAdvisor → Notification: toplu bildirim (danışmanın kendi adaylarına)
- Employer → Notification: broadcast bildirim (havuza atama duyurusu)

## Sonuçlar

- Modül bağımlılık grafiği DAG olarak kalır; ArchitectureTests'teki yön kuralları değişmeden korunur.
- Host katmanında yeni bir "cross-module orchestration" sorumluluğu doğar (deaktivasyon + yeniden atama). Bu, gelecekte benzer çok-modüllü admin akışları için tekrar kullanılabilecek bir yer haline gelir.
- Matching modülü, planlanandan daha erken (Faz 1 içinde, Faz 1.5 olarak) devreye giriyor; kapsamı bu ADR'de yalnızca `CandidateSuggestion` ile sınırlıdır, tam eşleştirme skorlama sistemi (PROJECT.md §10) hâlâ ileri faz.
- Interview modülü ve adayın/firmanın yapılandırılmış görüşme süreci (PROJECT.md §8.4-8.6) bilinçli olarak bu ADR kapsamı dışında bırakılmıştır; `NoteType.IsGorusmesi` ve `MeetingRequest`, o modül gelene kadarki minimal köprüdür.

## Reddedilen Alternatifler

1. **Event/Outbox tabanlı yeniden atama** — ADR-018'de zaten reddedilmişti, aynı gerekçeyle burada da reddedildi.
2. **CareerAdvisor → Candidate/Employer doğrudan çağrısı** — modül döngüsü oluşturduğu için reddedildi, yerine Host-seviyesi orkestrasyon kabul edildi.
3. **Havuz/öneri mantığının CareerAdvisor modülü içine gömülmesi** — CareerAdvisor'ın hem Candidate hem Employer'a bağımlı hale gelmesini gerektirirdi; bunun yerine ayrı bir Matching modülü tercih edildi.
4. **Tam kapsamlı Interview modülünün bu fazda açılması** — kapsam çok büyüyeceği için ertelendi; `NoteType.IsGorusmesi` + `MeetingRequest` geçici köprü olarak yeterli görüldü.
