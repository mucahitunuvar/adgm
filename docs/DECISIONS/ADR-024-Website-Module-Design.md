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

| Port | Amaç | Bu projedeki implementasyon |
|---|---|---|
| `IWebsiteEmailSender` | Doğrulama, bildirim, bülten e-postaları | Host adaptörü → Notification modülünün public contract'ı (başka bir iş modülüne dokunuyor, ARCHITECTURE.md §50.1) |
| `IBotProtectionVerifier` | Anonim formlarda bot doğrulaması | Website'in kendi Infrastructure'ı → Cloudflare Turnstile (bkz. §12) — yalnızca harici bir servise dokunduğu için Host adaptörüne gerek yok (ARCHITECTURE.md §50.1) |
| `IExternalSearchSource` | Dış kaynakların (ör. ilanlar) genel arama indeksine periyodik olarak çekilmesi | Host adaptörü → Employer public contract'ından yayındaki ilanları okuyan adaptör (başka bir iş modülüne dokunuyor, bkz. §10) |

### 2. Yetkilendirme

- Website endpoint'leri **isimli policy** kullanır. Policy'ler şimdilik Admin rolüne çözülür (literal rol string'i; ADR-016 pattern'i, `Identity.Domain.UserRole`'a bağımlılık yok).
- İleride gerçek permission sistemi geldiğinde yalnızca policy tanımları değişir; endpoint'lere dokunulmaz.

| Policy | Kapsam |
|---|---|
| `Website.Content.Manage` | İçerik oluşturma/düzenleme/silme, medya, video, revizyon geçmişi listeleme/karşılaştırma/geri yükleme (Faz 5, §4.6) |
| `Website.Content.Publish` | Yayınlama, yayından kaldırma, arşivleme |
| `Website.Structure.Manage` | ContentType tanımları, site dilleri (sistem yöneticisi işi) |
| `Website.Design.Manage` | Menü, sayfa düzeni, slider, pop-up, partner, tema |
| `Website.Settings.Manage` | SiteSettings, script/çerez yönetimi, yönlendirmeler, arama kaynakları listeleme/yeniden indeksleme (Faz 5, §10) |
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

`SortMode: EventDateAsc` yalnızca `SupportsEvent` açık bir türde seçilebilir. Faz 4'ten itibaren gerçek bir sıralamadır: liste sorgusu bağlı `EventSchedule.StartsAtUtc`'ye göre artan sıralar (içerik ile takvim arasında sol join; takvimi olmayan içerik listenin sonuna düşer, bkz. §11.2).

Boş `RoutePrefix`'li bir tür (`page` gibi kök seviye türler) `HasListingPage` olamaz - liste sayfasının bir URL'si olmaz. `RoutePrefix` (ve kök seviye bir `ContentItem`'ın ilk yol segmenti) statik altyapı yollarıyla ve hiçbir `SiteLanguage` koduyla (aktif/pasif fark etmez) çakışamaz; ayrılmış segmentler: `api`, `admin`, `portal`, `webuploads`, `uploads`, `media`, `assets`, `static`, `sitemap.xml`, `robots.txt`.

**Kategoriler ve etiketler** (Faz 1b Görev 3, uygulandı): `ContentCategory` tür başına tanımlanır (`ContentTypeId` zorunlu), en fazla 2 seviye derinliğinde (torun kategori reddedilir) ve çevrilebilirdir (her dilde ayrı ad + slug; slug tür+dil içinde benzersiz). `ContentTag` ise türden bağımsız, **her dile ait ayrı bir kümedir** - aynı kavramın farklı dillerdeki etiketleri birbirinden ayrı satırlardır, ortak bir kimlikle eşleştirilmez (ad, dil içinde büyük/küçük harf duyarsız benzersizdir). Admin panelinde etiketler yeniden adlandırılabilir veya birbirine **birleştirilebilir** (`MergeTagCommand`: hedefe taşınan her `ContentItemTranslation.TagIds` güncellenir, kaynak silinir). Bir `ContentItem`, kategori kimliklerini (`CategoryIds`) ve her çevirisi kendi etiket kimliklerini (`TagIds`) EF Core primitive collection (JSON dizi sütunu) olarak tutar - ayrı bir köprü tablosu yoktur.

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

- **İç içe yol** kullanılır: `/kurumsal/hakkimizda`. Tam yol (`FullPath`) çeviri başına hesaplanıp saklanır; sorgu anında hesaplanmaz. `FullPath`, bir dil içinde **tüm modül genelinde** benzersizdir - yalnızca aynı tür içinde değil, farklı türlere ait içerikler arasında da çakışamaz (tek bir global URL alanı).
- Kurallar (domain invariant): parent aynı türden olmalı, döngü yasak, maksimum derinlik 3.
- Slug `Slug` value object'i ile üretilir: Türkçe karakter dönüşümü (ç→c, ğ→g, ı→i, ö→o, ş→s, ü→u), küçük harf, tire ayracı. (tür + dil + parent) içinde benzersizdir.
- Slug veya parent değiştiğinde etkilenen tüm alt içeriklerin `FullPath`'i güncellenir ve **eski her yol için otomatik 301 `Redirect` kaydı** oluşturulur. Otomatik bir `Redirect`, statik bir hedef yol değil, **hedef `ContentItem`'ın kendisini** tutar - çözümleme her seferinde o içeriğin güncel `FullPath`'ini canlı okur. Bu sayede bir içerik birden çok kez taşınsa bile zincir oluşmaz: her taşınma için ayrı, birbirinden bağımsız geçerli bir kayıt eklenir, var olan hiçbir kayıt güncellenmez veya takip edilmez.

#### 4.4. Durum modeli

Ayrı bir aktif/pasif alanı **yoktur**; tek durum alanı kullanılır:

```text
Draft ──► Published ◄──► Unpublished (pasif)
                              │
                              ▼
                          Archived
```

- `Draft`, yalnızca `Published`'a geçebilir (ilk yayınlama) - `Unpublished`'a veya `Archived`'a doğrudan bir geçiş yoktur.
- `Archived`, yalnızca `Unpublished`'a geri döner (`Unarchive`); doğrudan `Published`'a dönemez - bir editörün tekrar gözden geçirmesi gerekir. (`Unpublished`'dan tekrar `Published`'a geçmek ayrı, normal bir "republish" adımıdır.)
- Yayında görünme koşulu: `Status == Published && (PublishAtUtc == null || PublishAtUtc <= now) && (UnpublishAtUtc == null || UnpublishAtUtc > now) && DeletedAtUtc == null`. Bir içerik ancak **kendisi ve tüm ataları** bu koşulu sağlıyorsa görünürdür - zamanlanmış veya yayından kaldırılmış bir ebeveynin altındaki, kendi başına yayında olan bir çocuk da görünmez sayılır.
- Zamanlanmış yayın için arka plan job'ı gerekmez; koşul sorguda uygulanır.
- `RequiresReview` açık bir türde `Draft → InReview → Published` akışı ve yazan ≠ yayınlayan kuralı devreye girer. Bu projede kapalı.

#### 4.5. Diğer içerik davranışları

- **Önizleme linki:** Yayınlanmamış içerik için süreli, imzalı token'lı public önizleme URL'si üretilebilir.
- **Çöp kutusu:** Silme soft delete'tir; 30 gün içinde geri alınabilir, sonra kalıcı silinir (Hangfire recurring job).
- **Kopyalama:** Bir içerik, çevirileri ve medya referanslarıyla birlikte yeni bir taslak olarak kopyalanabilir.
- **İlişkili içerik:** Elle bağlanan içerikler; yoksa aynı kategoriden son yayınlananlar döner.

#### 4.6. İçerik revizyon geçmişi (Faz 5, uygulandı)

- Kapsam yalnızca `ContentItem`'dır (çeviri metinleri, SEO, etiket/kategori). `PageLayout`, `SiteSettings` ve `LegalDocument` kapsam dışıdır (`LegalDocument` zaten kendi sürümlemesine sahiptir; `PageLayout`'un taslak/yayın modeli aynı amaca hizmet eder).
- **`ContentItemRevision`** (ayrı entity, aggregate değil — kayıt tablosu): `ContentItemId`, `RevisionNumber` (içerik içinde artan, hiçbir zaman yeniden kullanılmaz), `SavedAtUtc`, `SavedByUserId`, `Kind` (`Created`/`Edited`/`Published`/`Restored`), `ChangedLanguages`, `IsPublishedSnapshot` (yalnızca `Kind = Published` ile birlikte `true` olabilir), `ContentHash` (SHA-256, maks 64), `SnapshotJson` (tüm diller için: `Title`, `Summary`, `Body` — sanitize edilmiş, SEO alanları, `TagIds`; içerik düzeyinde `CategoryIds`). Benzersiz indeks: `(ContentItemId, RevisionNumber)`.
- **Yakalama:** `IContentRevisionRecorder` portu, içeriği değiştiren handler'lardan (oluşturma, çeviri güncelleme/silme, kopyalama, yayınlama, kategori/etiket değişimi, geri yükleme) **aynı Unit of Work içinde** çağrılır. `Kind = Published` **her zaman** bir revizyon üretir; diğer türlerde önceki revizyonla `ContentHash` aynıysa kayıt oluşturulmaz (gürültü önleme).
- **Yönetim uçları** (`Website.Content.Manage`): `GET /api/v1/admin/website/contents/{id}/revisions` (sayfalı özet liste, anlık görüntü yok), `GET .../revisions/{revisionNumber}` (tüm diller için anlık görüntü), `GET .../revisions/compare?from=&to=&lang=` (`to` bir revizyon numarası veya `current` olabilir; alan bazlı fark, metin diff algoritması yok), `POST .../revisions/{revisionNumber}/restore` (`RowVersion`, dil, opsiyonel kategori geri yükleme).
- **Geri yükleme:** yalnızca seçilen dilin anlık görüntü alanları geri yazılır; `Body` **yeniden sanitize edilir**; slug/durum/yol **değişmez**; sonuç yeni bir `Restored` revizyondur ve arama indeksi + `InvalidateAllPublic` aynı akışta çalışır. Anlık görüntüde o dil yoksa `404` (`ContentItemRevision.TranslationNotFound`); dil artık pasif/çevirisi silinmişse `409` (`Revision.TranslationNoLongerExists`); `RowVersion` çakışmasında `409` (`ContentItem.ConcurrencyConflict`).
- **Saklama:** Günlük `PruneContentItemRevisionsJob` (Hangfire recurring, tek seferde en çok 500 içerik): içerik başına en yeni **50** revizyon kalır; `IsPublishedSnapshot` olan en yeni revizyon bu pencerenin dışında kalsa bile **her zaman** korunur.
- **Silme:** İçerik kalıcı silindiğinde revizyonları da aynı işlemde silinir; çöp kutusuna taşıma (soft delete) revizyonlara dokunmaz.

### 5. Video kütüphanesi

- `Video` aggregate'i: dile göre ad, `YouTubeVideoId`, opsiyonel `CoverImageId`, `SortOrder`, `IsActive`.
- Yalnızca YouTube kabul edilir; backend girilen linkten video ID'sini çıkarır ve doğrular. Kapak verilmezse YouTube küçük resmi kullanılır.
- Oynatma `youtube-nocookie.com` üzerinden yapılır (frontend sorumluluğu, KVKK gereği).
- İçerikler videoları bu kütüphaneden **sıralı liste** olarak seçer. Videolar sayfası kütüphanenin public listesidir.

### 6. Medya kütüphanesi

- `MediaAsset` aggregate'i ADR-019'daki `IFileStorageService` üzerine kurulur; ADR-019'daki kapalı `FileCategory` listesine Website kategorileri eklenir: `website-images`, `website-documents` (public) ve `website-form-attachments` (özel).
- Alanlar: dosya, klasör, dile göre alt metin ve açıklama, kaynak/telif, kullanım izni, `ContainsPersonalData`, genişlik/yükseklik, boyut, MIME, yükleyen.
- **Görsel işleme:** Yükleme anında **SkiaSharp (MIT)** ile üç sabit genişlik üretilir - `small` 400px, `medium` 800px, `large` 1600px - ve WebP'ye (kalite 82) dönüştürülür; orijinal, yön düzeltilmiş ve metadata'sı temizlenmiş halde kendi formatında ayrıca saklanır. En-boy oranı her zaman korunur; orijinal bir varyant genişliğinden küçük veya eşitse o varyant üretilmez (büyütme yapılmaz).
- **Kullanım takibi:** Bir içerikte, blokta, slider'da vb. kullanılan medya silinemez; kullanıldığı yerler listelenir.
- Public medya uzun süreli cache header'larıyla doğrudan servis edilir. Form ekleri gibi özel dosyalar yalnızca yetkili endpoint'ten stream edilir.

### 7. Menüler (Faz 2 Görev 1'de netleşti/uygulandı)

- `Menu` aggregate'i bir konuma bağlıdır. Konumlar: `Header`, `Utility`, `Footer` - üçü de migration ile boş seed edilir, admin tarafından oluşturulmaz/silinmez, yalnızca içeriği (`MenuItem` ağacı, `ReplaceItems` ile tüm ağaç birden) değiştirilir. Bu ADR'nin ilk taslağındaki `Mobile` konumu Faz 2 master prompt'unun kullanıcı kararıyla (§1) düşürülmüştür: ayrı bir mobil menü tutulmaz, frontend mobil menüyü `Header`'dan üretir.
- İçinde iç içe `MenuItem` ağacı bulunur (`ParentId`, aynı menü içindeki başka bir `MenuItem`'a düz skaler referans - EF navigation yok, ağaç bellekte dolaşılarak kurulur/doğrulanır). Öğenin ayrı bir "Group" türü yoktur: `LinkTarget`'ı boş (`LinkTarget.CreateEmpty()`) olan bir öğe kendiliğinden linksiz bir grup başlığıdır.
- **`LinkTarget`**: `Menu`, `Slider` (§8.2) ve `Popup`'ın (§9) paylaştığı ortak bir Domain value object'i. Dört karşılıklı dışlayıcı şekil + boş durum:
  - `Content` → bir `ContentItem` kimliği
  - `ContentTypeListing` → bir `ContentType` kimliği
  - `InternalPath` → `/` ile başlayan, `//`/`\`/şema (`:`) içermeyen, en fazla 500 karakterlik sabit iç yol
  - `ExternalUrl` → mutlak `https`/`http`/`mailto:`/`tel:`, en fazla 1000 karakter
  - `None` → link yok (grup başlığı veya linki opsiyonel bırakılmış bir öğe/slide/pop-up)

  Hedefi public `href`'e çeviren tek bir Application servisi vardır (`LinkTargetResolver`); bir sayfadaki tüm hedefler **toplu** (`ResolveManyAsync`) çözülür - N+1 yok. `Content` hedefi görünmüyorsa (kendisi/bir atası görünmez, türü pasif, `HasDetailPage=false`, istenen dilde çevirisi yok) veya `ContentTypeListing` hedefi geçersizse (tür pasif, `HasListingPage=false`, dilde çevirisi yok) çözümleme `null` döner.
- Alanlar: dile göre etiket, ikon (`IconKey`, `[a-z0-9-]`, maks 50), yeni sekmede açma, `IsActive`, `SortOrder`.
- Kurallar: döngü yok, maksimum derinlik 3, menü başına en fazla 200 öğe, `Utility` menüsü tek seviyelidir (çocuk öğe reddedilir).
- **Güncelleme tüm ağacı birden değiştirir:** editör sürükle-bırak ağacı tek çağrıda gönderir; istemci tarafı geçici kimlikler Application katmanında gerçek kimliklere çözülür, `RowVersion` ile iyimser eşzamanlılık uygulanır.
- Bağlı içerik yayında değilse veya türü pasifse öğe public yanıtta otomatik gizlenir (link çözümlenemediği için); hiç görünür çocuğu kalmayan grup başlıkları da gizlenir.
- Menü öğesini pasifleştirmek yalnızca linki gizler; bir bölümü tamamen kapatmak için ContentType pasifleştirilir.
- **Kullanım koruması:** Bir `ContentItem` kalıcı silindiğinde (çöp kutusu job'ı dahil), ona `Content` tipiyle bağlı tüm menü öğelerinin linki temizlenir ve öğe pasife alınır (`Menu.DeactivateItemsLinkingToContent`) - aynı transaction'da, kalıcı silme işlemiyle birlikte; böylece öğe sessizce aktif-ama-boş bir grup başlığına dönüşmez.

### 8. Tasarım modülü

#### 8.1. Sayfa düzeni (blok tabanlı) (Faz 2 Görev 4/5'te netleşti/uygulandı)

- Serbest sürükle-bırak builder **değildir**; kodla gelen blok tiplerinin seçilip sıralandığı, açılıp kapandığı ve ayarlandığı bir sistemdir. Her blok tipinin frontend'de karşılık gelen bir bileşeni vardır.
- `PageLayout` bir hedefe bağlanır: `Home` (migration ile tek satır seed edilir, admin oluşturmaz/silmez) ya da `SupportsBlockLayout` açık, çöp kutusunda olmayan bir `ContentItem` (içerik başına tek düzen; ilk taslak kaydında oluşur).
- `PageLayout` **taslak ve yayın** olmak üzere iki ayrı blok listesi tutar (`DraftBlocks`/`PublishedBlocks`); değişiklikler taslakta yapılır, `.../preview` ile önizlenir, `Publish` ile taslak yayındaki listeye kopyalanır (referans doğrulaması bu anda tekrar yapılır), `DiscardDraft` ile taslak yayındaki haline döndürülür. Düzen başına en fazla 30 blok. `RowVersion`, veritabanı üretimli bir rowversion sütunu değil - modül SqlServer ve Sqlite'ın ikisinde de çalıştığı için (ADR-012) - her mutasyonda yeniden üretilen, uygulama tarafından yönetilen bir eşzamanlılık token'ıdır (modüldeki diğer tüm aggregate'lerle aynı desen).
- `LayoutBlock`: `BlockType`, `SortOrder`, `IsActive`, dile göre metinler (`TextsJson`), `Settings` (JSON, `SettingsJson`). Ayarlar ve metinler backend'deki blok tipi kaydında tanımlı validator ile doğrulanır; tanımsız blok tipi veya geçersiz ayar kabul edilmez. JSON deserialize edilirken bilinmeyen alanlar **reddedilir** (`System.Text.Json`, `UnmappedMemberHandling.Disallow`).
- **Blok tipi kaydının tek kaynağı:** `GET /api/v1/admin/website/block-types`, her blok tipinin `Settings`/`Texts` C# kayıtlarını `System.Reflection` ile gezen küçük, modül içi bir `BlockTypeFieldDescriber` kullanır - ad (camelCase), tip ve zorunluluk C# nullability'sinden, sınırlar (`MinLength`/`MaxLength`/`Range`) `System.ComponentModel.DataAnnotations` niteliklerinden çıkarılır. Bu nitelikler yalnızca bu uç nokta için açıklama metadata'sıdır; doğrulamanın kendisi hâlâ blok tipinin `ValidateSettings`/`ValidateTexts`'idir - aynı property üzerinde oldukları için şema, saklanan şekilden kopamaz. Yeni bir JSON-Schema kütüphanesi eklenmemiştir (AGENTS.md §47: küçük bir sorunu çözmek için paket eklenmez; repo'da başka bir "kendi şeklini reflection'la anlat" yardımcısı da yoktu).
- Kullanım koruması: taslak **ve** yayındaki blokların referans verdiği medya, video, slider ve göstergeler silinemez (`LayoutMediaUsageProvider`; `ISliderUsageChecker`'ın gerçek implementasyonu; `IVideoUsageChecker`'a düzen kaynağı eklendi). İçerik türü/kategori yalnızca pasife alınabildiği (silinemediği) için onlara referans blokta kalabilir - public tarafta bu, aşağıdaki gizleme kurallarıyla ele alınır.
- İçerik kalıcı silindiğinde düzeni de aynı transaction'da silinir.

Başlangıç blok kataloğu (uygulandı, 15 blok tipi; `(dil)` işaretli alanlar `Texts` kaydına, diğerleri `Settings`'e aittir; tüm link alanları §7'deki `LinkTarget`'tır):

| Key | Ayarlar | Metinler (dil) | Hedef |
|---|---|---|---|
| `hero-slider` | `sliderId` | — | Home, Content |
| `logo-strip` | `maxItems` (1-30) | `title` (ops.) | Home, Content |
| `quick-links` | `items[]` (1-8): `iconKey`, `link` | `items[]`: `label`, `description` (ops.) | Home, Content |
| `content-list` | `contentTypeKey`, `count` (1-12), `featuredOnly`, `categoryId` (ops.), `view` (`cards`/`list`) | `title`, `moreLabel` (ops.) | Home, Content |
| `upcoming-events` | `contentTypeKeys[]` (`SupportsEvent` türler), `count` (1-12) | `title`, `moreLabel` (ops.) | Home, Content |
| `job-list` | `count` (1-12) | `title`, `moreLabel` (ops.) | **Home yalnızca** |
| `feature-mosaic` | `items[]` (1-5): `imageMediaId` (ops.), `link` | `items[]`: `eyebrow` (ops.), `title` | Home, Content |
| `process-steps` | `stepCount` (2-8) | `title`, `steps[]`: `title`, `text` | Home, Content |
| `video-feature` | `videoId` | `eyebrow` (ops.), `title`, `text` (ops.), `link`+`linkLabel` (ops.) | Home, Content |
| `impact-stats` | `metricIds[]` (boşsa tüm aktifler, en fazla 8) | `title` (ops.) | Home, Content |
| `cta` | `buttons[]` (1-3): `link`, `style` (`primary`/`secondary`/`quiet`) | `eyebrow` (ops.), `title`, `buttons[]`: `label` | Home, Content |
| `rich-text` | — | `body` (sanitize) | Home, Content |
| `image-text` | `imageMediaId`, `imagePosition` (`left`/`right`), `link` (ops.) | `title`, `body` (sanitize), `linkLabel` (ops.) | Home, Content |
| `faq` | `contentTypeKey`, `categoryId` (ops.), `count` (1-30) | `title` (ops.) | Home, Content |
| `gallery` | `mediaIds[]` (1-30) | `title` (ops.) | Home, Content |

`job-list` bloğu yalnızca ayar tutar; veriyi frontend doğrudan Employer API'sinden çeker. Website, ilan verisini kopyalamaz.

**Public blok verisi:** `GET /api/v1/public/home` (ana sayfanın yayındaki blokları + `ContentSeoResolver` ile site varsayılan SEO'su) ve içerik detayına (`GET /api/v1/public/contents/{id}`) eklenen `blocks` alanı (`SupportsBlockLayout` türde ve yayındaki düzen varsa). Faz 1b'deki önizleme uçları (içerik önizleme ve yeni `GET .../layouts/home/preview`) **taslak** blokları döner ve cache'lenmez. Bir düzenin tüm blokları için referanslar toplu çözülür (tüm medya/link/video kimlikleri tek sorguda); liste tipi bloklar (`content-list`, `upcoming-events`, `faq`) kendi sorgularını çalıştırır - bu kabul edilebilir bulunmuştur. Gizleme kuralları (hata değil, bloğun yanıttan çıkarılması): blok pasifse; istenen dilde metni yoksa; zorunlu referans artık yoksa/kullanılamıyorsa (slider'ın görünür slide'ı kalmadı, tür/video pasif vb.); liste tipi blokların `data`'sı boş kalıyorsa; çözümlenemeyen linkler (tekil linklerde ilgili öğe/buton, dizi tipi bloklarda ilgili dizi öğesi gizlenir - öğe kalmazsa blok gizlenir).

#### 8.2. Bloklara veri sağlayan aggregate'ler (Faz 2 Görev 2/3'te netleşti/uygulandı)

- **`Slider`**: değiştirilemez bir `Key` (`[a-z0-9-]`, maks 50, benzersiz; ör. `home-hero`) ile tanımlanır, en fazla 20 sıralı `Slide` tutar. `Slide`: masaüstü görsel (zorunlu), mobil görsel (opsiyonel), `LinkTarget` (§7; buton etiketi varsa zorunlu), `SortOrder`, `IsActive`, `PublishAtUtc`/`UnpublishAtUtc` (görünürlük tek bir expression - `SlideVisibility`), dile göre öncül etiket/başlık/metin/buton etiketi/alt metin override'ı. Alt metin zinciri: `AltTextOverride` → masaüstü görselin medya kütüphanesindeki o dildeki alt metni → boş (Faz 1b galeri kuralıyla aynı). Ad çevirileri ve slide listesi ayrı uçlardan (`PUT .../sliders/{id}`, `PUT .../sliders/{id}/slides`) tüm liste olarak `RowVersion` ile değiştirilir. `SliderMediaUsageProvider` ile kullanımdaki görseller silinemez; `ISliderUsageChecker`'ın gerçek implementasyonu Görev 4'te (PageLayout kullanım koruması) eklenir.
- **`Partner`**: logo (zorunlu), dile göre ad ve opsiyonel açıklama, opsiyonel mutlak `https` link, `SortOrder`, `IsActive`. `GET /api/v1/public/partners?lang=` iş birlikleri sayfası için; logo şeridi bloğu (§8.1) aynı sorgu servisini kullanır. `PartnerMediaUsageProvider` ile kullanımdaki logo silinemez.
- **`ImpactMetric`**: dile göre etiket, birim; değer (decimal, ≥0, en fazla 2 ondalık); **dönem** ve **kaynak** zorunlu. Doğrulanmamış rakam yayınlanmaz - bir gösterge, varsayılan dilde `Period`/`Source` dolu olmadan aktif edilemez; aktifken bu alanlar boşaltılamaz. İleride diğer modüllerden (ör. Employment'tan işe yerleşme sayısı) Host adaptörüyle beslenebilir; bu ADR kapsamında manuel girilir.

#### 8.3. Tema ayarları

`SiteSettings.Theme`: logo (açık/koyu), favicon, ana ve ikincil renk, font ailesi. Frontend bunları CSS değişkenlerine basar. Amaç, modülün başka projelerde kod değişikliği olmadan farklı kimlikle kullanılabilmesidir.

### 9. Pop-up ve duyuru şeridi (Faz 2 Görev 6'da netleşti/uygulandı)

`Popup` aggregate'i iki görünüm moduna sahiptir: `Modal` ve `Banner` (sitenin üstünde ince bilgi bandı).

| Alan | Açıklama |
|---|---|
| `DisplayMode` | `Modal` / `Banner` |
| `ImageMediaId` | Opsiyonel, yalnızca `Modal` (`Banner`'da görsel olamaz) |
| `LinkTarget` | Menu/Slider ile paylaşılan ortak value object (§7); opsiyonel, buton etiketi varsa zorunlu |
| Çeviriler | `Title` (maks 150; `Banner`'da opsiyonel), `Body` (sanitize; `Banner`'da maks 300 karakter düz metin, HTML yok), `ButtonLabel` (maks 50) |
| `Targeting` | `PopupTargeting` value object: `AllPages` / `HomeOnly` / `Contents` (en fazla 50 `ContentItem` kimliği) / `Paths` (en fazla 20 yol; her biri tam yol ya da `/*` ile biten önek, dil öneki içermez) |
| `DeviceTarget` | `All` / `Desktop` / `Mobile` |
| `PublishAtUtc`, `UnpublishAtUtc`, `IsActive` | Zamanlama |
| `DelaySeconds` | 0-60; `Banner`'da her zaman 0 |
| `Frequency` | `EveryVisit` / `OncePerSession` / `EveryNDays` (+ `FrequencyDays`, 1-365, yalnızca `EveryNDays`); takip tarayıcıda yapılır |
| `Dismissible` | `Banner`'da seçilebilir; `Modal`'da her zaman `true` (istenen değerden bağımsız) |
| `Priority` | 0-100; aynı anda tek modal gösterilir: en yüksek öncelikli |

Kurallar: aynı anda en fazla 20 aktif ve süresi dolmamış pop-up - veritabanı genelinde sayım gerektirdiği için, `ContentType.RoutePrefix` benzersizliği gibi, Application katmanındaki komut handler'ında doğrulanır. Görünürlük (`IsActive` + zaman aralığı) tek bir expression'dır (`PopupVisibility`).

Public yanıt: `GET /api/v1/public/site`'nin `popups` alanı - o anda görünür ve istenen dilde çevirisi olan tüm pop-up/şeritler, `Priority` azalan sıralı. `Contents` hedeflemesi istenen dildeki yollara çevrilir (görünmeyen içerikler listeden çıkarılır); link'i çözümlenemeyen pop-up'ın butonu gizlenir, pop-up'ın kendisi gösterilir (Slide'ın aynı kuralı, §8.2). **Hangi pop-up'ın gösterileceğine frontend karar verir**: sayfa yoluna uyan en yüksek öncelikli bir `Modal` ve bir `Banner`. `PopupMediaUsageProvider` ile kullanımdaki görsel silinemez.

Çerez onay banner'ı bu yapının dışındadır (bkz. §13).

### 10. Arama

İki katman:

**Liste içi arama ve filtre:** Her liste endpoint'i metin, kategori, etiket ve tarih aralığı filtrelerini destekler; `SupportsEvent` türlerinde format, konum, yaş aralığı eklenir. Hangi filtrelerin geçerli olduğu ContentType bayraklarından çıkar. Sayfalama SharedKernel `PagedRequest/PagedResult<T>` ile yapılır.

**Genel site araması** (Faz 5, uygulandı):

- `SiteSettings.GlobalSearchEnabled` bayrağı ile kurulumdan kuruluma açılıp kapanır; kapalıyken `GET /api/v1/public/search` `404` (`Website.Search.NotAvailable`) döner.
- Website bir **`SearchDocument`** read-model'i tutar (aggregate değil basit entity): `SourceKey` (maks 50: `website`, `employer.job`…), `SourceId` (maks 100), `LanguageCode`, `TypeKey` (maks 50), `Title` (maks 300), `Summary` (maks 500), `Url` (maks 500), `NormalizedText` (`TurkishTextNormalizer` çıktısı, maks 4000 — **yalnızca eşleme için**, yanıtta dönmez), `PublishedAtUtc`, `IndexedAtUtc`, `IncludeInSitemap`. Benzersiz indeks `(SourceKey, SourceId, LanguageCode)`; ek indeksler `(LanguageCode, TypeKey)` ve `PublishedAtUtc`. **`SearchSourceState`** (kaynak başına bir satır, benzersiz `SourceKey`): `LastStartedAtUtc`, `LastSucceededAtUtc`, `LastError` (maks 500, kırpılmış istisna mesajı), `DocumentCount`.
- **`TurkishTextNormalizer`** ADR-020 kapsamında Candidate modülünde tanımlanmıştı; Faz 5'te davranışı birebir korunarak `GenclikMerkezi.SharedKernel.Text.TurkishTextNormalizer`'a taşındı (`src/BuildingBlocks/SharedKernel/Text/TurkishTextNormalizer.cs`) ve Candidate'in kullanımları (`CandidateSearchIndexProjector`, `CandidateSearchIndexRepository`) güncellendi — Website, Candidate'e bağımlı olamayacağı için bu taşıma önce yapıldı (mimari test bunu doğrular).
- Website'in kendi içerikleri (`IsSearchable` + aktif tür, görünür — kendisi ve tüm ataları yayında/zamanlaması geçerli — içerik, her dil çevirisi ayrı `SearchDocument`) her mutasyon handler'ından `ISearchIndexUpdater` portu ile **aynı Unit of Work içinde** indekslenir. Zamanlanmış yayın/yayından kalkma bir mutasyon üretmediği için `ReconcileWebsiteSearchIndexJob` (Hangfire recurring, sabit `*/10 * * * *`) indeksteki `website` kaynağını gerçek görünür kümeyle karşılaştırıp eksikleri ekler/fazlaları siler; aynı mantık admin'in anında yeniden indeksleme komutuyla da tetiklenebilir. `NoIndex` işaretli çeviri aramada yer alır ama `IncludeInSitemap = false`'tur.
- Dış kaynaklar için **pull modeli** kullanılır. Website `IExternalSearchSource` port'unu tanımlar (`SourceKey`, sayfalı `GetPublishedDocumentsAsync`). Host katmanında `EmployerJobSearchSource` adaptörü (`SourceKey = employer.job`) `IPublishedJobModuleContract`'ı sayfalayarak okur; `Url = {Website:JobsPublicPathPrefix}/{Slug}` (varsayılan `/ilanlar`), `SearchableText` = başlık + firma + il + pozisyon + çalışma şekli + çalışma yeri tipi + özet birleşimi, `IncludeInSitemap = true`. **İlanlar tek dillidir:** yalnızca site varsayılan dilinde indekslenir (varsayım — diğer dillerde ilan aramada çıkmaz). `PublicJobListingsEnabled = false` iken kaynak boş sayfa döner; bu, bir sonraki tam senkronda önceden indekslenmiş tüm ilanların silinmesini tetikler.
- `SyncExternalSearchSourcesJob` (Hangfire recurring, varsayılan 10 dakika, `Website:Search:ExternalSyncIntervalMinutes`) her kayıtlı dış kaynak için **tam senkron** yapar: sayfa sayfa çeker, `(SourceKey, SourceId, LanguageCode)` ile upsert eder, görülen anahtar kümesini tutar; **sayfalardan biri hata verirse görülmeyenler silinmez** (kısmi senkron bırakılır, zaten eklenenler kalır). Kaynaklar birbirinden **izole** çalışır — biri istisna fırlatırsa yalnızca kendi `SearchSourceState.LastError`'ına yazılır, döngü diğer kaynaklara devam eder. Eşzamanlı çalıştırma koruması (`SearchSourceSyncCoordinator`) **process-içi** (in-memory) bir kilittir — tek Hangfire sunucusu/tek instance dağıtımı varsayımıyla kabul edilmiştir; çok-instance dağıtımda veritabanı seviyeli bir kilide geçilmesi gerekir (bu ADR kapsamında ele alınmamış bir sınırlamadır).
- **Yönetim uçları** (`Website.Settings.Manage`): `GET /api/v1/admin/website/search/sources` (her zaman `website` + kayıtlı tüm dış kaynaklar, hiç çalışmamışsa `null`/`0` ile listelenir), `POST /api/v1/admin/website/search/sources/{sourceKey}/reindex` (`website` → uzlaştırma, dış kaynak → tam senkron; bilinmeyen kaynak `404`, zaten çalışıyorsa `409` `SearchSource.AlreadyRunning`).
- Gerekçe: Employer CAP kısıtı nedeniyle güvenilir event yayınlayamaz; ilan yayın komutlarına Host'tan müdahale etmek (push) ise Employer'ın iç akışını Host'a sızdırır. Pull modeli iki modülü de birbirinden habersiz bırakır; bedeli, ilanların aramaya dakikalar düzeyinde gecikmeyle yansımasıdır ve kabul edilmiştir.
- **Eşleme (`GET /api/v1/public/search`):** `q` (2-100 karakter, trim) `TurkishTextNormalizer` ile normalize edilir, boşluktan **en çok 6 belirtece** bölünür; her belirteç `%belirteç%` kalıbına sarılır, `%`/`_`/`[` (ve kaçış karakterinin kendisi) kaçışlanır, `NormalizedText` üzerinde her belirteç için ayrı bir `EF.Functions.Like(...)` **AND**'lenerek uygulanır. **Bu, baştaki `%` nedeniyle indeksli bir arama değildir** — `(LanguageCode, TypeKey)` indeksi yalnızca eşitlik filtrelerine yardım eder, `LIKE '%x%'`'in kendisi dil+kaynak filtreleriyle sınırlanmış bir tarama yapar (PERFORMANCE.md §12.1). SQL Server full-text'e yalnızca ölçümle ihtiyaç kanıtlanırsa geçilir.
- **Sıralama:** (1) başlıkta tüm belirteçler geçenler önce, (2) `PublishedAtUtc` azalan, (3) `Id`. **`typeCounts`:** dil/kaynak/belirteç filtreleri uygulanmış ama **tür filtresinden önceki** taban sorgu üzerinden hesaplanır (bilinçli faceting davranışı — tür sekmeleri arasında geçiş yaparken diğer türlerin sayısı sabit kalır).
- **Koruma:** `public-search` rate limit policy'si (IP başına dakikada `RateLimiting:PublicSearchPermitLimit`, varsayılan 30, test ortamında yüksek). **`q` metni hiçbir yerde loglanmaz/saklanmaz**; yanıt sunucu tarafında **cache'lenmez**.
- İndekse yalnızca public içerik girer; kişisel veri, firma iç kaydı veya danışman notu tasarım gereği aramada yer alamaz.

### 11. Etkinlikler

#### 11.1. Etkinlik bilgisi

Etkinliğin *içeriği* ayrı bir aggregate değildir; `SupportsEvent` açık türlerdeki (Etkinlik, Eğitim/Atölye) bir ContentItem'dır. Böylece çeviri, SEO, galeri, menü ve arama ortak altyapıdan gelir. Etkinliğin *takvim ve kontenjan* bilgisi ise ContentItem ile 1:1 bağlı, ayrı bir **`EventSchedule`** aggregate'idir.

`EventSchedule`'ın owned entity değil ayrı aggregate olmasının nedeni concurrency'dir: kayıt işlemleri kontenjan sayaçlarını sık günceller. Sayaçlar ContentItem ile aynı satırda/rowversion'da olsaydı, her kayıt editörün açık düzenlemesini çakışmaya düşürürdü (ve tersi). İki aggregate aynı modülün DbContext'inde olduğu için, etkinlik oluşturma/düzenleme komutu ikisini tek Unit of Work'te kaydeder. DOMAIN.md'deki `Website.Training`, Eğitim türündeki ContentItem'dır; CareerDevelopment yalnızca `ContentItemId` referansı tutar.

`EventSchedule`: başlangıç/bitiş, format (`InPerson`/`Online`/`Hybrid`; `Online`/`Hybrid` için mutlak `https` online link zorunlu), kontenjan (null = sınırsız, aksi halde ≥ 1), başvuru açılış/kapanış zamanı (kapanış ≤ başlangıç), yaş aralığı (**yalnızca bilgi amaçlı** - doğum tarihi toplanmaz, kayıtta yaş denetimi yapılmaz), ücret bilgisi (metin), eğitmenler, program akışı (sanitize edilmiş zengin metin), erişilebilirlik notu, `IsCancelled` ve iptal açıklaması, `RegistrationEnabled`, `WaitlistEnabled`, **`AutoConfirm`** (etkinlik başına seçilir: açıksa kontenjan varsa doğrulanmış kayıt otomatik `Confirmed` olur, kapalıysa `Applied` kalır ve admin onaylar), `ConfirmedCount`/`WaitlistedCount` sayaçları. Online link yalnızca onaylı katılımcılara e-postayla gönderilir, hiçbir public yanıtta yer almaz. Başlangıç zamanı geçmiş bir etkinliğin takvim/kapasite/kayıt alanları değiştirilemez (`Event.CannotModifyScheduleAfterStart`); yalnızca metinler ve iptal güncellenebilir. Kapasite, mevcut `ConfirmedCount`'un altına düşürülemez (`Event.CapacityBelowConfirmed`).

#### 11.2. Kayıt

`EventRegistration` aggregate'i.

- **Herkes kayıt olabilir.** Giriş yapmış kullanıcıda formu frontend doldurur; backend `UserId`'yi **yalnızca token'dan** alır, istemcinin gönderdiği kimliğe güvenmez (AGENTS §26). Her durumda ad, e-posta, telefon kayıt anındaki haliyle saklanır.
- **Anonim kayıtta e-posta doğrulaması zorunludur.** Doğrulanmamış kayıt (`PendingVerification`) kontenjandan yer tutmaz; 24 saat içinde doğrulanmazsa saatlik bir Hangfire job'ı ile silinir (en fazla 500/çalışma; sayaçlara dokunmaz, çünkü bu kayıtlar hiçbir sayaçta yer tutmuyordu - "kayıt `Rejected` olmaz, silinme job'ına bırakılır"). Giriş yapmış ve e-postası doğrulanmış kullanıcı için bu adım atlanır, kontenjan kararı aynı istekte verilir.
- Doğrulama linkiyle birlikte **iptal linki** de gönderilir (rastgele, süresiz, DB'de doğrulanan token - bülten çıkış tokenıyla aynı desen). İptal **idempotent**tir: zaten iptal edilmiş bir kayıt için aynı link tekrar açılırsa durum değişmeden `200` döner; başlamış etkinlikte iptal `409` döner.
- Aynı etkinliğe aynı e-postayla ikinci aktif (`PendingVerification`/`Applied`/`Confirmed`/`Waitlisted`) kayıt engellenir. Anonim çağıran için yanıt **tekdüzedir** (kayıt sızdırmaz): mevcut kayıt `PendingVerification` ise doğrulama e-postası yeniden gönderilir (kayıt başına 10 dakikada en fazla 1 gönderim), diğer aktif durumlarda "zaten kayıtlısınız" bildirimi + iptal linki gönderilir. Giriş yapmış kullanıcı için yanıt doğrudan `409` (`Event.AlreadyRegistered`) - kimliği zaten doğrulanmış olduğundan sızdırma riski yoktur.
- Onaylanan aydınlatma metni sürümü kaydedilir (§12); yürürlükteki sürüm değişmişse `409` (`PublicSubmission.LegalVersionChanged`).

Durumlar:

```text
PendingVerification ──► Applied ──► Confirmed ──► Attended / NoShow
                           │            │
                           ├──► Waitlisted ──► Confirmed
                           └──► Rejected
Her aktif durumdan ──► Cancelled
```

Kontenjanı yalnızca **kontenjanı tutan durumlar** (`Confirmed`, `Attended`, `NoShow`) doldurur; `Applied` ve `PendingVerification` hiçbir sayaçta yer tutmaz. Kontenjan kararı (doğrulanmış kayıt için) tek bir yerde (`EventSchedule.ReserveCapacity`) verilir: etkinlik iptal/kapalıysa veya kayıt penceresi dışındaysa reddedilir (`Event.Cancelled` / `Event.RegistrationClosed`); kapasite sınırsız veya boşsa `AutoConfirm` açıkken `Confirmed` (+1 `ConfirmedCount`), kapalıyken `Applied` (sayaç değişmez); kapasite doluyken `WaitlistEnabled` açıksa `Waitlisted` (+1 `WaitlistedCount`), kapalıysa `409` (`Event.CapacityFull`).

- **Kontenjan eşzamanlılığı:** `EventSchedule` aggregate'inde onaylı/yedek sayaçları ve **optimistic concurrency (rowversion)** kullanılır; kayıt komutu `EventRegistration`'ı oluşturup `EventSchedule` sayacını aynı Unit of Work'te günceller. Çakışmada işlem **en fazla 3 kez** yeniden denenir, sonra `409`.
- Bir yer açıldığında yedekten otomatik terfi yapılmaz; admin onaylar (`confirm` ucu, ileride otomatikleştirilebilir). Admin onayı hem `Applied → Confirmed` hem `Waitlisted → Confirmed` (terfi, aynı Unit of Work'te `-1 Waitlisted`/`+1 Confirmed`) için kullanılır; kontenjan doluysa `409` (`Event.CapacityFull`).
- Etkinlik iptal edildiğinde `Applied`/`Confirmed`/`Waitlisted` kayıtlara bildirim gider (kayıtların kendi durumu değişmez, geçmiş korunur); gönderim commit sonrası best-effort, en fazla 200 alıcı tek seferde işlenir, fazlası Hangfire job'ıyla parçalanır.
- Tüm e-postalar commit sonrası `IWebsiteEmailSender` ile best-effort gönderilir (CAP kısıtı). Doğrulama e-postası için **yeniden gönder** endpoint'i vardır. Onay e-postasında online link yalnızca `Online`/`Hybrid` etkinlikte yer alır; bildirim e-postaları başvuru dışı kişisel veri taşımaz.
- **Saklama/anonimleştirme:** etkinlik bitiminden **365 gün** sonra (sabit, `SECURITY.md` §12.4 - hukuk danışmanınca teyit edilmelidir) günlük bir job `FirstName`/`LastName`/`Email`/`Phone`/`UserId` alanlarını temizler (`AnonymizedAtUtc`); durum, tarihler ve durum geçmişi kalır, iptal/doğrulama token'ları işlevsiz hale gelir (kayıt artık değiştirilemez). En fazla 500/çalışma.
- Takvim dosyası (`.ics`, RFC 5545, tek `VEVENT`, online link ve kişisel veri içermez) ve katılımcı listesi CSV dışa aktarımı (UTF-8 BOM, CSV enjeksiyonu kaçışı; sütunlar `firstName,lastName,email,phone,status,registeredAtUtc,confirmedAtUtc,language`) sunulur. Katılımcı listesi hiçbir zaman public olmaz; public uçlar yalnızca `registrationState` (`NotOpen`/`Open`/`Full`/`WaitlistOpen`/`Closed`/`Cancelled`, tek bir saf fonksiyonda hesaplanır) ve `remainingSpots` döner.
- QR ile giriş, yoklama ekranı, sertifika ve katılım puanı bu ADR kapsamında **yoktur.** Etkinlik öncesi hatırlatma e-postası ve etkinlik tarihi/yeri değişince kayıtlılara bildirim de **kapsam dışıdır** (ileride ayrı bir karar gerektirir).

#### 11.3. Public etkinlik uçları

`GET /api/v1/public/events` (filtreler: `typeKey`, `when` - `upcoming`/`past`/`all`, `from`/`to`, `format`, `lang`, sayfalama) ve içerik detayına eklenen `event` alanı, `EventSchedule`'ın public kısmını döner; `OnlineLink` ikisinde de **yer almaz**. `GET /api/v1/public/events/{contentItemId}/calendar.ics?lang=` tek bir `.ics` dosyası üretir.

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
- Özellik bayrakları (yalnızca public siteyi ilgilendirenler): `GlobalSearchEnabled`, `NewsletterEnabled`, `PublicJobListingsEnabled`, `DonationPageEnabled` (bu projede `false` başlar), `BotProtectionEnabled` (bu projede `true`), `AllowSearchEngineIndexing` (bool, varsayılan `true`; Faz 5 — `false` iken sitemap boş döner/segment 404 verir, bakım modu + bu bayrak birlikte `false` iken `robots.txt` tüm siteyi `Disallow: /` yapar, bkz. §15)
- Bakım modu (admin'ler için bypass)

Başka modüllerin backend davranışını değiştiren bayraklar (ör. istihdam aracılığı) Website'e ait değildir.

**Script yönetimi:** Üçüncü taraf script'ler (analitik vb.) kategoriyle (`Necessary`, `Analytics`, `Marketing`) tanımlanır. Frontend, ziyaretçi ilgili kategoriye onay vermeden script'i yüklemez. Onay tercihi tarayıcıda tutulur.

### 14. Bülten

- `NewsletterSubscriber`: e-posta, dil, onaylanan yasal metin sürümü, durum (`PendingConfirmation → Active → Unsubscribed`).
- **Çift onay:** Abonelik, e-postadaki linke tıklanınca aktifleşir.
- Her e-postada imzalı abonelikten çıkma linki bulunur.
- Bu ADR kapsamında bülten *gönderim* aracı (kampanya editörü) yoktur; abone yönetimi ve dışa aktarım vardır. Derneğin bülteninin ticari elektronik ileti / İYS kapsamına girip girmediği hukuken kontrol edilir.

### 15. SEO altyapısı ve route çözümleme

- **`Redirect`**: kaynak yol → hedef yol, 301/302, otomatik veya elle oluşturulmuş. Kaynak yol (`FromPath`) dil içinde benzersizdir; bir yönlendirmenin hedefi başka bir yönlendirmenin kaynağı olamaz (zincir reddedilir). Elle oluşturulan yönlendirmeler düzenlenebilir/silinebilir, otomatik olanlar yalnızca silinebilir.
- **404 kaydı (`NotFoundLog`):** Bulunamayan yollar sayılarak loglanır (yol + sayı + ilk/son görülme; IP veya User-Agent tutulmaz). Bu bir **komuttur** (`RecordNotFoundPathCommand`), route çözümleme sorgusunun kendisi değil - AGENTS.md §13 (bir sorgu yazmaz) gereği, çözümleme isteğini işleyen public endpoint, sonucu `NotFound` olduğunda bu komutu ayrıca gönderir; komut başarısız olursa yalnızca loglanır, ziyaretçinin yanıtını etkilemez. En fazla 10.000 farklı yol tutulur (sınıra ulaşınca yeni yol eklenmez, var olanların sayacı artmaya devam eder); günlük bir arka plan job'ı 90 günden eski ve sayacı 5'in altında kalan kayıtları siler. Admin panelinde en çok 404 alan yollar listelenir, tek adımda (ve tek transaction'da) yönlendirmeye dönüştürülebilir.
- **Route çözümleme:** `GET /api/v1/public/routes/resolve?path=...` (anonim). İlk yol segmenti aktif ve varsayılan olmayan bir dilin koduysa o dil seçilir ve segment yoldan çıkarılır; varsayılan dilin kodu önek olarak kullanılmışsa öneksiz yola, pasif bir dilin kodu önek olarak gelirse `NotFound`'a çözümlenir. Yol küçük harfe çevrilir, ardışık `/` teke indirilir, sondaki `/` kaldırılır; sonuç istenen yoldan farklıysa tek bir 301 yönlendirmeyle kanonik yola gidilir (dil öneği düzeltmesiyle birlikte, tek kanonik URL). Çözümleme sırası: boş yol → `Home`; tam eşleşen görünür içerik (kendisi ve tüm ataları görünür, türü aktif ve `HasDetailPage`) → `Detail`; tek segment ve aktif, `HasListingPage` açık bir türün o dildeki `RoutePrefix`'i → `Listing`; bir `Redirect` kaydı (hedefi görünmeyen bir içerikse uygulanmaz) → `Redirect`; hiçbiri → `NotFound`. Çözümleme (dil, `FullPath`) ve (dil, `FromPath`) üzerindeki tekil indekslerle tek satır aramalarıdır. İçeriğin kendi verisi (başlık, gövde, breadcrumb, görseller) bu endpoint'te dönmez - yalnızca "bu yol ne tür bir şeye çözümleniyor" bilgisini verir.
- **Public liste ve detay** (Faz 1b Görev 7, uygulandı): `GET /api/v1/public/contents?type=...` (anonim, sayfa başına tek istek) - tür, kategori (alt kategoriler dahil), etiket, tarih aralığı, öne çıkan ve metin arama filtreleriyle sayfalı liste döner; `HasDetailPage=false` türlerde (SSS, Ekip, Belge) her öğenin gövdesi listede satır içi döner (`Path=null`), aksi halde yalnızca `Path` döner. `GET /api/v1/public/contents/{id}?lang=...` (anonim) - içeriğin kendisi ve tüm ataları görünür değilse (§4.4) veya türün `HasDetailPage`'i kapalıysa 404 döner; aksi halde galeri, video, ek dosya, kategori, etiket, alt içerik (hiyerarşik türlerde), ilişkili içerik, breadcrumb (Ana Sayfa → tür liste sayfası varsa → atalar → kendisi), `hreflang` karşılıkları ve çözümlenmiş SEO alanlarıyla tek istekte döner. Her iki endpoint de SSR/ön-render kararından bağımsız kullanılabilir.
- **SEO çözümleme zinciri** (`ContentSeoResolver`, Faz 1b Görev 7): `MetaTitle` boşsa başlığa, `MetaDescription` boşsa özetin kelime sınırında 160 karaktere kırpılmış haline, o da boşsa site varsayılanına düşer; `OgTitle`/`OgDescription` boşsa çözümlenmiş meta alanlarına düşer; `OgImage` sırasıyla açık override → detay görseli → kapak görseli → site varsayılan görseline düşer. Bu zincir liste sayfası (tür/kategori SEO'su), detay sayfası (içerik SEO'su) ve `GetContentPreview` (Faz 1b Görev 6, sonradan uygulanan düzeltme) tarafından paylaşılır - önizleme yanıtı da aynı kurallarla çözülmüş bir SEO bloğu döner (taslak içerik zaten `noindex`/`no-store` başlıklarıyla arama motorlarından gizlendiği için bu, önizlemenin arama motorunda görünmesi değil, yalnızca önizleme/yayın arasında SEO alanlarının tutarlı görünmesi amacını taşır).
- **Breadcrumb** hiyerarşiden otomatik üretilir.
- **Sitemap ve robots** (Faz 5, uygulandı): `GET /sitemap.xml` ve `GET /robots.txt` **kök yolda**, `/api/v1/public` öneki **dışında** anonim olarak sunulur (`public-read` rate limit). Sitemap'e giren: ana sayfa, `IsActive && HasListingPage` türlerin liste sayfaları, görünür (§4.4) **ve** `NoIndex = false` olan `HasDetailPage` içerikler, dış kaynak belgeleri (`IncludeInSitemap = true`). Etkinlikler: yaklaşan + son 90 gün içindeki, daha eskiler hariç. Her URL için `lastmod` (`UpdatedAtUtc` → `PublishedAtUtc` → `CreatedAtUtc` sırasıyla düşer) ve `hreflang`/`x-default` alternatifleri üretilir (`System.Xml.Linq` ile, `&`/`<` otomatik kaçışlanır). URL sayısı `Website:Sitemap:MaxUrlsPerSitemapFile` (varsayılan 10.000) eşiğini aşarsa `/sitemap.xml` bir sitemap index'e döner, parçalar `/sitemap-{n}.xml`'de sayfalanır. Toplanan URL listesi 1 saat cache'lenir (`WebsiteCacheInvalidator.InvalidateAllPublic` ile boşalır); `robots.txt` bilinçli olarak cache'lenmez. `SiteSettings.AllowSearchEngineIndexing = false` iken `/sitemap.xml` **boş** `<urlset>` döner (segment istekleri `404`); `robots.txt`, **bakım modu ve `AllowSearchEngineIndexing = false` birlikteyken** `Disallow: /` döner, aksi halde `Website:Robots:DisallowedPaths` (varsayılan `/api/`, `/admin`, `/portal`, `/webuploads/private`) listesini ve `Sitemap:` satırını döner. **Dağıtım notu:** API ve public site ayrı host ise `/sitemap.xml`/`/robots.txt` isteklerinin API'ye yönlendirilmesi dağıtım yapılandırmasıdır (IIS URL Rewrite, ters proxy yok); `Website:PublicSiteBaseUrl` doğru ayarlanmalıdır.
- **Yapılandırılmış veri (schema.org, Faz 5, uygulandı):** Backend JSON-LD'yi hazır, tipli nesne olarak döner (`jsonLd`); frontend yalnızca `<script type="application/ld+json">` içine basar. `ContentType.SchemaKind` (`None`/`Article`/`NewsArticle`/`FaqPage`) içerik türüne göre seçilir (seed: Haber/Duyuru/Basın Bülteni → `NewsArticle`; Proje/Faaliyet/Başarı Hikayesi → `Article`; SSS → `FaqPage`; diğerleri `None`); `SupportsEvent` türlerde `SchemaKind`'den bağımsız olarak otomatik `Event` üretilir. İçerik detayına (`GET /api/v1/public/contents/{id}`) her zaman `BreadcrumbList`, varsa `Article`/`NewsArticle` ve/veya `Event` eklenir; `FaqPage` json-ld yalnızca liste yanıtına eklenir; `GetContentPreview` yanıtı **jsonLd içermez**. `Event` çıktısında **`OnlineLink` hiçbir koşulda yer almaz** (regresyon testiyle korunur); salt online etkinlikte `VirtualLocation.url` **etkinliğin kendi public sayfa URL'sidir**, online linkin kendisi değil. İlan için `JobPosting` şeması **uygulanmamıştır** (kapsam dışı). `GET /api/v1/public/site` yanıtına `Organization` (ad, `url`, `logo`, `sameAs`, `contactPoint`, `address`) eklenir; boş alanlar yazılmaz. Tüm URL'ler `Website:PublicSiteBaseUrl` ile mutlak yapılır. **Güvenlik notu:** frontend JSON'u `<script>` içine basarken `<` karakterini `<` olarak kaçışlamalıdır (script kırma/XSS riski, bkz. SECURITY.md §23.5).

### 16. Güvenlik

- **Zengin metin:** Frontend editörü **TipTap (MIT)**. Backend, gelen HTML'i **HtmlSanitizer (Ganss.Xss, MIT)** ile beyaz liste bazlı temizler; frontend'e güvenilmez. İzin verilen etiket/öznitelik listesi Faz 0'da belirlenir. `iframe` yalnızca YouTube-nocookie alan adına izinlidir.
- **Lisans ilkesi:** Bu modülde yalnızca ücretsiz ve ticari kullanımda kısıt getirmeyen lisanslar (MIT, Apache-2.0, BSD) kullanılır. GPL/ticari çift lisanslı editörler (CKEditor 5, TinyMCE 7) ve gelire bağlı lisanslı kütüphaneler bu nedenle reddedilmiştir.
- Dosya yüklemelerinde MIME ve uzantı doğrulaması, boyut limiti (ADR-019 doğrulama politikası).
- Tüm yönetim işlemleri (yayınlama, silme, yasal metin yayını, başvuru görüntüleme) ADR-015 audit kapsamına alınır.

### 17. Performans ve cache

- Public okumalar SharedKernel cache'i (ADR-017) ile cache'lenir. Yayınlama, güncelleme, kaldırma ve ayar değişikliklerinde ilgili anahtarlar invalid edilir.
- Zamanlanmış yayın nedeniyle cache TTL'i, ilgili listedeki bir sonraki `PublishAtUtc` / `UnpublishAtUtc` anını aşmaz.
- Liste sorguları projection + `AsNoTracking` kullanır; çeviri tablosu dil filtresiyle join edilir, tüm çeviriler yüklenmez.
- **Invalidation granülerliği** (Faz 1b Görev 7, uygulandı): tek bir kaba taneli önek (`website:public-content:`) kullanılır; `ContentItem`, `ContentType`, `ContentCategory`, `ContentTag`, `Video`, `MediaAsset`, `Redirect`, `SiteLanguage` veya SEO'yu besleyen `SiteSettings` alanlarını değiştiren **her** komut handler'ı, commit sonrası bu öneki tamamen temizler (`ICacheService.RemoveByPrefix`). Anahtar bazlı hedefli invalidation (ör. yalnızca etkilenen içeriğin/listelerin anahtarını silmek) yerine bu tercih edilmiştir - kaç farklı liste/detay anahtarının bir tekil mutasyondan etkilendiğini önceden çıkarmak (kategori ağacı, ilişkili içerik, alt içerik zincirleri, alternates) kırılgan ve hataya açık bir bağımlılık grafiği gerektirirdi. Bedeli, ilgisiz bir mutasyonun da tüm public içerik cache'ini temizlemesidir; bu, düşük yazma/yüksek okuma oranı ve public trafiğin çoğunlukla ilk isteğin cache'i yeniden dolduracağı varsayımıyla kabul edilebilir bulunmuştur (ADR-024 Faz 1b master prompt'unda "kaba taneli temizlik kabul edilebilir" olarak onaylanmıştır). `NotFound` rota çözümleme sonuçları hiçbir zaman cache'lenmez (bir sonraki isteğin güncel veriyi görmesi için); arama (`search`) sorguları da sınırsız anahtar alanı nedeniyle cache'lenmez. Her mutasyon handler'ının invalidation'ı çağırdığı `tests/ArchitectureTests/PublicContentCacheInvalidationTests.cs` ile (dosya bazlı çağrı taraması + gerekçeli istisna listesi) test edilir.

**Faz 2 genişletmesi** (menü/slider/pop-up, Görev 1/2/6): `GET /api/v1/public/site` yanıtı artık menülere (içerik/tür hedefli `LinkTarget`'lar üzerinden) ve pop-up'lara bağlı olduğu için, `ContentItem`/`ContentType`/`ContentCategory` mutasyonlarını işleyen her komut handler'ı artık `InvalidatePublicContent` yerine `InvalidateAllPublic` (hem public-content hem public-site önekini aynı çağrıda temizleyen yardımcı) çağırır. Public site cache'inin TTL'i de `ContentCacheTtlCalculator` yeniden kullanılarak, site genelindeki en yakın zamanlama anına göre (tüm içerik türlerindeki `PublishAtUtc`/`UnpublishAtUtc`, her slider'ın slide zamanlamaları, her aktif pop-up'ın kendi zamanlaması) kısaltılır. `PublicContentCacheInvalidationTests` mimari testi bu fazda eklenen tüm mutasyon handler'larını kapsayacak şekilde güncellenmiştir.

### 18. Faz planı

| Faz | Kapsam |
|---|---|
| **0 — Temel** ✅ | WebsiteDbContext, SiteLanguage, çeviri deseni, `Slug`/`Seo` value object'leri, HtmlSanitizer, isimli policy'ler, MediaAsset + SkiaSharp boyutları + public medya servisi, SiteSettings (tema, iletişim, IBAN, bayraklar, bakım modu), `IBotProtectionVerifier` + Turnstile adaptörü, `IWebsiteEmailSender` + Notification adaptörü |
| **1a — İçerik çekirdeği (tür, içerik, yol, yönlendirme)** ✅ | ContentType, ContentItem (çeviri, hiyerarşi, durum/zamanlama, sıra, öne çıkan), yol hesaplama + otomatik `Redirect`, elle yönlendirme yönetimi, 404 kaydı (`NotFoundLog`), public route çözümleme (`GET /api/v1/public/routes/resolve`) |
| **1b — İçerik çekirdeği (devamı)** ✅ | Galeri, video kütüphanesi + içerik video listesi, dosya ekleri, kategoriler, etiketler, ilişkili içerik, önizleme linki, çöp kutusu + kalıcı silme job'ı, kopyalama, public liste/detay endpoint'leri (`GET /api/v1/public/contents`, `GET /api/v1/public/contents/{id}`), breadcrumb, public cache + invalidation. **Kapsam dışı bırakılan (bu fazda uygulanmadı):** form bağlantısı (`FormDefinition`/`FormSubmission` motoru Faz 3'te gelir; `SupportsForm` bayrağı ve içerik-form ilişkisi alanı şimdilik veri modelinde yer tutar ama işlevsel değildir) |
| **2 — Sunum** ✅ | `LinkTarget` + `LinkTargetResolver` (Menu/Slider/Popup'ın paylaştığı ortak hedef VO'su), konum bazlı `Menu` (`Header`/`Utility`/`Footer`; ayrı mobil menü yok, frontend `Header`'dan üretir), `Slider` + zamanlanmış `Slide`'lar, `Partner`, `ImpactMetric`, kod tabanlı blok tipi kaydı (`BlockTypeFieldDescriber` ile tek kaynaktan `GET .../block-types`) + taslak/yayın `PageLayout` (`Home` ve `SupportsBlockLayout` içerikler), public ana sayfa (`GET /api/v1/public/home`) ve içerik detayına eklenen `blocks` alanı, `Popup`/`Banner` (hedefleme, sıklık, öncelik) |
| **3 — Etkileşim** ✅ | Anonim gönderim koruması (`public-forms` rate limit, imzalı/süreli gönderim token'ı, honeypot, minimum doldurma süresi, `IBotProtectionVerifier`); sürümlü `LegalDocument` (taslak/yayın, yürürlük tarihi, değiştirilemezlik); tipli alanlı `FormDefinition` (alan tanımı sürümü, yasal metin bağlantıları, içerik–form bağlantısı); dosya ekli, yasal onaylı, referans numaralı `FormSubmission` (eşzamanlı-güvenli sıra üretimi, özel depolama kökü, dosya imzası doğrulaması); başvuru durum makinesi, atama, iç not, iki aşamalı saklama (30 gün sonra arşiv, form bazlı saklama süresi sonunda anonimleştirme) ve kişisel veri erişim kaydı (`PersonalDataAccessLog`); çift onaylı `NewsletterSubscriber` (kayıt sızdırmayan akış, CSV dışa aktarım); ham script kabul etmeyen tipli `ThirdPartyScript` sağlayıcıları ve kimliksiz anonim `CookieConsentRecord` |
| **4 — Etkinlik** ✅ | Etkinlik başına seçilebilen `AutoConfirm`/`WaitlistEnabled` ile `EventSchedule` (kontenjan, kayıt penceresi, geçmiş etkinlikte kısıtlı güncelleme); gerçek `EventDateAsc` sıralaması, public etkinlik listesi/detayı ve hesaplanan `registrationState`/`remainingSpots`, public `.ics`; doğrulamalı/anonim `EventRegistration` (24 saatlik doğrulama + temizlik job'ı, kayıt sızdırmayan yinelenen kayıt akışı, idempotent iptal linki, rowversion ile en fazla 3 yeniden denemeli kontenjan eşzamanlılığı); admin kayıt yönetimi (onay/yedek terfi/reddet/iptal/`Attended`/`NoShow`, kişisel veri erişim kaydı); etkinlik iptali bildirim fan-out'u; 365 günlük anonimleştirme job'ı ve katılımcı CSV dışa aktarımı. **Kapsam dışı:** etkinlik öncesi hatırlatma e-postası, etkinlik tarihi/yeri değişince kayıtlılara bildirim, yedekten otomatik terfi, QR giriş/yoklama ekranı/sertifika/katılım puanı |
| **5 — Keşif** ✅ | `SearchDocument`/`SearchSourceState` + genel arama (`GET /api/v1/public/search`, belirteç AND eşlemesi, `typeCounts`, sorgu metni saklanmaz); Website içeriğinin transaction içi indekslenmesi + zamanlanmış görünürlük için uzlaştırma job'ı; `IExternalSearchSource` + Host'taki `EmployerJobSearchSource` adaptörü + tam senkron job'ı (kısmi hatada silme yok, kaynak izolasyonu); admin arama kaynağı yönetimi/yeniden indeksleme; dinamik `/sitemap.xml` (+ index bölme) ve `/robots.txt` (+ `AllowSearchEngineIndexing`); `ContentType.SchemaKind` ile schema.org JSON-LD (`BreadcrumbList`/`Article`/`NewsArticle`/`Event`/`FaqPage`/`Organization`); `ContentItem` revizyon geçmişi (yakalama, karşılaştırma, geri yükleme, 50 revizyonluk saklama + yayınlanmış revizyonun korunması); `TurkishTextNormalizer`'ın Candidate'ten SharedKernel'e taşınması. **Kapsam dışı:** ilan için schema.org `JobPosting`; arama sorgusu analitiği/popüler aramalar; otomatik tamamlama; SQL Server full-text; `PageLayout`/`SiteSettings`/medya revizyonları; metin fark (diff) algoritması |

Her faz (gerektiğinde alt fazlara bölünerek, bkz. 1a/1b), ayrı görevlere bölünmüş kendi master prompt'uyla uygulanır; her görev ayrı commit'tir.

**Paralel iş (Employer modülü, bu ADR'nin kapsamı dışında) — ✅ Tamamlandı:** İlan listesine sayfalama ve filtre, ilan slug'ı ve public detay endpoint'i, public firma profili ve firmanın logosunun sitede gösterilmesine izin veren onay alanı, arama adaptörü için yayındaki ilan özetlerini sayfalı dönen public contract metodu (`IPublishedJobModuleContract`, bkz. ADR-023 §6). Faz 5'in arama adaptörü artık bu contract üzerine kurulabilir.

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
