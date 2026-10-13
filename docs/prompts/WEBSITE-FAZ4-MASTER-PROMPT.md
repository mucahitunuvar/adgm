# Website Modülü — Faz 4 (Etkinlik: Takvim, Kayıt, Kontenjan, Yedek Liste) Master Prompt

## 0. Nasıl kullanılır

Bu dosya **görev görev** uygulanır. Her görev ayrı bir Claude Code oturumunda yürütülür:

1. Kullanıcı `/clear` yapar ve şunu gönderir: *"docs/prompts/WEBSITE-FAZ4-MASTER-PROMPT.md dosyasındaki §1'i ve Görev N'i oku, yalnızca Görev N'i uygula."*
2. Yalnızca **§1** ve **ilgili görevi** okursun. Görevin **"Oku"** listesi dışındaki dokümanları okuma; AGENTS.md'nin yalnızca belirtilen bölümlerine bak.
3. Önceki görevlerin kodu commit'lenmiştir; ihtiyaç duyduğunda `git log` ve kaynak dosyalarından öğren.
4. Görev bitince commit'le, push'la, kısa raporunu yaz ve **dur**.

---

## 1. Genel kurallar (her görevde geçerli)

**Kullanıcı kararları (Faz 4 için alındı):**

- **Onay modeli etkinlik başına seçilir:** `EventSchedule.AutoConfirm`. Açıksa doğrulanmış kayıt, kontenjan varsa otomatik `Confirmed` olur; kapalıysa `Applied` kalır ve admin onaylar. Kontenjanı yalnızca **kontenjanı tutan durumlar** (`Confirmed`, `Attended`, `NoShow`) doldurur; `Applied` ve `PendingVerification` yer tutmaz.
- Kontenjan doluyken: `WaitlistEnabled` açıksa kayıt `Waitlisted` olur, kapalıysa kayıt `409` (`Event.CapacityFull`) ile reddedilir. Yedekten otomatik terfi **yoktur**; admin onaylar.
- Dışa aktarım **CSV** (bülten dışa aktarımıyla aynı desen: UTF-8 BOM, CSV enjeksiyonu kaçışı). `.ics` public ve kişisel veri içermez. XLSX yoktur.
- **Etkinlik öncesi hatırlatma e-postası Faz 4 kapsamı dışındadır.** Online link yalnızca onay e-postasında gider.
- QR giriş, yoklama ekranı, sertifika ve katılım puanı yoktur (ADR-024 §11.2). `Attended`/`NoShow` yalnızca admin listesinde elle işaretlenir.

**Mimari:** Faz 0–3 desenleri: vertical slice, `RowVersion` ile iyimser eşzamanlılık (`409`), `TimeProvider`, projection + `AsNoTracking`, Result deseni, zengin metinler `IHtmlContentSanitizer` ile handler'da temizlenir, dile bağlı metinler `...Translation` child entity'lerinde ve varsayılan dilde zorunlu. Yönetim uçları `/api/v1/admin/website/...`, public uçlar `/api/v1/public/...`. Anonim yazma uçları Faz 3'teki `IPublicSubmissionGuard` ve `public-forms` rate limit'ini kullanır. E-postalar `IWebsiteEmailSender` ile commit **sonrasında**, best-effort gönderilir; şablonlar kodda tek yerde, dile göre tutulur.

**Aggregate sınırı (ADR-024 §11.1):** `EventSchedule`, `ContentItem` ile 1:1 bağlı **ayrı** aggregate'tir (aralarında yalnızca `ContentItemId` referansı). `EventRegistration` ayrı aggregate'tir. Kayıt komutu `EventRegistration`'ı ve `EventSchedule` sayaçlarını **aynı Unit of Work'te** kaydeder; kontenjan eşzamanlılığı `EventSchedule.RowVersion` ile korunur, çakışmada **en fazla 3 kez** yeniden denenir, sonra `409`.

**Yetkilendirme:**

| İşlem | Policy |
|---|---|
| Etkinlik takvimi oluşturma/güncelleme/iptal | `Website.Content.Manage` |
| Kayıtları ve katılımcı listesini görüntüleme, dışa aktarma | `Website.Submissions.View` |
| Kayıt durumu değiştirme, admin iptali | `Website.Submissions.Manage` |

**Kişisel veri erişim kaydı:** Faz 3'teki `PersonalDataAccessLog`'un `EntityType` değerlerine `EventRegistration` eklenir. Kayıt içeren her okuma ucu (liste, detay, dışa aktarma) erişimi yazar; kişisel veri içermeyen sayaç/özet uçları yazmaz.

**Cache:** Etkinlik içeriği/takvimi mutasyonları (Görev 1) `WebsiteCacheInvalidator.InvalidateAllPublic` çağırır. **Kayıt işlemleri çağırmaz** (her kayıt tüm public cache'i boşaltmasın); kontenjan/durum alanları cache'lenmez, her istekte hafif bir projection ile hesaplanır. Mimari testteki "her mutasyon invalidator çağırır" kuralı bu istisnayı açıkça tanımlar.

**Test komutu (görev içinde):**

```bash
dotnet test GenclikMerkezi.slnx --filter "FullyQualifiedName~Website|FullyQualifiedName~Architecture" --verbosity quiet --nologo
```

Her görevde **unit testlerin yanında integration testleri zorunludur** (SQLite + HTTP). Tam paket yalnızca Görev 6'da çalışır.

**Commit:** Açık dosya listesiyle stage et (`git add .` kullanma). Push'tan sonra `git log origin/main -1` ile doğrula.

**Çelişki:** Kod veya ADR ile çelişki bulursan kendin karar verme; AGENTS.md §6 formatında dur ve raporla.

**Rapor (kısa):** commit hash, migration, filtreli test sayıları, sapmalar ve varsayımlar.

---

## Görev 1 — `EventSchedule` aggregate'i ve yönetimi

**Oku:** ADR-024 §4 (`ContentType` bayrakları, `SortMode`), §11.1; `ContentItem` ve `ContentType.SupportsEvent` kodu; Faz 1b'deki içerik kopyalama ve kalıcı silme kodu; `IHtmlContentSanitizer`.

**Domain (`EventSchedule`):**

- `ContentItemId` (benzersiz, 1:1), `StartsAtUtc`, `EndsAtUtc` (> başlangıç), `Format` (`InPerson`, `Online`, `Hybrid`), `OnlineLink` (opsiyonel; mutlak `https`; `Online`/`Hybrid` için zorunlu), `Capacity` (null = sınırsız; ≥ 1), `RegistrationEnabled`, `RegistrationOpensAtUtc`, `RegistrationClosesAtUtc` (ikisi de opsiyonel; kapanış ≤ `StartsAtUtc`), `MinAge`/`MaxAge` (opsiyonel, **yalnızca bilgi amaçlı**; doğum tarihi toplanmaz, kayıtta yaş denetimi yoktur), `AutoConfirm`, `WaitlistEnabled`, `IsCancelled`, `CancelledAtUtc`, `CancellationReason` (maks 500), sayaçlar `ConfirmedCount` ve `WaitlistedCount`, `RowVersion`, audit alanları.
- Dile göre (`EventScheduleTranslation`): `VenueName`, `VenueAddress`, `FeeInfo` (metin), `Instructors` (metin), `ProgramFlow` (sanitize edilmiş zengin metin), `AccessibilityNote`.
- Sayaç kuralları domain metotlarıyla korunur: `ConfirmedCount ≤ Capacity`; sayaçlar negatife inemez; kapasite mevcut `ConfirmedCount`'un altına düşürülemez (`Event.CapacityBelowConfirmed`). Sayaçlar yalnızca bu aggregate'in metotlarıyla değişir (Görev 3–4 kullanır); yönetim ucu sayaç yazamaz.
- Yalnızca `ContentType.SupportsEvent` açık türlerdeki içeriğe takvim bağlanabilir. Bağlandıktan sonra türü `SupportsEvent` kapatılamaz (mevcut tür güncelleme kuralına eklenir, takvimi olan tür bayrağı kapatılamaz).
- Başlangıç zamanı geçmiş etkinliğin kapasite ve kayıt ayarları değiştirilemez; yalnızca metinler ve iptal güncellenebilir.

**Yönetim uçları (`Website.Content.Manage`):**

- `GET /api/v1/admin/website/contents/{contentItemId}/event` — takvim detayı (sayaçlarla).
- `PUT .../event` — oluşturma/güncelleme (`RowVersion`; yoksa oluşturur). Dile göre çeviri alanları aynı gövdede.
- `POST .../event/cancel` (`reason`, `RowVersion`) ve `POST .../event/reactivate`. İptal e-posta fan-out'u Görev 4'tedir; bu görevde yalnızca durum geçişi ve `IEventCancellationNotifier` portu (boş implementasyon) vardır.
- `DELETE .../event` — yalnızca kayıt yoksa (Görev 3 sonrası kontrol bağlanır; bu görevde port `IEventRegistrationUsageChecker`, her zaman `false`).

**Mevcut koda etkisi:** İçerik kopyalama takvimi de kopyalar (kopya: sayaçlar 0, `IsCancelled = false`, `RegistrationEnabled = false`, tarihler aynı). Kalıcı silme takvimi siler; kayıtlı etkinliğin içeriği kalıcı silinemez (`Event.HasRegistrations`, Görev 3'te bağlanır).

**Testler:** Tarih ve kapasite kuralları (tablo testi); `OnlineLink` zorunluluğu; `SupportsEvent` olmayan türde reddi; geçmiş etkinlikte kısıtlı güncelleme; kopyalama; çeviri varsayılan dil zorunluluğu; yetki (integration).

**Commit:** `feat(website): add event schedule aggregate with capacity and registration window`

---

## Görev 2 — Public etkinlik uçları, takvim sıralaması ve `.ics`

**Oku:** Görev 1'in kodu; `GetPublicContents` / `GetPublicContentById` ve liste sorgusu sıralama kodu (`SortMode`); ADR-024 §4 (`EventDateAsc` notu); Faz 1b public cache kodu.

- **`SortMode: EventDateAsc` artık gerçektir:** `SupportsEvent` türlerde liste sorgusu `StartsAtUtc` artan sıralar; takvimi olmayan içerik sona düşer. ADR-024'teki "şimdilik `PublishDateDesc` gibi davranır" notu kaldırılır (Görev 6'da doküman).
- **`GET /api/v1/public/events`** (`public-read`): filtreler `typeKey`, `when` (`upcoming` varsayılan / `past` / `all`), `from`, `to`, `format`, `lang`, sayfalama. Yalnızca yayında ve görünür içerikler. Her satır: içerik özeti (başlık, kapak, slug/yol) + `event` özeti: `startsAtUtc`, `endsAtUtc`, `format`, `venueName`, `isCancelled`, `registrationState`, `remainingSpots`. `past` sıralaması azalan.
- **`registrationState`** (hesaplanır, saklanmaz): `NotOpen`, `Open`, `Full` (yedek kapalı), `WaitlistOpen`, `Closed`, `Cancelled`. Kural tek bir saf fonksiyonda (`EventRegistrationStateResolver`) tanımlanır; Görev 3'teki kayıt komutu da bunu kullanır. `remainingSpots`: kapasite varsa `Capacity - ConfirmedCount` (≥ 0), yoksa `null`.
- **İçerik detayına (`GET /api/v1/public/contents/{id}`) `event` alanı:** özetin tamamı + `programFlow`, `feeInfo`, `instructors`, `accessibilityNote`, `venueAddress`, `minAge`/`maxAge`, `registrationOpensAtUtc`/`ClosesAtUtc`. **`OnlineLink` hiçbir public yanıtta yer almaz.**
- **`GET /api/v1/public/events/{contentItemId}/calendar.ics?lang=`:** RFC 5545 uyumlu tek `VEVENT`: kararlı `UID` (`{contentItemId}@{site alan adı}`), `DTSTART`/`DTEND` UTC, `SUMMARY`, `LOCATION` (yüz yüze/hibritte mekân; salt online'da "Online"), `DESCRIPTION` (kısa özet + sitedeki etkinlik sayfası linki), `URL`; iptal edilmişse `STATUS:CANCELLED`. CRLF satır sonları, 75 oktet katlama, metin kaçışı. Online link ve kişisel veri **yoktur**. `Content-Type: text/calendar; charset=utf-8`.
- **Cache:** içerik ve takvim metinleri mevcut public cache'ten; `registrationState` ve `remainingSpots` her istekte hesaplanır (bkz. §1 Cache).

**Testler:** `EventDateAsc` sıralaması; `upcoming`/`past`/`all` ve tarih filtreleri (zaman sabitlenerek); `registrationState` tablo testi (pencere, kapasite, yedek, iptal); `OnlineLink`'in hiçbir yanıtta olmaması; `.ics` biçimi (katlama, kaçış, iptal, UID kararlılığı); gizli/yayında olmayan içerik 404 (integration).

**Commit:** `feat(website): add public event listing, calendar sorting and ics export`

---

## Görev 3 — Kayıt: oluşturma, doğrulama, iptal linki, kontenjan

**Oku:** Görev 1–2'nin kodu; Faz 3 Görev 1 (`IPublicSubmissionGuard`), Görev 2 (`LegalDocument`, yürürlükteki sürüm çözümleyici), Görev 6 (bülten onay/çıkış token deseni ve `UnsubscribeToken` yaklaşımı); `IWebsiteEmailSender`; `SiteSettings`.

**Yeni ayarlar:** `SiteSettings.EventPrivacyNoticeKey` (örn. `kvkk-event`); yapılandırma `Website:PublicSiteBaseUrl` (Faz 3'te var).

**Domain (`EventRegistration`):** `EventScheduleId`/`ContentItemId`, `FirstName`, `LastName`, `Email` (normalize: trim + küçük harf), `Phone` (opsiyonel), `UserId` (opsiyonel), `LanguageCode`, `Status` (`PendingVerification`, `Applied`, `Confirmed`, `Waitlisted`, `Rejected`, `Cancelled`, `Attended`, `NoShow`), onaylanan aydınlatma anahtarı + sürümü, `CreatedAtUtc`, `VerifiedAtUtc`, `StatusChangedAtUtc`, `WaitlistedAtUtc`, `CancelledAtUtc`, `CancelledBy` (`Participant`/`Admin`/`Event`), `VerificationTokenHash` + süre, `CancelToken` (rastgele; DB'de doğrulanır, bülten çıkış tokenı deseni), `AnonymizedAtUtc`, `RowVersion`. Durum geçişleri yalnızca domain metotlarıyla; her geçiş `StatusHistory`'ye (zaman, kim, eski/yeni) yazılır. Geçiş tablosu ADR-024 §11.2'deki diyagramdır.

**Public uçlar:**

- `POST /api/v1/public/events/{contentItemId}/registrations` (`public-forms`): guard alanları (`SubmissionToken`, `TurnstileToken`, `Website`), `firstName`, `lastName`, `email`, `phone?`, `lang`, `acceptedPrivacyNoticeVersion`.
  - İçerik görünür ve takvimli olmalı; `registrationState` `Open` veya `WaitlistOpen` olmalı (değilse `409`, kod `Event.RegistrationClosed` / `Event.CapacityFull` / `Event.Cancelled`). Aydınlatma sürümü yürürlükteki değilse `409` `PublicSubmission.LegalVersionChanged` (Faz 3 ile aynı).
  - **`UserId` yalnızca token'dan** alınır; istemciden gelen kimliğe güvenilmez (AGENTS §26). Ad, e-posta, telefon her durumda kayıt anındaki haliyle saklanır.
  - **Anonim kayıt** `PendingVerification` olur: kontenjan **tutmaz**; 24 saat geçerli imzalı doğrulama linki e-postayla gider (`PublicSiteBaseUrl` altında frontend sayfası; GET ile durum değiştiren uç yoktur). **Giriş yapmış ve e-postası doğrulanmış kullanıcı** için doğrulama atlanır, kayıt doğrudan §"Kontenjan kararı"na girer. Kullanıcının e-posta doğrulamasının nasıl öğrenileceğine bak (token claim'i veya Identity'nin public contract'ı); modül sınırını aşmadan çözülemiyorsa **dur ve AGENTS.md §6 formatında raporla**.
  - **Yinelenen kayıt:** aynı etkinliğe aynı e-postayla aktif (`PendingVerification`, `Applied`, `Confirmed`, `Waitlisted`) ikinci kayıt oluşturulmaz. Kayıt sızdırmamak için anonim yanıt tekdüzedir (`202`); mevcut kayıt `PendingVerification` ise doğrulama e-postası yeniden gönderilir (Görev 3'teki yeniden gönderme sınırına tabi), diğer aktif durumlarda e-posta adresine "zaten kayıtlısınız" bildirimi ve iptal linki gönderilir. Giriş yapmış kullanıcıya doğrudan `409` (`Event.AlreadyRegistered`) dönülebilir.
- `POST /api/v1/public/event-registrations/verifications` (`token`) → doğrulama. Doğrulanınca §"Kontenjan kararı" çalışır. Süresi dolmuş/kurcalanmış token: tek tip hata.
- `POST /api/v1/public/event-registrations/verification-resends` (`contentItemId`, `email`, `public-forms` + guard): tekdüze `202`; kayıt başına 10 dakikada en fazla 1 gönderim.
- `POST /api/v1/public/event-registrations/cancellations` (`token` = e-postadaki iptal linki; süre sınırı yok, etkinlik başlayana kadar geçerli; guard gerektirmez, `public-forms` limiti uygulanır) → `Cancelled` (`CancelledBy = Participant`). Kontenjanı tutan bir kayıt iptal edilirse sayaç düşer; **otomatik terfi yoktur**. Başlamış etkinlikte iptal `409`. Aynı token tekrar kullanılırsa idempotent (`200`, değişiklik yok).

**Kontenjan kararı (doğrulanmış kayıt için, tek yerde):**

1. Etkinlik iptal/kapalıysa reddet (yukarıdaki kodlar).
2. Kapasite sınırsız veya `ConfirmedCount < Capacity`: `AutoConfirm` açıksa `Confirmed` (+1 `ConfirmedCount`), kapalıysa `Applied` (sayaç değişmez).
3. Kapasite dolu: `WaitlistEnabled` açıksa `Waitlisted` (+1 `WaitlistedCount`, `WaitlistedAtUtc` set), değilse `409 Event.CapacityFull` (anonimde kayıt zaten `PendingVerification` olduğundan e-postadaki doğrulama sayfası bu hatayı gösterir; kayıt `Rejected` olmaz, silinme job'ına bırakılır).

Kayıt ve sayaç güncellemesi tek Unit of Work'te; `EventSchedule.RowVersion` çakışmasında en fazla 3 yeniden deneme, sonra `409`.

**E-postalar (commit sonrası, dile göre):** doğrulama; `Applied` ("başvurunuz alındı"); `Confirmed` (etkinlik bilgisi, **online link yalnızca `Online`/`Hybrid` etkinlikte**, public `.ics` linki, iptal linki); `Waitlisted` (yedek listedesiniz + iptal linki). Başvuru dışı kişisel veri e-postaya konmaz.

**Mevcut koda etkisi:** `IEventRegistrationUsageChecker` gerçek implementasyonu bağlanır; kayıtlı etkinliğin takvimi ve içeriği kalıcı silinemez.

**Testler:** Kontenjan kararı tablo testi (`AutoConfirm` × dolu/boş × `WaitlistEnabled`); anonim kaydın kontenjan tutmaması; doğrulama token'ı (süre, kurcalama, tekrar); yinelenen kayıt (tekdüze yanıt); iptal linki (idempotent, başlamış etkinlik, sayaç düşüşü); `UserId`'nin istemciden alınmaması; eski yasal sürümle `409`; **eşzamanlılık:** kapasite 5, 20 eşzamanlı doğrulama (`Task.WhenAll`) → tam 5 kontenjanlı, kalanlar yedek/`409`, sayaçlar tutarlı (integration); e-posta içerikleri ve online linkin yalnızca onay e-postasında bulunması.

**Commit:** `feat(website): add event registration with verification, capacity control and cancellation link`

---

## Görev 4 — Kayıt yönetimi, yedek liste ve etkinlik iptali

**Oku:** Görev 3'ün kodu; Faz 3 Görev 5'in `PersonalDataAccessLog` ve yönetim uçları deseni; `IEventCancellationNotifier` portu (Görev 1).

**Yönetim uçları:**

| Uç | Policy |
|---|---|
| `GET /api/v1/admin/website/contents/{contentItemId}/event/registrations` — sayfalı; filtre `status`, `search` (ad/e-posta), sıralama kayıt zamanı; yedek listede `WaitlistedAtUtc` artan. Liste kişisel veri içerir → erişim kaydı `View`. Üstte sayaç özeti (Confirmed/Applied/Waitlisted/kalan). | `Website.Submissions.View` |
| `GET .../registrations/{id}` — detay, durum geçmişi, onaylanan yasal sürüm. Erişim kaydı `View`. | `Website.Submissions.View` |
| `POST .../registrations/{id}/confirm` | `Website.Submissions.Manage` |
| `POST .../registrations/{id}/waitlist` (yalnızca `WaitlistEnabled` ve `Applied`) | `Website.Submissions.Manage` |
| `POST .../registrations/{id}/reject` (`reason` opsiyonel, kişiye iletilmez) | `Website.Submissions.Manage` |
| `POST .../registrations/{id}/cancel` (admin iptali, `CancelledBy = Admin`) | `Website.Submissions.Manage` |
| `POST .../registrations/{id}/attended`, `/no-show` — yalnızca etkinlik başladıktan sonra, yalnızca `Confirmed`'dan | `Website.Submissions.Manage` |

Tüm durum uçları `RowVersion` alır. `confirm` kontenjan doluysa `409 Event.CapacityFull`; yedek listeden terfi de `confirm`'dir ve sayaçları (`-1 Waitlisted`, `+1 Confirmed`) aynı Unit of Work'te günceller. Kontenjanı tutan kayıtta `reject`/`cancel` sayacı düşürür. Kontenjanı tutan durumlar (`Confirmed`, `Attended`, `NoShow`) sayaçta kalır.

**E-postalar:** `confirm` → onay e-postası (Görev 3 şablonu, online link dahil); `reject` → bilgilendirme; admin iptali → bilgilendirme. Yedekten terfi aynı onay şablonunu kullanır.

**Etkinlik iptali fan-out'u (`IEventCancellationNotifier` gerçek implementasyonu):** etkinlik iptal edilince `Applied`, `Confirmed`, `Waitlisted` kayıtlara iptal bildirimi gider (sebep metni dahil; kişiye özel veri yok), kayıtların durumu **değişmez** (etkinlik zaten iptal; geçmiş korunur). Gönderim commit sonrası, best-effort, en fazla 200 alıcıyı tek seferde işler, fazlası için Hangfire job'ı ile parçalanır.

**Testler:** Durum geçiş tablosu (geçerli/geçersiz); sayaç tutarlılığı her geçişte (özellikle yedekten terfi ve dolu kontenjanda `confirm`); `attended`/`no-show` zaman kuralı; liste ve detay erişim kaydı yazar, özet sayaç uçları yazmaz; yetki ayrımı (View ile durum değiştirilemez); iptal fan-out alıcı kümesi ve parçalama; eşzamanlı `confirm` yarışı (integration).

**Commit:** `feat(website): add event registration management, waitlist promotion and cancellation notices`

---

## Görev 5 — Temizlik job'ları, saklama ve katılımcı dışa aktarımı

**Oku:** Görev 3–4'ün kodu; mevcut Hangfire job'ları (`CleanupExpiredCookieConsentRecordsJob`, Faz 3 anonimleştirme job'ı); Faz 3 Görev 6'daki CSV dışa aktarım kodu.

- **Doğrulanmamış kayıt temizliği:** Saatlik Hangfire job'ı, `CreatedAtUtc` üzerinden **24 saati** geçmiş `PendingVerification` kayıtları siler (ADR-024 §11.2). Sayaçlara dokunmaz (bu kayıtlar sayaçta yoktur). Tek seferde en fazla 500.
- **Saklama / anonimleştirme:** Günlük job; etkinlik **bitiminden 365 gün** sonra kayıtlardaki `FirstName`, `LastName`, `Email`, `Phone`, `UserId` temizlenir (`AnonymizedAtUtc`), durum, tarihler ve durum geçmişi kalır. Süre kodda sabittir, `SECURITY.md`'ye yazılır ve **hukuk danışmanınca teyit edilmelidir**. Anonimleştirilmiş kayıtta durum değiştirilemez; iptal/doğrulama token'ları geçersizleşir. Tek seferde en fazla 500.
- **Katılımcı dışa aktarımı:** `GET /api/v1/admin/website/contents/{contentItemId}/event/registrations/export?status=` (`Website.Submissions.View`), CSV: `referenceNo` yok; sütunlar `firstName,lastName,email,phone,status,registeredAtUtc,confirmedAtUtc,language`. UTF-8 BOM, `=`, `+`, `-`, `@` ile başlayan hücreler kaçışlanır. Erişim kaydı `Export` (`Detail`: etkinlik ve filtre). **Katılımcı listesi hiçbir koşulda public olmaz** (public uçlar yalnızca sayaç/`remainingSpots` döner; mimari test: public endpoint yanıt tipleri `EventRegistration` alanı içermez).

**Testler:** 24 saat sınırı (23:59 ve 24:01; zaman sabitlenerek) ve sayaçların değişmemesi; anonimleştirme (etkinlik bitiminden 364/366 gün, alanların temizlenmesi, kalan alanlar, anonimleşmiş kaydın değiştirilememesi); CSV biçimi, BOM, enjeksiyon kaçışı, durum filtresi; export erişim kaydı; yetki (integration).

**Commit:** `feat(website): add event registration cleanup, retention and participant csv export`

---

## Görev 6 — Doküman senkronizasyonu ve tam test

**Oku:** ADR-024 §4, §11, §12, §18 (faz tablosu); `ARCHITECTURE.md` §8.11; `SECURITY.md`'nin KVKK bölümü; bu fazın commit'leri.

- **ADR-024:** `EventSchedule` alanları ve `AutoConfirm`; kontenjanı tutan durumlar ve sayaç kuralları; kontenjan kararı akışı ve `409` kodları; yinelenen kayıt ve tekdüze yanıt; 24 saatlik doğrulama ve temizlik; iptal linki ve idempotans; yedekten otomatik terfi olmaması; etkinlik iptali bildirim fan-out'u; `EventDateAsc`'ın artık gerçek davranışı (§4'teki "şimdilik" notunun kaldırılması); public yanıtlarda `OnlineLink`'in olmaması; `.ics` kapsamı; saklama süresi; faz tablosunda Faz 4 ✅ ve **kapsam dışı**: hatırlatma e-postası, etkinlik tarihi değişince kayıtlılara bildirim.
- **SECURITY.md:** katılımcı verisi erişim kaydı; anonimleştirme süresi (**hukuk danışmanı teyidi gerekli**); online linkin yalnızca onay e-postasında gitmesi; CSV enjeksiyonu kaçışı; katılımcı listesinin public olmaması; `UserId`'nin yalnızca token'dan alınması.
- **ARCHITECTURE.md §8.11:** Faz 4'ün tamamlandığı; kayıt işlemlerinin public cache'i boşaltmaması kuralı; Faz 5'in kapsamı.
- **Tam test paketi:** `dotnet test GenclikMerkezi.slnx`, sayılarıyla.

**Commit:** `docs(website): sync ADR-024, security and architecture docs with Faz 4`

**Faz sonu raporu (kısa):** görev başına commit hash'i, tam paket sayıları, tüm sapmalar ve ertelenen işler.

---

## Kapsam dışı

Etkinlik öncesi hatırlatma e-postası; etkinlik tarihi/yeri değişince kayıtlılara bildirim; yedekten otomatik terfi; çoklu oturum/tekrarlayan etkinlik; ücretli kayıt ve ödeme; QR giriş, yoklama ekranı, sertifika ve katılım puanı; XLSX dışa aktarım; kayıtta yaş denetimi; arama, sitemap, schema.org (Faz 5); admin ve public frontend'leri.
