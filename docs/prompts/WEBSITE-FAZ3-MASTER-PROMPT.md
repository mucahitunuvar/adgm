# Website Modülü — Faz 3 (Etkileşim: Yasal Metinler, Form Motoru, Bülten, Çerez ve Script Yönetimi) Master Prompt

## 0. Nasıl kullanılır

Bu dosya **görev görev** uygulanır. Her görev ayrı bir Claude Code oturumunda yürütülür:

1. Kullanıcı `/clear` yapar ve şunu gönderir: *"docs/prompts/WEBSITE-FAZ3-MASTER-PROMPT.md dosyasındaki §1'i ve Görev N'i oku, yalnızca Görev N'i uygula."*
2. Yalnızca **§1** ve **ilgili görevi** okursun. Görevin **"Oku"** listesi dışındaki dokümanları okuma; AGENTS.md'nin yalnızca belirtilen bölümlerine bak.
3. Önceki görevlerin kodu commit'lenmiştir; ihtiyaç duyduğunda `git log` ve kaynak dosyalarından öğren.
4. Görev bitince commit'le, push'la, kısa raporunu yaz ve **dur**.

---

## 1. Genel kurallar (her görevde geçerli)

**Kullanıcı kararları (Faz 3 için alındı):**

- Yasal metinler ve formlar **seed edilmez**; tamamen panelden oluşturulur, dile göre çevrilir ve sürümlenir.
- Formlarda **dosya yükleme** vardır: alan başına tek dosya; izin verilen türler admin tarafından güvenli bir listeden seçilir (PDF, DOCX, JPG, PNG); en fazla 10 MB; özel depolama köküne yazılır.
- Başvuru saklama **iki aşamalıdır**:
  - **Arşiv:** `Completed` veya `Rejected` durumundaki başvuru, bu duruma geçişinden **30 gün** sonra otomatik arşivlenir. Ana listede görünmez, arşiv filtresiyle tüm içeriğiyle açılır. Açık başvurular arşivlenmez.
  - **Anonimleştirme:** Her formun saklama süresi vardır (**varsayılan 730 gün**, form bazında değiştirilebilir). Süre başvuru tarihinden itibaren işler; dolunca kişisel veriler ve dosyalar silinir, istatistik için referans numarası, tarih, form ve durum kalır.
- Başvuru bildirimleri: başvurana referans numaralı onay e-postası; formun yetkililerine **içerik içermeyen**, yalnızca admin paneline link veren bildirim.
- Bülten: abone yönetimi ve CSV dışa aktarım; gönderim aracı yok.
- Çerez onayı tarayıcıda tutulur; ek olarak sunucuda **anonim** bir onay kaydı tutulur (IP ve kimlik bilgisi yok).

**Mimari:** Faz 0–2 desenleri: vertical slice, `RowVersion` ile iyimser eşzamanlılık (`409`), `TimeProvider`, projection + `AsNoTracking`, Result deseni, zengin metinler `IHtmlContentSanitizer` ile handler'da temizlenir, dile bağlı metinler `...Translation` child entity'lerinde ve varsayılan dilde zorunlu. Yönetim uçları `/api/v1/admin/website/...`, public uçlar `/api/v1/public/...`.

**Yetkilendirme:**

| İşlem | Policy |
|---|---|
| Yasal metinler, form tanımları, script tanımları | `Website.Settings.Manage` |
| Başvuruları ve aboneleri görüntüleme, dosya indirme, dışa aktarma | `Website.Submissions.View` |
| Başvuru durumu, atama, iç not | `Website.Submissions.Manage` |

**Kişisel veri erişim kaydı:** Identity'deki merkezi audit log başka modüllerce kullanılamadığı için Website kendi `PersonalDataAccessLog` tablosunu tutar (Görev 5'te eklenir): kim, ne zaman, hangi kayda, hangi işlem (görüntüleme, dosya indirme, dışa aktarma). Kişisel veri içeren her okuma ucu bu kaydı yazar.

**Anonim yazma uçları** (form gönderimi, bülten aboneliği, çerez onayı) Görev 1'deki `IPublicSubmissionGuard` ve `public-forms` rate limit'ini kullanır.

**E-postalar** `IWebsiteEmailSender` portu üzerinden, commit **sonrasında**, best-effort gönderilir (ADR-022 deseni). E-posta şablonları kodda tek bir yerde, dile göre tutulur.

**Cache:** Public okumaları etkileyen her mutasyon `WebsiteCacheInvalidator.InvalidateAllPublic` çağırır; mimari test yeni handler'ları kapsar.

**Test komutu (görev içinde):**

```bash
dotnet test GenclikMerkezi.slnx --filter "FullyQualifiedName~Website|FullyQualifiedName~Architecture" --verbosity quiet --nologo
```

Her görevde **unit testlerin yanında integration testleri zorunludur**: yeni endpoint'ler ve EF eşlemeleri gerçek veritabanı (SQLite) ve HTTP üzerinden test edilir. Tam paket yalnızca Görev 8'de çalışır.

**Commit:** Açık dosya listesiyle stage et (`git add .` kullanma). Push'tan sonra `git log origin/main -1` ile doğrula.

**Çelişki:** Kod veya ADR ile çelişki bulursan kendin karar verme; AGENTS.md §6 formatında dur ve raporla.

**Rapor (kısa):** commit hash, migration, filtreli test sayıları, sapmalar ve varsayımlar.

---

## Görev 1 — Anonim gönderim koruması

**Oku:** ADR-024 §12.3; `Application/Abstractions/IBotProtectionVerifier.cs` ve implementasyonu; `SiteSettings` (`BotProtectionEnabled`, `TurnstileSiteKey`); Host `Program.cs`'teki rate limit tanımları ve Data Protection yapılandırması.

- **`public-forms` rate limit policy'si** (Host): IP başına dakikada 10 istek; test ortamında mevcut desene uygun yüksek limit.
- **Gönderim token'ı:** `GET /api/v1/public/submission-tokens` (anonim, `public-forms`). Data Protection `ITimeLimitedDataProtector` ile imzalanmış, düzenlenme zamanını içeren, 2 saat geçerli bir token döner. İstemcinin gönderdiği zamana güvenilmez.
- **`IPublicSubmissionGuard`** (Application): sırasıyla
  1. token geçerli ve süresi dolmamış mı,
  2. token'ın düzenlenmesinden bu yana en az **3 saniye** geçmiş mi,
  3. honeypot alanı (`website` adlı, istemcide gizli) boş mu,
  4. `BotProtectionEnabled` açıksa `IBotProtectionVerifier` (Turnstile token'ı ve istemci IP'siyle).
- Başarısızlıkta tek tip, ayrıntı vermeyen bir hata döner (`400`, kod `PublicSubmission.Rejected`); hangi adımın başarısız olduğu yalnızca structured log'a yazılır.
- Guard'ın kullanacağı ortak istek alanları için bir kayıt tanımla (`SubmissionToken`, `TurnstileToken`, `Website`); Görev 4, 6 ve 7'deki istekler bunu içerir.

**Testler:** Her adımın başarı ve başarısızlık yolu (süresi dolmuş, kurcalanmış, çok hızlı gönderilmiş token; dolu honeypot; Turnstile reddi; bot koruması kapalıyken Turnstile'ın atlanması); token endpoint'inin `public-forms` limitine bağlı olduğu (integration).

**Commit:** `feat(website): add anonymous submission guard with token, honeypot and timing checks`

---

## Görev 2 — Yasal metinler (`LegalDocument`)

**Oku:** ADR-024 §12.1; mevcut bir çevirili aggregate örneği (`Video` veya `Partner`); `IHtmlContentSanitizer`.

**Domain:**

- `LegalDocument` aggregate: `Key` (`[a-z0-9-]`, maks 50, benzersiz, değiştirilemez; örn. `kvkk-contact`), `Kind` (`PrivacyNotice` / aydınlatma, `ExplicitConsent` / açık rıza, `CookiePolicy`, `TermsOfUse`, `Other`), dile göre `Title` (yönetim adı ve public başlık), `RowVersion`, audit alanları.
- `LegalDocumentVersion` (child entity): `VersionNumber` (aggregate içinde 1'den başlayarak artan tamsayı), `Status` (`Draft` / `Published` / `Superseded`), `EffectiveAtUtc`, `PublishedAtUtc`, `PublishedByUserId`, `ChangeSummary` (maks 500; yönetim için); dile göre `Body` (sanitize edilmiş zengin metin).
- Kurallar:
  - Aynı anda en fazla bir `Draft` sürüm vardır; yeni sürüm taslağı istenirse mevcut yayındaki sürümün metinleri kopyalanarak oluşturulur.
  - Yayınlanmış sürüm **değiştirilemez ve silinemez**.
  - Yayınlama: varsayılan dilde gövde dolu olmalı; `EffectiveAtUtc` verilmezse yayın anıdır. Yayınlanan sürüm `Published` olur, önceki `Superseded` olur.
  - Gelecek tarihli `EffectiveAtUtc` ile yayınlanan sürüm, o tarihe kadar yürürlükte olan sürümün yerini almaz; **yürürlükteki sürüm** "`Published` veya `Superseded` olup `EffectiveAtUtc <= now` olan en yüksek sürüm"dür. Bu kural tek bir expression olarak tanımlanır.
  - `Draft` sürüm silinebilir.
  - Hiç yayınlanmış sürümü olmayan doküman silinebilir; bir form tarafından kullanılıyorsa silinemez (Görev 3 bu kontrolü bağlar: `ILegalDocumentUsageChecker` portu bu görevde kurulur).

**Yönetim uçları (`Website.Settings.Manage`):** liste (her doküman için yürürlükteki sürüm numarası ve taslak durumu), detay (tüm sürümler), oluşturma, başlık çevirisi güncelleme, taslak oluşturma, taslak gövdesini dile göre güncelleme (`RowVersion`), taslağı yayınlama (`EffectiveAtUtc` opsiyonel), taslağı silme, doküman silme.

**Public uçlar (anonim, `public-read`):**

- `GET /api/v1/public/legal-documents/{key}?lang=` — yürürlükteki sürüm: başlık, gövde, sürüm numarası, yürürlük tarihi. Yürürlükte sürümü yoksa veya o dilde gövde yoksa `404`.
- `GET /api/v1/public/legal-documents/{key}/versions/{versionNumber}?lang=` — geçmiş bir sürümü gösterir (bir kişinin onayladığı metni sonradan göstermek için).
- Cache'lenir; TTL bir sonraki `EffectiveAtUtc` anına göre kısaltılır.

**Testler:** Sürüm yaşam döngüsü; yayınlanmış sürümün değiştirilemezliği; gelecek tarihli yürürlük (zaman sabitlenerek, birim testte); public uçlar (integration).

**Commit:** `feat(website): add versioned legal documents managed from the panel`

---

## Görev 3 — Form tanımları ve içerik–form bağlantısı

**Oku:** ADR-024 §12.2; Görev 2'nin kodu; `ContentType.SupportsForm`; `ContentItem` ve Faz 1b'deki koleksiyon güncelleme desenleri; `GetPublicContentById`.

**Domain:**

- `FormDefinition` aggregate: `Key` (benzersiz, değiştirilemez), `IsActive`, `RetentionDays` (30–3650, varsayılan **730**), `NotificationEmails` (en fazla 10 adres), `RowVersion`, audit; dile göre `Title`, `Description` (sanitize), `SuccessMessage`, `SubmitButtonLabel`.
- **Yasal metin bağlantıları:** `PrivacyNoticeKey` (zorunlu; `Kind = PrivacyNotice` olan bir dokümana) ve opsiyonel `ExplicitConsentKeys` listesi (en fazla 3; `Kind = ExplicitConsent`). Aydınlatma ve her açık rıza ayrı onay kutusu olarak gösterilir (ADR-024 §12.1). Her açık rıza için `IsRequired` belirtilir.
- **Alanlar** (`FormField` child entity, sıralı, en fazla 30):
  - `Key` (`[a-z0-9_]`, form içinde benzersiz), `Type` (`Text`, `Email`, `Phone`, `Textarea`, `Select`, `MultiSelect`, `Checkbox`, `Date`, `File`), `IsRequired`, `SortOrder`.
  - Tipe göre kurallar: `Text`/`Textarea` için `MinLength`/`MaxLength` (maks 4000); `Select`/`MultiSelect` için seçenekler (anahtar + dile göre etiket, en az 2, en fazla 50); `Date` için opsiyonel min/max; `File` için `AllowedFileTypes` (`Pdf`, `Docx`, `Jpg`, `Png` alt kümesi, en az biri) ve `MaxSizeMb` (1–10).
  - Dile göre `Label`, `Placeholder`, `HelpText`.
  - Formda en fazla 3 `File` alanı.
- **Tanım sürümü:** Her alan değişikliği `DefinitionVersion` değerini artırır. Gönderim anında alanların o anki tanımı başvuruya kopyalanır (Görev 4), böylece sonradan değişen form eski başvuruların okunmasını bozmaz.
- **Aktivasyon:** Form ancak bağlı aydınlatma metninin (ve zorunlu açık rızaların) **yürürlükte bir sürümü** varsa aktif edilebilir. Aktif bir formun bağlı olduğu yasal dokümanlar silinemez (`ILegalDocumentUsageChecker` gerçek implementasyonu).

**Yönetim uçları (`Website.Settings.Manage`):** liste, detay, oluşturma, temel alanların güncellenmesi (`RowVersion`), çeviri `PUT`/`DELETE`, alan listesinin tamamının değiştirilmesi (`PUT .../fields`, `RowVersion`), `activate`/`deactivate`, silme (başvurusu olan form silinemez, yalnızca pasife alınır).

**İçerik–form bağlantısı:**

- `ContentItem`'a `FormDefinitionId` eklenir; yalnızca `SupportsForm` türlerde atanabilir: `PUT /api/v1/admin/website/contents/{id}/form` (`RowVersion`, `Website.Content.Manage`; `null` ile kaldırılır).
- Kopyalama (Faz 1b) form bağlantısını da kopyalar; kalıcı silme bağlantıyı kaldırır.
- Public içerik detayına (`GET /api/v1/public/contents/{id}`) `form` alanı eklenir: bağlı form aktifse Görev 4'teki public form tanımıyla aynı biçimde; değilse `null`.

**Testler:** Alan kuralları (tablo testi); seçenek ve dosya kuralları; aktivasyonun yürürlükte yasal metin olmadan reddedilmesi; kullanımdaki yasal dokümanın silinememesi; `DefinitionVersion` artışı; içerik–form bağlantısı ve public detayda görünmesi (integration).

**Commit:** `feat(website): add form definitions with typed fields and content linking`

---

## Görev 4 — Public form gönderimi

**Oku:** Görev 1–3'ün kodu; ADR-019 ve Faz 0'daki `MediaAsset` dosya imzası doğrulaması (`FileSignatureValidator`); `IWebsiteEmailSender`; `SiteSettings`.

**Public uçlar:**

- `GET /api/v1/public/forms/{key}?lang=` (`public-read`): aktif formun o dildeki tanımı — başlık, açıklama, alanlar (etiket, tip, kurallar, seçenekler), buton metni, aydınlatma ve açık rıza dokümanlarının anahtarı, yürürlükteki sürüm numarası ve başlığı (frontend metni ayrı uçtan çeker). Form pasifse, o dilde çevirisi yoksa veya bağlı yasal metinlerin yürürlükte sürümü yoksa `404`.
- `POST /api/v1/public/forms/{key}/submissions` (`public-forms`, `multipart/form-data`):
  - Görev 1'in guard alanları; `lang`; alan değerleri; `File` alanları için dosyalar; `acceptedPrivacyNoticeVersion` ve her açık rıza için `{key, version, accepted}`; opsiyonel `contentItemId` (form bir içerikten gönderiliyorsa).
  - Doğrulama: her alan tanıma göre backend'de doğrulanır; bilinmeyen alan anahtarları reddedilir; e-posta ve telefon biçimleri; `contentItemId` verilmişse içerik görünür olmalı ve bu forma bağlı olmalı.
  - **Yasal onay:** Gönderilen aydınlatma sürümü yürürlükteki sürüm olmalıdır (değilse `409`, kod `PublicSubmission.LegalVersionChanged`; frontend metni yenileyip tekrar onay ister). Zorunlu açık rızalar `accepted = true` olmalıdır. Onaylanan sürümler başvuruya kaydedilir.
  - **Dosyalar:** uzantı, content-type ve **dosya imzası** doğrulanır (mevcut `FileSignatureValidator`'ı JPG/PNG/DOCX/PDF için kullan ya da genişlet); `FileCategory.WebsiteFormAttachment` ile **özel** köke yazılır. İstek boyutu limiti yalnızca bu uçta yükseltilir (3 × 10 MB + form verisi). Dosyalar veritabanı kaydından önce yazılır; kayıt başarısız olursa yazılan dosyalar silinir.
  - Giriş yapmış kullanıcı varsa `SubmittedByUserId` yalnızca token'dan alınır.
  - Yanıt: `referenceNumber` ve formun başarı mesajı.
- **Referans numarası:** `{önek}-{yıl}-{6 haneli sıra}` (örn. `GM-2026-000123`). Önek `SiteSettings`'e eklenen `SubmissionReferencePrefix` alanından gelir (varsayılan `GM`, `[A-Z]{2,6}`; taşınabilirlik için). Sıra yıl bazında, eşzamanlı gönderimlerde çakışmayacak şekilde bir sayaç tablosuyla (iyimser eşzamanlılık + yeniden deneme) üretilir; SQL Server'a özgü `SEQUENCE` kullanılmaz (SQLite testleri).

**Domain (`FormSubmission`):** `FormDefinitionId`, `DefinitionVersion`, alan tanımlarının anlık görüntüsü (JSON), `ReferenceNumber`, `LanguageCode`, `SubmittedAtUtc`, `SubmittedByUserId` (opsiyonel), `SourceContentItemId` (opsiyonel), yanıtlar (alan anahtarı → değer; JSON), dosya ekleri (`FileAttachment` + alan anahtarı), onaylanan yasal sürümler, `Status` (`New`), `RowVersion`. Durum yönetimi, arşiv ve anonimleştirme Görev 5'tedir.

**E-postalar (commit sonrası, best-effort):**

- Başvurana (formda bir `Email` alanı varsa ilk dolu e-posta alanına): referans numarası, form adı ve başarı mesajı, dile göre. Yanıtların kendisi e-postaya konmaz.
- Formun `NotificationEmails` adreslerine: form adı, referans numarası ve admin panelindeki başvuru detayına link. **Başvurunun içeriği ve başvuranın bilgileri e-postada yer almaz.** Admin panelinin temel adresi yapılandırmadan okunur (`Website:AdminPanelBaseUrl`).

**Testler:** Tüm doğrulama kuralları; eski yasal sürümle gönderimde `409`; zorunlu açık rıza; dosya türü, imza ve boyut reddi; dosyaların özel köke yazıldığı ve public köke yazılmadığı; kayıt başarısız olunca dosyaların silindiği; eşzamanlı 10 gönderimde referans numaralarının benzersiz ve ardışık olduğu (integration, `Task.WhenAll`); e-posta içeriklerinin başvuru verisi içermediği; içerikten gönderimde içerik doğrulaması.

**Commit:** `feat(website): add public form submissions with files, legal consent and reference numbers`

---

## Görev 5 — Başvuru yönetimi, arşiv ve anonimleştirme

**Oku:** Görev 4'ün kodu; mevcut Hangfire job'ları (`CleanupStaleNotFoundLogsJob`, `PermanentlyDeleteExpiredTrashJob`); `IFileStorageService`.

**Kişisel veri erişim kaydı:** `PersonalDataAccessLog`: `UserId`, `AccessedAtUtc`, `EntityType` (`FormSubmission`, `NewsletterSubscriber`), `EntityId` (dışa aktarmada `null`), `Action` (`View`, `DownloadFile`, `Export`), opsiyonel `Detail` (örn. dışa aktarmada filtreler). Yazma işlemi okuma isteğinin parçası olarak ayrı bir komutla yapılır (Faz 1a'daki 404 kaydı deseni: sorgu yazmaz). Yönetim ucu: `GET /api/v1/admin/website/personal-data-access-log` (sayfalı; `Website.Settings.Manage`).

**Durum ve işlem:**

- Durumlar: `New → InReview → AwaitingInfo → InReview`, `InReview → Approved / Rejected`, `Approved → Completed`. `Rejected` ve `Completed` kapanış durumlarıdır; kapanış durumundan `InReview`'a dönülebilir (yeniden açma). Durum geçişleri domain'de; her geçiş `StatusHistory`'ye (zaman, kullanıcı, eski/yeni durum) eklenir.
- `ClosedAtUtc`: kapanış durumuna geçişte set edilir, yeniden açılınca temizlenir.
- `AssignedToUserId` (atama), `InternalNotes` (child entity: yazan, zaman, metin; maks 2000; düzenlenemez, yalnızca eklenir).
- Arşivlenmiş başvuru yalnızca görüntülenebilir; durumu değiştirilmek istenirse önce arşivden çıkarılır.

**Arşiv:**

- `ArchivedAtUtc` alanı (durum değil, ayrı bir işaret). Hangfire günlük job'ı: `ClosedAtUtc` üzerinden **30 gün** geçmiş ve arşivlenmemiş başvuruları arşivler.
- Yönetici elle arşivleyebilir ve arşivden çıkarabilir (yalnızca kapanış durumundaki başvurular arşivlenebilir). Arşivden çıkarılan ve tekrar kapalı kalan başvuru job tarafından yeniden 30 gün sonra arşivlenir (sayaç arşivden çıkarma anından başlar).

**Anonimleştirme:**

- Hangfire günlük job'ı: `SubmittedAtUtc + form.RetentionDays` geçmiş ve anonimleştirilmemiş başvurular için tek transaction'da: yanıtlar, alan anlık görüntüsündeki etiketler dışındaki tüm değerler, iç notların metinleri ve `SubmittedByUserId` temizlenir; `AnonymizedAtUtc` set edilir. Dosyalar transaction commit'inden **sonra** depolamadan silinir; silme hatası loglanır ve job bir sonraki çalışmada tekrar dener (silinmemiş dosya referansları ayrı tutulur).
- Kalanlar: referans numarası, form, dil, tarihler, durum, durum geçmişi (kullanıcı ID'leri yöneticilere aittir, kalabilir).
- Anonimleştirme açık başvurularda da uygulanır (saklama süresi durumdan bağımsızdır). Anonimleştirilmiş başvurunun durumu değiştirilemez.
- Job, toplu işlemlerde tek seferde en fazla 500 başvuru işler.

**Yönetim uçları:**

| Uç | Policy |
|---|---|
| `GET /api/v1/admin/website/form-submissions` — sayfalı; filtre: `formKey`, `status`, `archived` (varsayılan `false`), `assignedToUserId`, `from`, `to`, `referenceNumber`. Liste **kişisel veri içermez**: referans numarası, form, durum, tarih, atanan kişi, ek sayısı. Erişim kaydı yazılmaz. | `Website.Submissions.View` |
| `GET .../form-submissions/{id}` — yanıtlar alan etiketleriyle eşleştirilmiş olarak (gönderim anındaki tanıma göre), dosya listesi, onaylanan yasal sürümler, durum geçmişi, iç notlar. Erişim kaydı: `View`. | `Website.Submissions.View` |
| `GET .../form-submissions/{id}/files/{fileId}` — dosyayı akış olarak döner (`Content-Disposition: attachment`). Erişim kaydı: `DownloadFile`. | `Website.Submissions.View` |
| `POST .../form-submissions/{id}/status`, `/assign`, `/notes`, `/archive`, `/unarchive` (`RowVersion`) | `Website.Submissions.Manage` |

**Testler:** Durum makinesi (geçerli ve geçersiz geçişler, yeniden açma); arşiv job'ı (kapanıştan 29 ve 31 gün sonra, açık başvuru, arşivden çıkarma sonrası yeniden sayım — zaman sabitlenerek); anonimleştirme job'ı (alanların temizlenmesi, kalan alanlar, dosyaların silinmesi ve başarısız silmenin tekrar denenmesi, 500 sınırı); liste ucunun kişisel veri döndürmemesi; detay ve dosya indirmenin erişim kaydı yazması; yetki ayrımı (View yetkisiyle durum değiştirilememesi — yapılandırma testi) (integration).

**Commit:** `feat(website): add submission management with auto-archive, anonymization and access log`

---

## Görev 6 — Bülten aboneliği

**Oku:** ADR-024 §14; Görev 1, 2 ve 5'in ilgili kodu; `SiteSettings.NewsletterEnabled`; Faz 1b'deki önizleme token'ı (Data Protection deseni).

**Domain (`NewsletterSubscriber`):** `Email` (normalize: trim + küçük harf; benzersiz), `LanguageCode`, `Status` (`PendingConfirmation`, `Active`, `Unsubscribed`), `SubscribedAtUtc`, `ConfirmedAtUtc`, `UnsubscribedAtUtc`, onaylanan aydınlatma metni anahtarı ve sürümü (bülten için kullanılacak doküman anahtarı `SiteSettings`'e eklenen `NewsletterPrivacyNoticeKey` alanından gelir), `RowVersion`.

**Akış:**

- `POST /api/v1/public/newsletter/subscriptions` (`public-forms`): guard alanları, `email`, `lang`, `acceptedPrivacyNoticeVersion`. `NewsletterEnabled = false` ise veya aydınlatma metninin yürürlükte sürümü yoksa `404`. Eski sürümle onayda `409` (Görev 4 ile aynı kod).
  - **Kayıt sızdırmaz:** E-posta zaten kayıtlı olsa da aynı yanıt döner (`202`). Kayıtlı ve `Active` ise e-posta gönderilmez; `PendingConfirmation` ise onay e-postası (en fazla saatte bir) yeniden gönderilir; `Unsubscribed` ise yeniden `PendingConfirmation`'a alınır ve onay e-postası gönderilir.
- Onay e-postasındaki link: imzalı, 7 gün geçerli token. `POST /api/v1/public/newsletter/confirmations` (`token`) → `Active`.
- Her bülten e-postasında kullanılacak abonelikten çıkma linki: imzalı, **süresiz** token (aboneye özgü; anahtar rotasyonuna dayanıklı olması için token, aboneye ait rastgele bir `UnsubscribeToken` değerini taşır ve veritabanında doğrulanır). `POST /api/v1/public/newsletter/unsubscriptions` (`token`) → `Unsubscribed`. Abonelikten çıkma, guard gerektirmez (tek tık çalışmalıdır) ama `public-forms` limitine tabidir.
- Frontend'in onay ve çıkış sayfaları için link adresi yapılandırmadan okunur (`Website:PublicSiteBaseUrl`); e-postadaki link frontend sayfasına gider, frontend token'ı API'ye iletir. GET ile durum değiştiren uç yazılmaz (e-posta güvenlik tarayıcılarının linki açıp aboneliği değiştirmesini önlemek için).

**Saklama:**

- `PendingConfirmation` durumunda 7 gün içinde onaylanmayan kayıtlar silinir.
- `Unsubscribed` kayıtlar 30 gün sonra silinir.
- Her ikisi tek bir günlük Hangfire job'ında.

**Yönetim uçları (`Website.Submissions.View`):**

- `GET /api/v1/admin/website/newsletter-subscribers` — sayfalı; filtre: `status`, `language`, `search` (e-postada). Erişim kaydı: `View` (liste e-posta içerdiği için).
- `GET .../newsletter-subscribers/export?status=Active&language=` — CSV (`email`, `language`, `confirmedAtUtc`). UTF-8 BOM'lu (Excel'de Türkçe karakterler için). Erişim kaydı: `Export`. CSV enjeksiyonuna karşı `=`, `+`, `-`, `@` ile başlayan hücreler kaçışlanır.
- `DELETE .../newsletter-subscribers/{id}` (`Website.Submissions.Manage`) — kişinin silme talebi için.

**Testler:** Akışın tüm durumları; kayıt sızdırmama (aynı yanıt); yeniden gönderim sınırı; token geçerlilik ve kurcalama; bülten kapalıyken `404`; saklama job'ı (zaman sabitlenerek); CSV biçimi, BOM ve enjeksiyon kaçışı; erişim kayıtları (integration).

**Commit:** `feat(website): add double opt-in newsletter subscriptions with export`

---

## Görev 7 — Çerez onayı ve script yönetimi

**Oku:** ADR-024 §13; `GetPublicSite` (yanıt ve cache); Görev 1 ve 2'nin kodu; `SECURITY.md`'nin çerez ve YouTube notu.

**Script tanımları (`ThirdPartyScript`):**

- Ham HTML/JavaScript **kabul edilmez** (yönetim panelinden XSS ve tedarik zinciri riskini önlemek için). Bunun yerine tipli sağlayıcılar:
  - `GoogleAnalytics4` — `MeasurementId` (`G-[A-Z0-9]{4,12}`)
  - `GoogleTagManager` — `ContainerId` (`GTM-[A-Z0-9]{4,10}`)
  - `MetaPixel` — `PixelId` (10–20 haneli sayı)
  - `ExternalScript` — `Src` (mutlak `https` URL; alan adı `Website:AllowedScriptHosts` yapılandırma listesinde olmalı), `Async`/`Defer`
- Ortak alanlar: `Category` (`Necessary`, `Analytics`, `Marketing`), `Placement` (`Head`, `BodyEnd`), `IsActive`, `SortOrder`, dile göre ziyaretçiye gösterilecek `Name` ve `Purpose` (çerez tercihleri panelinde listelenir), `RowVersion`.
- Yönetim uçları (`Website.Settings.Manage`): standart CRUD, çeviri, `activate`/`deactivate`.

**Çerez onay yapılandırması:**

- `SiteSettings`'e `CookiePolicyKey` alanı (bir `CookiePolicy` türündeki yasal dokümanın anahtarı) ve dile göre çerez banner'ı metinleri (`CookieBannerTitle`, `CookieBannerText`, kategori açıklamaları) eklenir. Bu alanlar mevcut grup bazlı `SiteSettings` güncelleme uçlarına yeni bir grup olarak (`PUT .../settings/cookie-consent`) eklenir.
- `GET /api/v1/public/site` yanıtına `cookieConsent` alanı: banner metinleri, kategoriler ve her kategorideki aktif script'lerin ad ve amaçları, çerez politikasının anahtarı ve yürürlükteki sürüm numarası; ayrıca `scripts` alanı: aktif script'lerin tipli tanımları (frontend yalnızca onay verilen kategorilerdekini yükler).
- **Politika sürümü değişince** frontend yeniden onay ister (yanıttaki sürüm, tarayıcıda saklanan sürümle karşılaştırılır). Bu kural ARCHITECTURE.md'ye yazılır (Görev 8).

**Anonim onay kaydı (`CookieConsentRecord`):**

- `POST /api/v1/public/cookie-consents` (`public-forms`; guard gerektirmez — banner her ziyaretçiye çıkar, token akışı orantısız olur): `consentId` (istemcinin ürettiği rastgele UUID; aynı tarayıcının sonraki tercih değişikliklerini bağlamak için), `categories` (onaylanan kategoriler), `policyVersion`, `action` (`AcceptAll`, `RejectAll`, `Custom`).
- Saklanan: `ConsentId`, kategoriler, politika anahtarı ve sürümü, `Action`, `RecordedAtUtc`. **IP, User-Agent, kullanıcı kimliği tutulmaz.**
- `Necessary` kategorisi her zaman onaylı kabul edilir; istemci göndermese de kayda eklenir.
- Politika sürümü yürürlükteki sürüm değilse `409`.
- Kayıtlar 3 yıl saklanır (günlük job ile silinir; süre sabit olarak tanımlanır ve SECURITY.md'ye yazılır).
- Yönetim: `GET /api/v1/admin/website/cookie-consents/summary?from=&to=` — yalnızca toplu sayılar (kategori ve eylem bazında); tekil kayıt listesi yok (`Website.Settings.Manage`).

**Testler:** Sağlayıcı ID doğrulamaları ve izin verilmeyen script host'unun reddi; ham script'in hiçbir yoldan kabul edilmediği; public site yanıtında yalnızca aktif script'ler; onay kaydının kişisel veri içermediği (kayıt alanlarının kontrolü); `Necessary`'nin otomatik eklenmesi; eski politika sürümüyle `409`; özet uç (integration).

**Commit:** `feat(website): add cookie consent records and typed third-party script management`

---

## Görev 8 — Doküman senkronizasyonu ve tam test

**Oku:** ADR-024 §2, §12, §13, §14, §18; `ARCHITECTURE.md` §8.11; `SECURITY.md`'nin KVKK ve çerez bölümleri; bu fazın commit'leri.

- **ADR-024:** yasal metinlerin panelden yönetilmesi, sürüm ve yürürlük kuralları; form alan modeli, tanım sürümü ve anlık görüntü; dosya yükleme kuralları; referans numarası biçimi ve önek ayarı; başvuru durum makinesi; **iki aşamalı saklama** (30 gün sonra arşiv, form bazında saklama süresi sonunda anonimleştirme, varsayılan 730 gün); bildirim e-postalarının içerik taşımaması; kişisel veri erişim kaydı; bülten akışı, kayıt sızdırmama ve saklama kuralları; tipli script sağlayıcıları ve ham script'in reddi; anonim çerez onay kaydı ve saklama süresi.
- **SECURITY.md:** kişisel veri erişim kaydı; anonimleştirme ve arşiv kuralları; form ekleri (özel kök, imza doğrulaması); bildirim e-postalarında kişisel veri olmaması; CSV enjeksiyonu kaçışı; çerez onay kaydında tutulmayan bilgiler; varsayılan saklama süresinin **hukuk danışmanınca teyit edilmesi gerektiği**.
- **ARCHITECTURE.md §8.11:** Faz 3'ün tamamlandığı; çerez politikası sürümü değişince frontend'in yeniden onay istemesi kuralı; Faz 4'ün kapsamı.
- **Tam test paketi:** `dotnet test GenclikMerkezi.slnx`, sayılarıyla.

**Commit:** `docs(website): sync ADR-024, security and architecture docs with Faz 3`

**Faz sonu raporu (kısa):** görev başına commit hash'i, tam paket sayıları, tüm sapmalar ve ertelenen işler.

---

## Kapsam dışı

Bülten gönderim aracı; form başvurularının CSV dışa aktarımı; form sonuçlarının raporlanması; etkinlik kaydı (Faz 4 — Görev 1'deki guard ve Görev 2'deki yasal metin altyapısını kullanacak); arama, sitemap, schema.org (Faz 5); admin ve public frontend'leri; ilgili kişi (KVKK) başvuru yönetimi modülü.
