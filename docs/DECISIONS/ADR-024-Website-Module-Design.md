# ADR-024: Website Modülü — Taşınabilir, Headless İçerik Yönetim Sistemi Tasarımı

## Durum

Kabul edildi.

## Bağlam

ARCHITECTURE.md §8.11, önceden ayrı planlanan CMS, Event ve Media sorumluluklarını tek bir **Website** modülünde birleştirdi. Modül şu an tamamen boş (`src/Modules/Website` altında yalnızca klasörler var).

Tasarım sürecinde kapsamı belirleyen girdiler:

- PROJECT.md §5.4, §13, §14 (kurumsal site bölümleri, web sitesi–platform ilişkisi)
- DOMAIN.md §21 (Event) ve CareerDevelopment'ın `Website.Training` referansı
- `genclikmerkeziweb` reposundaki deneme frontend'i ve ürün dokümanları (`ASSOCIATION_WEBSITE.md`, `FRONTEND_API_REQUIREMENTS.md`). Bu frontend bağlayıcı değildir; yalnızca fikir kaynağı olarak kullanıldı.
- Benzer dernek/vakıf sitelerinin yaygın uygulamaları (etki göstergeleri, şeffaflık, gönüllülük, etkinlik kaydı, destek yolları)

Belirleyici gereksinim: **Modül, bu projeye özgü olmamalı; başka projelerde yeniden yazılmadan kullanılabilmeli.** İçerik türleri, menüler, sayfa düzeni ve arama davranışı koddan değil, veriden yönetilmeli.

Mevcut kısıtlar:

- **CAP tek DbContext kısıtı (ADR-014, ADR-018 uygulama notu, ADR-022):** Identity dışındaki modüller kendi Outbox'ları üzerinden güvenilir event yayınlayamıyor. Modüller arası etkileşimler senkron public contract çağrısı veya Host seviyesi orkestrasyonla yapılıyor.
- **Yetkilendirme:** Backend'de yalnızca rol bazlı kontrol var (`RequireRole`), permission altyapısı yok.
- **Employer public uçları:** `GET /api/v1/jobs` sayfalama, filtre, slug ve detay içermiyor; public firma profili yok.

## Karar

### 1. Kurulum modeli ve taşınabilirlik

- **Tek kurulum (single-tenant).** Her proje kendi kurulumu ve kendi veritabanıyla çalışır. Multi-tenant (tek sistemden çok site) yapılmaz; tüm tablolara ve yetkilendirmeye tenant boyutu eklemenin maliyeti gerekçelendirilemez.
- **Website modülü hiçbir iş modülüne referans vermez** (Employer, Candidate, CareerDevelopment, Matching vb.). Yalnızca SharedKernel ve BuildingBlocks'a bağımlıdır (`ICurrentUserContext`, `IFileStorageService`, cache, pagination, Result).
- Projeye özgü bağlantılar Website'in tanımladığı **port** arayüzleri üzerinden Host katmanındaki adaptörlerle kurulur (ADR-022'deki Host-seviyesi orkestrasyon yaklaşımının devamı). Başka projede bu adaptörler yazılmaz ya da farklı yazılır; modül kodu değişmez.
- **Headless:** Backend HTML render etmez; yapılandırılmış veri döner. Frontend (hangi teknolojiyle olursa olsun) render eder.

Website'in tanımladığı portlar:

| Port | Amaç | Bu projedeki Host adaptörü |
|---|---|---|
| `IWebsiteEmailSender` | Doğrulama, bildirim, bülten e-postaları | Notification modülünün public contract'ı |
| `IBotProtectionVerifier` | Anonim formlarda bot doğrulaması | Cloudflare Turnstile (bkz. §12) |
| `IExternalSearchSource` | Dış kaynakların (ör. ilanlar) genel arama indeksine periyodik olarak çekilmesi | Employer public contract'ından yayındaki ilanları okuyan adaptör (bkz. §10) |

### 2. Yetkilendirme

- Website endpoint'leri **isimli policy** kullanır. Policy'ler şimdilik Admin rolüne çözülür (literal rol string'i; ADR-016 pattern'i, `Identity.Domain.UserRole`'a bağımlılık yok).
- İleride gerçek permission sistemi geldiğinde yalnızca policy tanımları değişir; endpoint'lere dokunulmaz.

| Policy | Kapsam |
|---|---|
| `Website.Content.Manage` | İçerik oluşturma/düzenleme/silme, medya, video |
| `Website.Content.Publish` | Yayınlama, yayından kaldırma, arşivleme |
| `Website.Structure.Manage` | ContentType tanımları, site dilleri (sistem yöneticisi işi) |
| `Website.Design.Manage` | Menü, sayfa düzeni, slider, pop-up, partner, tema |
| `Website.Settings.Manage` | SiteSettings, script/çerez yönetimi, yönlendirmeler |
| `Website.Submissions.View` | Form başvuruları, etkinlik kayıtları, bülten aboneleri (kişisel veri — erişim auditlenir) |
| `Website.Submissions.Manage` | Başvuru durumu, atama, iç not |

Public okuma endpoint'leri anonimdir ve IP bazlı rate limiting'e tabidir.

### 3. Çok dillilik

- Website kendi **`SiteLanguage`** tablosunu tutar: `Code` (`tr`, `en`), `Name`, `IsDefault`, `IsActive`, `SortOrder`. Başlangıçta yalnızca `tr` aktiftir.
- ReferenceData'daki `Language` lookup'ı (ADR-016) **kullanılmaz**; o, adayların konuştuğu dilleri temsil eden farklı bir kavramdır.
- Dile bağlı her alan, ilgili aggregate'in `...Translation` child entity'sinde durur (`LanguageCode` + alanlar). Varsayılan dilde çeviri zorunludur; diğer dillerde eksik çeviri varsa o içerik o dilde listelenmez.
- URL'ler dil önekli olur; varsayılan dil öneksiz sunulur (`/haberler/x`, `/en/news/x`). Diller arası karşılıklar `hreflang` için endpoint yanıtında döner.

### 4. İçerik çekirdeği: ContentType + ContentItem

İçerik türleri koddaki enum değil, **veridir**.

#### 4.1. ContentType

Yalnızca `Website.Structure.Manage` yetkisiyle yönetilir.

| Alan | Açıklama |
|---|---|
| `Key` | Sistem anahtarı (`news`, `event`), değiştirilemez |
| Çeviriler | Ad, URL öneki (`haberler` / `news`), liste sayfası SEO alanları |
| `ListTemplate`, `DetailTemplate` | Frontend'in hangi şablonla render edeceği (`cards`, `grid`, `list`, `team-grid`, `faq-accordion`…) |
| `SortMode` | `Manual` / `PublishDateDesc` / `EventDateAsc` |
| `IsActive` | Pasif tür: liste ve tüm detayları 404 döner |
| Özellik bayrakları | aşağıda |

Özellik bayrakları: `SupportsHierarchy`, `SupportsCategories`, `SupportsTags`, `SupportsDetailImage`, `SupportsGallery`, `SupportsVideos`, `SupportsAttachments`, `SupportsEvent`, `SupportsBlockLayout`, `SupportsForm`, `SupportsRelatedContent`, `HasDetailPage`, `HasListingPage`, `IsSearchable`, `RequiresReview`.

`RequiresReview` tasarımda yer alır ama bu projede tüm türler için kapalı başlar (bkz. §4.4).

Türe özgü özel alanlar (ör. yalnızca Proje'de bitiş tarihi) **bu ADR kapsamında yoktur.** İhtiyaç doğarsa tipli özel alan tanımı ayrı bir ADR ile eklenir; EAV modeli bilinçli olarak reddedilmiştir.

Bu proje için seed türleri: Sayfa, Haber, Duyuru, Proje, Faaliyet, Başarı Hikayesi, Etkinlik, Eğitim/Atölye, Gönüllülük Fırsatı, SSS, Ekip, Belge, Basın Bülteni.

#### 4.2. ContentItem

Dilden bağımsız alanlar:

- `ContentTypeId`, `ParentId` (yalnızca `SupportsHierarchy`)
- `Status`, `PublishAtUtc`, `UnpublishAtUtc`
- `SortOrder`, `IsFeatured`
- `CoverImageId` (listelerde), `DetailImageId` (detay sayfası hero'su)
- `Gallery` — sıralı `MediaAsset` referansları; öğe başına dile göre alt metin ve açıklama
- `Videos` — `Video` kütüphanesinden sıralı referanslar
- `Attachments` — sıralı dosya ekleri (`MediaAsset`)
- Kategoriler, etiketler, ilişkili içerikler, bağlı form (`FormDefinitionId`)
- Audit: `CreatedBy/At`, `UpdatedBy/At`, `PublishedBy/At`
- `DeletedAtUtc` (çöp kutusu)

Dile bağlı alanlar (`ContentItemTranslation`):

- `Title`, `Slug`, `Summary` (kısa açıklama), `Body` (zengin metin, sanitize edilmiş HTML)
- `Seo` value object: `MetaTitle`, `MetaDescription`, `MetaKeywords`, `OgTitle`, `OgDescription`, `OgImageId`, `CanonicalUrl` (opsiyonel override), `NoIndex`

`MetaKeywords` saklanır ve render edilir; ancak büyük arama motorlarınca dikkate alınmadığı bilinmektedir, SEO değeri beklenmez.

#### 4.3. Hiyerarşi ve URL

- **İç içe yol** kullanılır: `/kurumsal/hakkimizda`. Tam yol (`FullPath`) çeviri başına hesaplanıp saklanır; sorgu anında hesaplanmaz.
- Kurallar (domain invariant): parent aynı türden olmalı, döngü yasak, maksimum derinlik 3.
- Slug `Slug` value object'i ile üretilir: Türkçe karakter dönüşümü (ç→c, ğ→g, ı→i, ö→o, ş→s, ü→u), küçük harf, tire ayracı. (tür + dil + parent) içinde benzersizdir.
- Slug veya parent değiştiğinde etkilenen tüm alt içeriklerin `FullPath`'i güncellenir ve **eski her yol için otomatik 301 `Redirect` kaydı** oluşturulur.

#### 4.4. Durum modeli

Ayrı bir aktif/pasif alanı **yoktur**; tek durum alanı kullanılır:

```text
Draft ──► Published ◄──► Unpublished (pasif)
             │
             ▼
          Archived
```

- Yayında görünme koşulu: `Status == Published && (PublishAtUtc == null || PublishAtUtc <= now) && (UnpublishAtUtc == null || UnpublishAtUtc > now) && DeletedAtUtc == null`.
- Zamanlanmış yayın için arka plan job'ı gerekmez; koşul sorguda uygulanır.
- `RequiresReview` açık bir türde `Draft → InReview → Published` akışı ve yazan ≠ yayınlayan kuralı devreye girer. Bu projede kapalı.

#### 4.5. Diğer içerik davranışları

- **Önizleme linki:** Yayınlanmamış içerik için süreli, imzalı token'lı public önizleme URL'si üretilebilir.
- **Çöp kutusu:** Silme soft delete'tir; 30 gün içinde geri alınabilir, sonra kalıcı silinir (Hangfire recurring job).
- **Kopyalama:** Bir içerik, çevirileri ve medya referanslarıyla birlikte yeni bir taslak olarak kopyalanabilir.
- **İlişkili içerik:** Elle bağlanan içerikler; yoksa aynı kategoriden son yayınlananlar döner.

### 5. Video kütüphanesi

- `Video` aggregate'i: dile göre ad, `YouTubeVideoId`, opsiyonel `CoverImageId`, `SortOrder`, `IsActive`.
- Yalnızca YouTube kabul edilir; backend girilen linkten video ID'sini çıkarır ve doğrular. Kapak verilmezse YouTube küçük resmi kullanılır.
- Oynatma `youtube-nocookie.com` üzerinden yapılır (frontend sorumluluğu, KVKK gereği).
- İçerikler videoları bu kütüphaneden **sıralı liste** olarak seçer. Videolar sayfası kütüphanenin public listesidir.

### 6. Medya kütüphanesi

- `MediaAsset` aggregate'i ADR-019'daki `IFileStorageService` üzerine kurulur; ADR-019'daki kapalı `FileCategory` listesine Website kategorileri eklenir: `website-images`, `website-documents` (public) ve `website-form-attachments` (özel).
- Alanlar: dosya, klasör, dile göre alt metin ve açıklama, kaynak/telif, kullanım izni, `ContainsPersonalData`, genişlik/yükseklik, boyut, MIME, yükleyen.
- **Görsel işleme:** Yükleme anında **SkiaSharp (MIT)** ile birkaç sabit boyut üretilir (ör. `thumb` 320px, `card` 640px, `hero` 1600px) ve WebP'ye dönüştürülür; orijinal de saklanır. Kesin boyutlar Faz 0 master prompt'unda belirlenir.
- **Kullanım takibi:** Bir içerikte, blokta, slider'da vb. kullanılan medya silinemez; kullanıldığı yerler listelenir.
- Public medya uzun süreli cache header'larıyla doğrudan servis edilir. Form ekleri gibi özel dosyalar yalnızca yetkili endpoint'ten stream edilir.

### 7. Menüler

- `Menu` aggregate'i bir konuma bağlıdır (`Header`, `Footer`, `Utility`, `Mobile`; konum listesi genişletilebilir).
- İçinde iç içe `MenuItem` ağacı bulunur. Öğe türleri:
  - `Group` — linksiz başlık (kategorize etmek için)
  - `Content` — bir ContentItem'a link
  - `ContentTypeListing` — bir türün liste sayfasına link
  - `InternalPath` — sabit iç yol (`/portal/giris`)
  - `ExternalUrl`
- Alanlar: dile göre etiket, ikon, yeni sekmede açma, `IsActive`, `SortOrder`.
- Kurallar: döngü yok, maksimum derinlik 3. Bağlı içerik yayında değilse veya türü pasifse öğe public yanıtta otomatik gizlenir.
- Menü öğesini pasifleştirmek yalnızca linki gizler; bir bölümü tamamen kapatmak için ContentType pasifleştirilir.

### 8. Tasarım modülü

#### 8.1. Sayfa düzeni (blok tabanlı)

- Serbest sürükle-bırak builder **değildir**; kodla gelen blok tiplerinin seçilip sıralandığı, açılıp kapandığı ve ayarlandığı bir sistemdir. Her blok tipinin frontend'de karşılık gelen bir bileşeni vardır.
- `PageLayout` bir hedefe bağlanır: `Home` ya da `SupportsBlockLayout` açık bir ContentItem.
- `PageLayout` **taslak ve yayın** olmak üzere iki sürüm tutar; değişiklikler taslakta yapılır, önizlenir, yayınlanır.
- `LayoutBlock`: `BlockType`, `SortOrder`, `IsActive`, dile göre metinler, `Settings` (JSON). Ayarlar, backend'deki blok tipi kaydında tanımlı validator ile doğrulanır; tanımsız blok tipi veya geçersiz ayar kabul edilmez.
- İlk hedef ana sayfadır; yapı baştan her içeriğe bağlanabilir kurulur.

Başlangıç blok kataloğu: `HeroSlider`, `LogoStrip`, `QuickLinks`, `ContentList`, `UpcomingEvents`, `JobList`, `FeatureMosaic`, `ProcessSteps`, `VideoFeature`, `ImpactStats`, `Cta`, `RichText`, `ImageText`, `Faq`, `Gallery`.

`JobList` bloğu yalnızca ayar tutar (adet, filtre); veriyi frontend doğrudan Employer API'sinden çeker. Website, ilan verisini kopyalamaz.

#### 8.2. Bloklara veri sağlayan aggregate'ler

- **`Slider`**: sıralı slide'lar — masaüstü ve mobil görsel, dile göre başlık/metin/buton etiketi ve linki, `PublishAtUtc`/`UnpublishAtUtc`, `IsActive`.
- **`Partner`**: logo, ad, link, `SortOrder`, `IsActive` (logo şeridi ve iş birlikleri sayfası).
- **`ImpactMetric`**: dile göre etiket, değer, birim, **dönem** ve **kaynak** zorunlu. Doğrulanmamış rakam yayınlanmaz. İleride diğer modüllerden (ör. Employment'tan işe yerleşme sayısı) Host adaptörüyle beslenebilir; bu ADR kapsamında manuel girilir.

#### 8.3. Tema ayarları

`SiteSettings.Theme`: logo (açık/koyu), favicon, ana ve ikincil renk, font ailesi. Frontend bunları CSS değişkenlerine basar. Amaç, modülün başka projelerde kod değişikliği olmadan farklı kimlikle kullanılabilmesidir.

### 9. Pop-up ve duyuru şeridi

`Popup` aggregate'i iki görünüm moduna sahiptir: `Modal` ve `Banner` (sitenin üstünde ince bilgi bandı).

| Alan | Açıklama |
|---|---|
| Çeviriler | Başlık, içerik, buton etiketi |
| `ImageId`, `LinkUrl` | Opsiyonel |
| `Targeting` | Tüm site / ana sayfa / belirli içerikler / yol listesi |
| `DeviceTarget` | Hepsi / masaüstü / mobil |
| `PublishAtUtc`, `UnpublishAtUtc`, `IsActive` | Zamanlama |
| `DelaySeconds` | Gösterim gecikmesi |
| `Frequency` | Her ziyaret / oturumda bir / N günde bir (takip tarayıcıda) |
| `Priority` | Aynı anda tek modal gösterilir: en yüksek öncelikli |

Çerez onay banner'ı bu yapının dışındadır (bkz. §13).

### 10. Arama

İki katman:

**Liste içi arama ve filtre:** Her liste endpoint'i metin, kategori, etiket ve tarih aralığı filtrelerini destekler; `SupportsEvent` türlerinde format, konum, yaş aralığı eklenir. Hangi filtrelerin geçerli olduğu ContentType bayraklarından çıkar. Sayfalama SharedKernel `PagedRequest/PagedResult<T>` ile yapılır.

**Genel site araması:**

- `SiteSettings` içindeki `GlobalSearchEnabled` bayrağı ile kurulumdan kuruluma açılıp kapanır; kapalıyken endpoint `feature_unavailable` ProblemDetails döner.
- Website bir **`SearchDocument`** read-model'i tutar: `SourceKey` (`website`, `employer.job`…), `SourceId`, `LanguageCode`, `TypeKey`, `Title`, `Summary`, `Url`, `NormalizedText`, `PublishedAtUtc`.
- Website'in kendi içerikleri, ADR-020'deki gibi modül içi senkron dispatch ile aynı işlem akışında indekslenir; yalnızca `IsSearchable` türler girer.
- Dış kaynaklar için **pull modeli** kullanılır. Website `IExternalSearchSource` port'unu tanımlar (`SourceKey`, sayfalı `GetPublishedDocumentsAsync`). Bu projede Host katmanı, Employer'ın public contract'ından yayındaki ilan özetlerini okuyan bir adaptör kaydeder. Website'e ait bir Hangfire recurring job (varsayılan 10 dakikada bir) kayıtlı kaynakları çeker ve o kaynağa ait `SearchDocument` kümesini tam olarak senkronlar (yeni ekler, değişeni günceller, artık dönmeyeni siler). Admin'e ayrıca **kaynak bazlı anında yeniden indeksleme** komutu sunulur. Başka projede kaynak kaydedilmezse job boş çalışır.
- Gerekçe: Employer CAP kısıtı nedeniyle güvenilir event yayınlayamaz; ilan yayın komutlarına Host'tan müdahale etmek (push) ise Employer'ın iç akışını Host'a sızdırır. Pull modeli iki modülü de birbirinden habersiz bırakır; bedeli, ilanların aramaya dakikalar düzeyinde gecikmeyle yansımasıdır ve kabul edilmiştir.
- Arama, `NormalizedText` üzerinde indeksli `LIKE` ile başlar (ADR-020 normalize fonksiyonu yeniden kullanılır). SQL Server full-text'e yalnızca ölçümle ihtiyaç kanıtlanırsa geçilir (PERFORMANCE.md).
- İndekse yalnızca public içerik girer; kişisel veri, firma iç kaydı veya danışman notu tasarım gereği aramada yer alamaz.

### 11. Etkinlikler

#### 11.1. Etkinlik bilgisi

Etkinliğin *içeriği* ayrı bir aggregate değildir; `SupportsEvent` açık türlerdeki (Etkinlik, Eğitim/Atölye) bir ContentItem'dır. Böylece çeviri, SEO, galeri, menü ve arama ortak altyapıdan gelir. Etkinliğin *takvim ve kontenjan* bilgisi ise ContentItem ile 1:1 bağlı, ayrı bir **`EventSchedule`** aggregate'idir.

`EventSchedule`'ın owned entity değil ayrı aggregate olmasının nedeni concurrency'dir: kayıt işlemleri kontenjan sayaçlarını sık günceller. Sayaçlar ContentItem ile aynı satırda/rowversion'da olsaydı, her kayıt editörün açık düzenlemesini çakışmaya düşürürdü (ve tersi). İki aggregate aynı modülün DbContext'inde olduğu için, etkinlik oluşturma/düzenleme komutu ikisini tek Unit of Work'te kaydeder. DOMAIN.md'deki `Website.Training`, Eğitim türündeki ContentItem'dır; CareerDevelopment yalnızca `ContentItemId` referansı tutar.

`EventSchedule`: başlangıç/bitiş, format (yüz yüze/online/hibrit), yer ve/veya online link, kontenjan, başvuru açılış/kapanış zamanı, yaş aralığı, ücret bilgisi (metin), eğitmenler, program akışı, erişilebilirlik notu, `IsCancelled` ve iptal açıklaması, `RegistrationEnabled`, `WaitlistEnabled`. Online link yalnızca onaylı katılımcılara e-postayla gönderilir, public yanıtta yer almaz.

#### 11.2. Kayıt

`EventRegistration` aggregate'i.

- **Herkes kayıt olabilir.** Giriş yapmış kullanıcıda formu frontend doldurur; backend `UserId`'yi **yalnızca token'dan** alır, istemcinin gönderdiği kimliğe güvenmez (AGENTS §26). Her durumda ad, e-posta, telefon kayıt anındaki haliyle saklanır.
- **Anonim kayıtta e-posta doğrulaması zorunludur.** Doğrulanmamış kayıt kontenjandan yer tutmaz ve 24 saat sonra Hangfire job'ı ile silinir. Giriş yapmış ve e-postası doğrulanmış kullanıcı için bu adım atlanır.
- Doğrulama linkiyle birlikte **iptal linki** de gönderilir (imzalı token).
- Aynı etkinliğe aynı e-postayla ikinci aktif kayıt engellenir.
- Onaylanan aydınlatma metni sürümü kaydedilir (§12).

Durumlar:

```text
PendingVerification ──► Applied ──► Confirmed ──► Attended / NoShow
                           │            │
                           ├──► Waitlisted ──► Confirmed
                           └──► Rejected
Her aktif durumdan ──► Cancelled
```

- **Kontenjan eşzamanlılığı:** `EventSchedule` aggregate'inde onaylı/yedek sayaçları ve **optimistic concurrency (rowversion)** kullanılır; kayıt komutu `EventRegistration`'ı oluşturup `EventSchedule` sayacını aynı Unit of Work'te günceller. Çakışmada işlem sınırlı sayıda yeniden denenir. Kontenjan doluysa ve `WaitlistEnabled` açıksa kayıt `Waitlisted` olur.
- Bir yer açıldığında yedekten otomatik terfi yapılmaz; admin onaylar (ileride otomatikleştirilebilir).
- Etkinlik iptal edildiğinde aktif kayıtlara bildirim gider.
- Tüm e-postalar commit sonrası `IWebsiteEmailSender` ile best-effort gönderilir (CAP kısıtı). Doğrulama e-postası için **yeniden gönder** endpoint'i vardır.
- Takvim dosyası (`.ics`) ve katılımcı listesi dışa aktarımı sunulur. Katılımcı listesi hiçbir zaman public olmaz.
- QR ile giriş, yoklama ekranı, sertifika ve katılım puanı bu ADR kapsamında **yoktur.**

### 12. Formlar, yasal metinler ve bot koruması

#### 12.1. Yasal metinler

- `LegalDocument` (anahtar: `kvkk-contact`, `kvkk-event`, `cookie-policy`, `terms`…) ve sürümleri.
- Yayınlanan sürüm **değiştirilemez**; değişiklik yeni sürümdür. Sürüm numarası ve yürürlük tarihi zorunludur.
- Formlar, etkinlik kaydı ve bülten, yayındaki sürüme referans verir; başvuruda onaylanan sürüm saklanır.
- Aydınlatma ile açık rıza aynı onay kutusunda birleştirilmez.

#### 12.2. Form motoru

- **`FormDefinition`**: anahtar, dile göre başlık/açıklama/başarı mesajı, tipli alan listesi (`Text`, `Email`, `Phone`, `Textarea`, `Select`, `MultiSelect`, `Checkbox`, `Date`, `File`) ve her alanın zorunluluk/uzunluk/seçenek kuralları, bağlı `LegalDocument`, dosya yükleme kuralları, bildirim alıcıları, saklama süresi, `IsActive`.
- **`FormSubmission`**: referans numarası (`GM-2026-000123`), yanıtlar (tanıma göre backend'de doğrulanmış), onaylanan yasal metin sürümü, durum, atanan kişi, iç notlar, kaynak içerik (`ContentItemId`, varsa).
- Durumlar: `New → InReview → AwaitingInfo → Approved / Rejected → Completed → Archived`.
- Başvurular kişisel veri içerdiği için görüntüleme `Website.Submissions.View` policy'sine bağlıdır ve **auditlenir**.
- Saklama süresi dolan başvurular Hangfire recurring job ile anonimleştirilir/silinir.
- Bu projedeki ilk formlar: İletişim, Gönüllülük, Dernek Üyeliği, Mentor/Eğitmen, Kurumsal İş Birliği. Girişimcilik gibi ileride kendi domain'i olabilecek başvurular da ilk aşamada form motoruyla karşılanır.
- **İçerik–form bağlantısı:** `SupportsForm` açık türlerde bir içeriğe form bağlanabilir (ör. her gönüllülük fırsatının kendi başvuru formu).

#### 12.3. Bot koruması

- Anonim POST endpoint'leri (form, etkinlik kaydı, bülten) `IBotProtectionVerifier` ile korunur.
- Bu projede sağlayıcı **Cloudflare Turnstile**'dır (`Managed` mod; Pre-Clearance çerezi kullanılmaz). Sağlayıcı ve açık/kapalı durumu `SiteSettings`'ten yönetilir.
- Her durumda honeypot alanı, minimum doldurma süresi kontrolü ve IP rate limiting uygulanır.
- Turnstile'ın işlediği sinyaller (IP, TLS parmak izi, User-Agent) ve yurt dışına aktarım, ilgili aydınlatma metinlerinde belirtilir. Yurt dışı aktarım için gereken sözleşmesel yükümlülükler hukuk danışmanıyla ayrıca netleştirilir.

### 13. Site ayarları, çerez ve script yönetimi

`SiteSettings` (tek kayıt):

- Dile göre site adı, varsayılan SEO alanları, footer metni
- Tema (§8.3)
- İletişim: adres, telefon, e-posta, WhatsApp, harita koordinatı/embed, sosyal medya linkleri
- **Banka hesapları** (IBAN listesi) — "Destek Ol" sayfası için. Bu projede ilgili sayfa ve menü öğesi **pasif** başlar. Online ödeme bu ADR kapsamında yoktur.
- Özellik bayrakları (yalnızca public siteyi ilgilendirenler): `GlobalSearchEnabled`, `NewsletterEnabled`, `PublicJobListingsEnabled`, `DonationPageEnabled` (bu projede `false` başlar), `BotProtectionEnabled` (bu projede `true`)
- Bakım modu (admin'ler için bypass)

Başka modüllerin backend davranışını değiştiren bayraklar (ör. istihdam aracılığı) Website'e ait değildir.

**Script yönetimi:** Üçüncü taraf script'ler (analitik vb.) kategoriyle (`Necessary`, `Analytics`, `Marketing`) tanımlanır. Frontend, ziyaretçi ilgili kategoriye onay vermeden script'i yüklemez. Onay tercihi tarayıcıda tutulur.

### 14. Bülten

- `NewsletterSubscriber`: e-posta, dil, onaylanan yasal metin sürümü, durum (`PendingConfirmation → Active → Unsubscribed`).
- **Çift onay:** Abonelik, e-postadaki linke tıklanınca aktifleşir.
- Her e-postada imzalı abonelikten çıkma linki bulunur.
- Bu ADR kapsamında bülten *gönderim* aracı (kampanya editörü) yoktur; abone yönetimi ve dışa aktarım vardır. Derneğin bülteninin ticari elektronik ileti / İYS kapsamına girip girmediği hukuken kontrol edilir.

### 15. SEO altyapısı ve route çözümleme

- **`Redirect`**: kaynak yol → hedef yol, 301/302, otomatik veya elle oluşturulmuş.
- **404 kaydı:** Bulunamayan yollar sayılarak loglanır (yol + sayı + son görülme; IP tutulmaz). Admin panelinde en çok 404 alan yollar listelenir, tek adımda yönlendirme oluşturulur.
- **Route çözümleme:** `GET /api/v1/public/routes/resolve?path=...&lang=...` → `Listing` / `Detail` / `Home` / `Redirect` / `NotFound` ve gerekli kimlikler.
- Sayfa başına tek istekte gerekli veriyi dönen public endpoint'ler (içerik + breadcrumb + SEO + hreflang + ilişkili içerik), ileride verilecek SSR/ön-render kararından bağımsız olarak kullanılabilir.
- **Breadcrumb** hiyerarşiden otomatik üretilir.
- **Sitemap ve robots:** backend tarafından dinamik üretilir; yalnızca yayındaki, `NoIndex` olmayan içerikler sitemap'e girer.
- **Yapılandırılmış veri:** Backend, schema.org için gerekli veriyi yanıtta döner (Organization/NGO, Event, NewsArticle, FAQPage, BreadcrumbList); JSON-LD'yi frontend basar.

### 16. Güvenlik

- **Zengin metin:** Frontend editörü **TipTap (MIT)**. Backend, gelen HTML'i **HtmlSanitizer (Ganss.Xss, MIT)** ile beyaz liste bazlı temizler; frontend'e güvenilmez. İzin verilen etiket/öznitelik listesi Faz 0'da belirlenir. `iframe` yalnızca YouTube-nocookie alan adına izinlidir.
- **Lisans ilkesi:** Bu modülde yalnızca ücretsiz ve ticari kullanımda kısıt getirmeyen lisanslar (MIT, Apache-2.0, BSD) kullanılır. GPL/ticari çift lisanslı editörler (CKEditor 5, TinyMCE 7) ve gelire bağlı lisanslı kütüphaneler bu nedenle reddedilmiştir.
- Dosya yüklemelerinde MIME ve uzantı doğrulaması, boyut limiti (ADR-019 doğrulama politikası).
- Tüm yönetim işlemleri (yayınlama, silme, yasal metin yayını, başvuru görüntüleme) ADR-015 audit kapsamına alınır.

### 17. Performans ve cache

- Public okumalar SharedKernel cache'i (ADR-017) ile cache'lenir. Yayınlama, güncelleme, kaldırma ve ayar değişikliklerinde ilgili anahtarlar invalid edilir.
- Zamanlanmış yayın nedeniyle cache TTL'i, ilgili listedeki bir sonraki `PublishAtUtc` / `UnpublishAtUtc` anını aşmaz.
- Liste sorguları projection + `AsNoTracking` kullanır; çeviri tablosu dil filtresiyle join edilir, tüm çeviriler yüklenmez.

### 18. Faz planı

| Faz | Kapsam |
|---|---|
| **0 — Temel** | WebsiteDbContext, SiteLanguage, çeviri deseni, `Slug`/`Seo` value object'leri, HtmlSanitizer, isimli policy'ler, MediaAsset + SkiaSharp boyutları + public medya servisi, SiteSettings (tema, iletişim, IBAN, bayraklar, bakım modu), `IBotProtectionVerifier` + Turnstile adaptörü, `IWebsiteEmailSender` + Notification adaptörü |
| **1 — İçerik çekirdeği** | ContentType, ContentItem (çeviri, hiyerarşi, kategori, etiket, görseller, galeri, ekler, durum/zamanlama, sıra, öne çıkan), Video, ilişkili içerik, Redirect + 404 kaydı, route çözümleme, breadcrumb, önizleme, çöp kutusu, kopyalama, public endpoint'ler, cache |
| **2 — Sunum** | Menü, PageLayout + bloklar, Slider, Partner, ImpactMetric, Pop-up/Banner |
| **3 — Etkileşim** | LegalDocument, form motoru + içerik–form bağlantısı, bülten, script yönetimi |
| **4 — Etkinlik** | EventSchedule, EventRegistration (doğrulama, kontenjan, yedek, iptal), `.ics`, katılımcı dışa aktarımı |
| **5 — Keşif** | SearchDocument + genel arama + `IExternalSearchSource` + Employer Host adaptörü + senkron job'ı, sitemap/robots, schema.org verisi, revizyon geçmişi |

Her faz, ayrı görevlere bölünmüş kendi master prompt'uyla uygulanır; her görev ayrı commit'tir.

**Paralel iş (Employer modülü, bu ADR'nin kapsamı dışında):** İlan listesine sayfalama ve filtre, ilan slug'ı ve public detay endpoint'i, public firma profili ve firmanın logosunun sitede gösterilmesine izin veren onay alanı, arama adaptörü için yayındaki ilan özetlerini sayfalı dönen public contract metodu.

## Sonuçlar

**Olumlu**

- Yeni içerik türü, menü, sayfa düzeni veya form eklemek kod değişikliği gerektirmez.
- Modül iş modüllerinden bağımsız olduğu için başka projelere taşınabilir; projeye özgü bağlantılar Host adaptörlerinde kalır.
- Çeviri yapısı baştan kurulduğu için ikinci dil eklemek veri girişi meselesidir.
- Kişisel veri içeren tüm yüzeyler (başvurular, kayıtlar, aboneler) tek policy ve audit altında toplanır.

**Olumsuz / Riskler**

- Genel `ContentItem` modeli, türe özgü iş kuralı gerektiren içerikler için yetersiz kalabilir. Bu durumda ilgili ihtiyaç ayrı aggregate veya ayrı modül olarak ele alınmalıdır.
- Blok ayarlarının JSON tutulması, her blok tipi için backend validator'ının frontend bileşeniyle senkron tutulmasını gerektirir.
- CAP kısıtı nedeniyle e-postalar ve dış kaynak indekslemesi best-effort'tur; yeniden gönderme ve yeniden indeksleme mekanizmalarıyla telafi edilir.
- Turnstile kullanımı yurt dışı aktarım yükümlülüğü doğurur; hukuki süreç tamamlanmadan canlıya çıkılmamalıdır.
- Public frontend'in SPA mı SSR/ön-render mı olacağı kararı verilmeden SEO alanlarının etkisi sınırlı kalır.

## Reddedilen Alternatifler

- **Her içerik türü için ayrı aggregate:** Büyük tekrar, yeni tür için kod zorunluluğu; taşınabilirlik hedefiyle çelişir.
- **Türe özgü özel alanlar için EAV:** Sorgu karmaşıklığı ve performans maliyeti; ihtiyaç kanıtlanmadı.
- **Multi-tenant yapı:** Tüm modele tenant boyutu ekler; tek kurulum gereksinimi karşılıyor.
- **ReferenceData `Language` tablosunu site dili olarak kullanmak:** Farklı kavram; Website'i ReferenceData'ya bağımlı kılar.
- **Ayrı aktif/pasif alanı + yayın durumu:** Anlamsız durum kombinasyonları doğurur.
- **Düz slug yapısı:** Daha basit, ancak hiyerarşi URL'de görünmez; iç içe yol tercih edildi.
- **Etkinlik içeriğini ayrı aggregate olarak modellemek:** Çeviri, SEO, galeri, menü ve arama altyapısının tekrarlanmasını gerektirir. (Yalnızca takvim/kontenjan kısmı concurrency nedeniyle ayrı aggregate'tir, bkz. §11.1.)
- **`EventSchedule`'ı ContentItem'ın owned entity'si yapmak:** Kayıt sayaçları ile içerik düzenleme aynı rowversion'ı paylaşır, gereksiz çakışma üretir.
- **Dış arama kaynaklarının push ile indekslenmesi:** Employer güvenilir event yayınlayamaz; Host'un Employer komutlarına müdahalesi iç akışı dışarı sızdırır. Pull modeli tercih edildi (bkz. §10).
- **Her public form için ayrı kod:** Tekrar; form motoru tercih edildi.
- **Website'in Employer'ı doğrudan çağırması veya ilan verisini kopyalaması:** Modül bağımsızlığını ve veri sahipliğini (AGENTS §9) ihlal eder.
- **Serbest sürükle-bırak sayfa oluşturucu:** Güvenlik ve bakım maliyeti yüksek; önceden tanımlı blok modeli yeterli.
- **CKEditor 5 / TinyMCE 7:** GPL veya ücretli ticari lisans; lisans ilkesiyle çelişir.
- **ImageSharp:** Gelire bağlı lisans koşulları; SkiaSharp (MIT) tercih edildi.
- **SQL Server full-text ile başlamak:** İhtiyaç ölçülmeden karmaşıklık; indeksli normalize arama ile başlanır.
