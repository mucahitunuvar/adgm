# Website Modülü — Faz 5 (Keşif: Genel Arama, Sitemap/Robots, schema.org, Revizyon Geçmişi) Master Prompt

## 0. Nasıl kullanılır

Bu dosya **görev görev** uygulanır. Her görev ayrı bir Claude Code oturumunda yürütülür:

1. Kullanıcı `/clear` yapar ve şunu gönderir: *"docs/prompts/WEBSITE-FAZ5-MASTER-PROMPT.md dosyasındaki §1'i ve Görev N'i oku, yalnızca Görev N'i uygula."*
2. Yalnızca **§1** ve **ilgili görevi** okursun. Görevin **"Oku"** listesi dışındaki dokümanları okuma; AGENTS.md'nin yalnızca belirtilen bölümlerine bak.
3. Önceki görevlerin kodu commit'lenmiştir; ihtiyaç duyduğunda `git log` ve kaynak dosyalarından öğren.
4. Görev bitince commit'le, push'la, kısa raporunu yaz ve **dur**.

**Ön koşul:** `EMPLOYER-PUBLIC-JOBS-MASTER-PROMPT.md` tamamlanmış olmalı (özellikle `IPublishedJobModuleContract`). Görev 4'ten önce `src/BuildingBlocks/Contracts/Employer/IPublishedJobModuleContract.cs` yoksa **dur ve AGENTS.md §6 formatında raporla**; Employer'a kendin dokunma. Görev 1–3 ve 5–7 bu ön koşula bağlı değildir.

---

## 1. Genel kurallar (her görevde geçerli)

**Kullanıcı kararları (Faz 5 için alındı):**

- **İlan indeksleme** ADR-024 §10'daki **pull modeliyle** yapılır; Employer'ın public contract'ı (`IPublishedJobModuleContract`) ayrı bir Employer işi olarak önceden tamamlanır.
- **Revizyon geçmişi yalnızca `ContentItem`'ı kapsar** (çeviri metinleri, SEO, etiket/kategori). `PageLayout`, `SiteSettings` ve `LegalDocument` kapsam dışıdır (`LegalDocument` zaten sürümlüdür; `PageLayout` taslak/yayın modeline sahiptir).
- **Revizyon saklama: içerik başına son 50.** Daha eskileri günlük job siler; **yayınlanmış sürümün revizyonu her zaman korunur.**
- Genel arama `SiteSettings.GlobalSearchEnabled` ile açılıp kapanır (kapalıyken `feature_unavailable` ProblemDetails).
- İndekse yalnızca **public içerik** girer; kişisel veri, firma iç kaydı, danışman notu, başvuru/kayıt verisi tasarım gereği aramada yer alamaz.

**Mimari:** Faz 0–4 desenleri: vertical slice, `RowVersion` ile iyimser eşzamanlılık (`409`), `TimeProvider`, projection + `AsNoTracking`, Result deseni, tek tip-tek dosya, zengin metinler `IHtmlContentSanitizer` ile temizlenir. Yönetim uçları `/api/v1/admin/website/...`, public uçlar `/api/v1/public/...`; sitemap ve robots kök yoldadır (Görev 5). Hangfire recurring job'lar mevcut desende (`Program.cs` kaydı, `DisableConcurrentExecution` benzeri koruma neyse onu kullan).

**Yetkilendirme:**

| İşlem | Policy |
|---|---|
| Arama kaynakları ve yeniden indeksleme, `SiteSettings` bayrakları | `Website.Settings.Manage` |
| Revizyon listeleme, karşılaştırma, geri yükleme | `Website.Content.Manage` |

**Cache:** Public okumaları etkileyen mutasyonlar `WebsiteCacheInvalidator.InvalidateAllPublic` çağırır; mimari test yeni handler'ları kapsar. Arama sonuçları cache'lenmez (Görev 3).

**Test komutu (görev içinde):**

```bash
dotnet test GenclikMerkezi.slnx --filter "FullyQualifiedName~Website|FullyQualifiedName~Architecture|FullyQualifiedName~Candidate" --verbosity quiet --nologo
```

(Candidate filtresi yalnızca Görev 1'deki normalizer taşıması içindir; sonraki görevlerde `Website|Architecture` yeterli.) Her görevde **unit testlerin yanında integration testleri zorunludur** (SQLite + HTTP). Tam paket yalnızca Görev 8'de çalışır.

**Commit:** Açık dosya listesiyle stage et (`git add .` kullanma). Push'tan sonra `git log origin/main -1` ile doğrula.

**Çelişki:** Kod veya ADR ile çelişki bulursan kendin karar verme; AGENTS.md §6 formatında dur ve raporla.

**Rapor (kısa):** commit hash, migration, filtreli test sayıları, sapmalar ve varsayımlar.

---

## Görev 1 — Arama altyapısı: normalizer taşıma, `SearchDocument`, `IExternalSearchSource`

**Oku:** ADR-024 §10; `src/Modules/Candidate/Domain/TurkishTextNormalizer.cs` ve kullanıldığı yerler (`CandidateSearchIndexProjector`, `CandidateSearchIndexRepository`); ADR-020; Website `WebsiteDbContext` ve bir mevcut read-model/yapılandırma örneği; SharedKernel yapısı.

- **Normalizer taşıma (mimari düzeltme):** `TurkishTextNormalizer` Candidate modülünde duruyor, Website ona bağımlı olamaz. ADR-024 §10 "ADR-020 normalize fonksiyonu yeniden kullanılır" der. Sınıfı **davranışı birebir koruyarak** SharedKernel'e taşı (`GenclikMerkezi.SharedKernel.Text.TurkishTextNormalizer`; yeni dosya, eski dosya silinir), Candidate'in kullanımlarını güncelle, mevcut normalizer testlerini taşı. Davranış değişikliği **yasak** (mevcut `CandidateSearchIndex` verisiyle uyumluluk). ADR-020'ye taşıma notu Görev 8'de yazılır.
- **`SearchDocument`** (Website, read-model; aggregate değil, basit entity): `Id`, `SourceKey` (maks 50: `website`, `employer.job`…), `SourceId` (maks 100; kaynağa göre içerik kimliği), `LanguageCode`, `TypeKey` (maks 50: içerik türü anahtarı veya `job`), `Title` (maks 300), `Summary` (maks 500, düz metin), `Url` (maks 500; site-içi yol, dil öneki dahil), `NormalizedText` (başlık + özet + kısa gövde özeti; maks 4000, `TurkishTextNormalizer` çıktısı), `PublishedAtUtc`, `IndexedAtUtc`, `IncludeInSitemap` (bool). **Benzersiz indeks:** `(SourceKey, SourceId, LanguageCode)`; ek indeksler `(LanguageCode, TypeKey)` ve `PublishedAtUtc`. Kişisel veri alanı yoktur.
- **`SearchSourceState`** (kaynak başına bir satır): `SourceKey`, `LastStartedAtUtc`, `LastSucceededAtUtc`, `LastError` (maks 500, istisna mesajı **kırpılır**, yığın izi yok), `DocumentCount`.
- **`IExternalSearchSource`** (Application port): `string SourceKey`, `Task<PagedResult<ExternalSearchDocument>> GetPublishedDocumentsAsync(int page, int pageSize, CancellationToken)`; `ExternalSearchDocument` (ayrı dosya): `SourceId`, `TypeKey`, `Title`, `Summary`, `Url`, `SearchableText`, `PublishedAtUtc`, `IncludeInSitemap`. Sıralama kaynağın sorumluluğunda **kararlı** olmalıdır (port yorumunda belirt). Hiçbir kaynak kayıtlı değilse sistem sorunsuz çalışır.
- **`ISearchDocumentRepository`:** upsert (SourceKey, SourceId, LanguageCode), silme (tek doküman, kaynak+kimlik kümesi, kaynağın görülmeyen kümesi), kaynak bazlı sayım. EF yalnızca Infrastructure'da.
- **`SearchTextBuilder`** (saf): HTML → düz metin (etiketleri ve varlıkları temizle), özet üretimi (kelime sınırında kırp), `NormalizedText` birleştirme/kırpma. Görev 2 ve 4 bunu kullanır.

**Testler:** Normalizer taşıma sonrası Candidate testlerinin değişmeden geçmesi; `SearchTextBuilder` (HTML, varlık, kelime sınırı, uzunluk üst sınırları, Türkçe `İ/ı` örnekleri); benzersiz indeks; upsert idempotansı; port'un hiçbir kaynak yokken kırılmaması; mimari test (Website, Candidate'e bağımlı değil) (integration).

**Commit (iki commit):**
1. `refactor(shared): move TurkishTextNormalizer from Candidate to SharedKernel`
2. `feat(website): add search document read model and external search source port`

---

## Görev 2 — Website içeriğinin indekslenmesi

**Oku:** Görev 1'in kodu; ADR-024 §4.4 (görünürlük kuralı: içerik ve tüm ataları görünür), §10; `ContentItem` durum/zamanlama kodu; `ContentType.IsSearchable`; içerik oluşturma/güncelleme/yayın/arşiv/çöp kutusu/kopyalama/kalıcı silme/çeviri silme handler'ları; Faz 4'teki etkinlik içerikleri; `WebsiteCacheInvalidator` kullanımı.

- **İndekslenecek:** yalnızca türü `IsSearchable` + aktif, **görünür** (kendisi ve tüm ataları yayında, zamanlaması geçerli, `HasDetailPage` açık) içerik; her dil çevirisi ayrı `SearchDocument` (`SourceKey = website`, `SourceId = ContentItemId`, `Url = çözümlenmiş tam yol`). `NoIndex` işaretli içerik **aramada yer alır ama `IncludeInSitemap = false`**; `Visibility` kısıtlı/üye-özel (varsa) içerik indekslenmez.
- **Eşzamanlı güncelleme:** İçeriği değiştiren her handler (oluşturma, çeviri ekleme/güncelleme/silme, durum geçişi, görünürlük/yol değişimi, kopyalama, çöp kutusuna taşıma/geri alma, kalıcı silme, tür `IsSearchable`/aktiflik değişimi, dil pasife alma) `ISearchIndexUpdater` portunu **aynı Unit of Work içinde** çağırır; `SearchDocument` aynı `WebsiteDbContext`'te olduğundan tek transaction'dır. **Hiyerarşik görünürlük:** üst içerik görünürlüğü/yolu değişince **tüm torunlar** yeniden değerlendirilir (sınırlı toplu işlem; çok büyük ağaçta tek seferde en çok 500 içerik, kalanı Hangfire'a devredilir).
- **Zamana bağlı görünürlük:** Zamanlanmış yayın/yayından kalkma bir mutasyon olayı üretmez. Bu nedenle **uzlaştırma job'ı** `ReconcileWebsiteSearchIndexJob` (Hangfire recurring, varsayılan 10 dakika): indeksteki `website` kaynağını gerçek görünür içerik kümesiyle karşılaştırıp eksikleri ekler, fazlaları siler. Aynı mantık yönetici komutu olarak da çalışır (Görev 4).
- **Etkinlik içerikleri** (`SupportsEvent`) başlık/özet/gövde ile indekslenir; takvim bilgisi (tarih, mekân) aramaya **girmez**, sonuç satırına eklenmez (Görev 3 sonuç yalnızca başlık/özet/URL/tür).
- Kişisel veri içeren hiçbir alan (form başvurusu, kayıt, bülten) indekslenmez; mimari test: `SearchDocument` yazan kodun yalnızca `ContentItem*` ve Görev 4 adaptör tiplerine bağımlı olduğunu doğrular.

**Testler:** Görünür/görünmez durum tablosu (taslak, zamanlanmış, süresi dolmuş, arşiv, çöp, pasif tür, `IsSearchable=false`, pasif dil, görünmez ata); her mutasyon sonrası indeks tutarlılığı; üst içerik gizlenince torunların düşmesi; zamanlanmış yayında uzlaştırma job'ının eklemesi/silmesi (zaman sabitlenerek); `NoIndex` + sitemap bayrağı; transaction atomikliği (handler başarısız olunca indeks değişmez) (integration).

**Commit:** `feat(website): index searchable website content transactionally with scheduled-visibility reconciliation`

---

## Görev 3 — Public genel arama ucu

**Oku:** Görev 1–2'nin kodu; `GetPublicContentsQueryHandler` (liste içi `search` doğrulama sabitleri: 2–100 karakter); `SiteSettings.GlobalSearchEnabled`; Host'taki rate limit tanımları; SharedKernel `PagedRequest/PagedResult<T>`.

- **`GET /api/v1/public/search`**: `q` (2–100 karakter, trim), `lang` (zorunlu değil; verilmezse varsayılan dil), `type` (içerik türü anahtarı, çoklu virgülle, opsiyonel), `source` (opsiyonel), sayfalama (`PagedRequest`, varsayılan 10, maks 50).
- **`GlobalSearchEnabled = false`** → `feature_unavailable` ProblemDetails (mevcut desen). Pasif dil → `404`.
- **Eşleme:** `q` `TurkishTextNormalizer` ile normalize edilir, boşluklardan **belirteçlere** (en çok 6) bölünür; her belirteç `NormalizedText` içinde geçmelidir (**AND**). Joker karakterler (`%`, `_`, `[`) belirteç içinde **kaçışlanır** (LIKE enjeksiyonu/yanlış eşleşme yok). ADR gereği `LIKE` ile başlanır; baştaki joker indeksi kullanamayacağından, **ölçüm** için `PERFORMANCE.md`'ye "arama gecikmesi izleme" notu eklenir (Görev 8) ve full-text'e yalnızca ölçümle geçilir.
- **Sıralama:** (1) başlıkta tüm belirteçler geçenler önce, (2) `PublishedAtUtc` azalan, (3) `Id`.
- **Yanıt:** `items` (`sourceKey`, `typeKey`, `title`, `summary`, `url`, `publishedAtUtc`), sayfalama bilgisi ve `typeCounts` (tür başına toplam; filtresiz sorgu üzerinden tek gruplu sorgu). `NormalizedText` ve iç alanlar **dönmez**.
- **Koruma:** yeni `public-search` rate limit policy'si (IP başına dakikada 30; test ortamında yüksek). **Arama sorgusu metni loglanmaz ve saklanmaz** (popüler arama/analitik yoktur; kişisel veri ihtimali).
- **Sunucu tarafı cache yok** (ölçülmeden eklenmez). HTTP yanıt başlıkları için diğer public liste uçlarının mevcut kuralını aynen izle.

**Testler:** Belirteç AND eşlemesi; Türkçe karakter ve büyük/küçük harf (`İSTANBUL` ↔ `istanbul`, `ığüşöç`); joker karakter kaçışı; başlık öncelikli sıralama; `type`/`source`/dil filtreleri; sayfalama; `typeCounts`; bayrak kapalıyken `feature_unavailable`; kısa/uzun `q` doğrulaması; rate limit; yanıtta yasaklı alan olmaması; sorgu metninin log/DB'ye yazılmaması (integration).

**Commit:** `feat(website): add public global search endpoint with token matching and type counts`

---

## Görev 4 — Dış kaynak senkronu ve Employer ilan adaptörü

**Oku:** Görev 1–3'ün kodu; ADR-024 §10 (pull modeli ve gerekçesi); `src/BuildingBlocks/Contracts/Employer/IPublishedJobModuleContract.cs` ve `PublishedJobSummary`; Host kompozisyon kökü (`Program.cs`, contract kayıtları, Hangfire recurring job kayıtları); `SiteSettings.PublicJobListingsEnabled`.

- **`EmployerJobSearchSource`** (**Host projesinde**, Website ve Employer'dan bağımsız adaptör; `IExternalSearchSource`): `SourceKey = employer.job`. `IPublishedJobModuleContract`'ı sayfalayarak okur. Eşleme: `TypeKey = job`, `Title = Title`, `Summary = Summary`, `SearchableText = Title + CompanyName + ProvinceName + PositionName + EmploymentTypeName + WorkLocationTypeName + Summary`, `Url = {Website:JobsPublicPathPrefix}/{Slug}` (yapılandırma, varsayılan `/ilanlar`), `IncludeInSitemap = true`. İlanlar tek dillidir: **varsayılan site dilinde** indekslenir (varsayım; diğer dillerde ilan aramada çıkmaz).
- **`PublicJobListingsEnabled = false` iken** kaynak boş küme döner → senkron o kaynağın belgelerini siler (ilanlar aramadan ve sitemap'ten düşer).
- **`SyncExternalSearchSourcesJob`** (Hangfire recurring, varsayılan 10 dakika; yapılandırma `Website:Search:ExternalSyncIntervalMinutes`): kayıtlı her kaynak için **tam senkron** — sayfa sayfa çek (200'lük), `(SourceKey, SourceId, LanguageCode)` ile upsert et, görülen anahtar kümesini tut; **yalnızca tüm sayfalar hatasız okunduysa** görülmeyenleri sil (kısmi hatada **hiçbir şey silinmez**). Kaynaklar birbirinden **izole** çalışır (biri patlarsa diğerleri sürer). Çalışma `SearchSourceState`'e yazılır (başlangıç, başarı, kırpılmış hata, belge sayısı). Aynı kaynağın eşzamanlı iki senkronu engellenir.
- **Yönetim uçları (`Website.Settings.Manage`):**
  - `GET /api/v1/admin/website/search/sources` — kaynaklar (`website` dahil), son başarılı/başlayan zaman, belge sayısı, son hata.
  - `POST /api/v1/admin/website/search/sources/{sourceKey}/reindex` — **anında yeniden indeksleme** (dış kaynak için tam senkron, `website` için Görev 2'deki uzlaştırma). Bilinmeyen kaynak `404`; zaten çalışıyorsa `409`.
- Kaynak kayıtlı değilse (başka proje) job boş çalışır, hata vermez.

**Testler:** Adaptör eşlemesi; tam senkron (ekle/güncelle/sil); **kısmi hata → silme yok**; kaynak izolasyonu (iki sahte kaynak, biri hatalı); bayrak kapalıyken belgelerin silinmesi; eşzamanlı çalıştırma koruması; admin uçları ve yetki; durum satırı güncellemeleri; 250 ilanla sayfalı gezinme (integration, sahte contract).

**Commit:** `feat(website): sync external search sources through host adapter for published jobs`

---

## Görev 5 — Sitemap ve robots.txt

**Oku:** ADR-024 §15 (sitemap/robots, ayrılmış segmentler), §4 (`NoIndex`, `hreflang` karşılıkları); `GetPublicContentById` içindeki `hreflang` üretimi; `SiteSettings`; Faz 1b public cache + invalidation; Host'taki public endpoint kayıt deseni.

- **`GET /sitemap.xml`** (kök yol, anonim): yalnızca **görünür, `NoIndex = false`** içerikler (Görev 2'deki görünürlük çözümleyicisini yeniden kullan, ikinci kural yazma), ana sayfa, `HasListingPage` açık türlerin liste sayfaları ve **dış kaynak belgeleri (`IncludeInSitemap = true`)**. Her URL: `loc` (`Website:PublicSiteBaseUrl` + yol), `lastmod` (içerik `UpdatedAtUtc`/belge `PublishedAtUtc`), çok dilli içerikte `xhtml:link rel="alternate" hreflang` karşılıkları (+ `x-default` varsayılan dil). Etkinlikler: gelecekteki ve yakın geçmiş (son 90 gün) dahil, daha eskiler hariç.
- **Bölme:** URL sayısı 10.000'i aşarsa `/sitemap.xml` bir **sitemap index** olur, parçalar `/sitemap-{n}.xml` (her biri en çok 10.000 URL; protokol sınırı 50.000/50 MB'ın çok altında).
- **`GET /robots.txt`** (kök yol): `User-agent: *`; `Disallow: /api/`, `/admin`, `/portal`, `/webuploads/private` benzeri **özel yollar** (yapılandırmadan okunan liste: `Website:Robots:DisallowedPaths`); `Sitemap: {PublicSiteBaseUrl}/sitemap.xml`. **Bakım modunda ve `SiteSettings.AllowSearchEngineIndexing = false` iken** `Disallow: /`.
- **Yeni ayar:** `SiteSettings.AllowSearchEngineIndexing` (bool, varsayılan `true`; test/staging kurulumları için); mevcut grup bazlı `SiteSettings` ucuna eklenir. Kapalıyken sitemap boş döner ve public sayfa yanıtları `noindex` kuralına uyar (mevcut `noindex` mekanizması varsa onu kullan; yoksa yalnızca robots+sitemap'i uygula ve raporla).
- **Cache:** sitemap XML'i bellekte **1 saat**, `InvalidateAllPublic` ile boşalır; yanıt `Content-Type: application/xml; charset=utf-8`. `ETag`/`If-None-Match` desteği (varsa mevcut desen).
- **Dağıtım notu (ARCHITECTURE.md'ye, Görev 8):** API ve public site ayrı host ise `/sitemap.xml` ve `/robots.txt` isteklerinin API'ye yönlendirilmesi dağıtım yapılandırmasıdır (IIS URL Rewrite, ters proxy yok); `PublicSiteBaseUrl` doğru ayarlanmalıdır.

**Testler:** Sitemap içeriği tablo testi (görünmez, `NoIndex`, pasif dil, arşiv, süresi dolmuş etkinlik hariç); `hreflang`/`x-default`; 10.000+ URL'de index bölme (sahte veri; sınır yapılandırılabilir sabitle test edilir); dış kaynak belgelerinin yalnızca `IncludeInSitemap`'le girmesi; robots içeriği (normal, bakım modu, indeksleme kapalı); XML geçerliliği ve kaçışlama (`&`, `<`); cache invalidation (integration).

**Commit:** `feat(website): add dynamic sitemap with hreflang and robots.txt with indexing controls`

---

## Görev 6 — schema.org yapılandırılmış veri

**Oku:** ADR-024 §15 (yapılandırılmış veri), §4 (`ContentType`), §13 (`SiteSettings` iletişim/sosyal); `GetPublicContentById` yanıtı ve breadcrumb kodu; `GetPublicSite`; Faz 4 public etkinlik yanıtı; `GenclikMerkeziContentTypeSeed`.

- **Karar:** Backend JSON-LD'yi **hazır, tipli nesne olarak** döner (`jsonLd`: dizi); frontend yalnızca `<script type="application/ld+json">` içine basar. Böylece alan eşlemesi tek yerde (backend) olur. `SECURITY.md`'ye (Görev 8): frontend JSON'u basarken `<` karakterini `<` olarak kaçışlamalıdır (script kırma/XSS).
- **`ContentType.SchemaKind`** (yeni enum `ContentSchemaKind`: `None`, `Article`, `NewsArticle`, `FaqPage`; varsayılan `None`): tür yönetim uçlarına ve çeviriye dokunmayan basit alan olarak eklenir; seed: Haber, Duyuru, Basın Bülteni → `NewsArticle`; Proje, Faaliyet, Başarı Hikayesi → `Article`; SSS → `FaqPage`; diğerleri `None`. **Etkinlik** `SupportsEvent` türlerinde `SchemaKind`'den bağımsız olarak otomatik `Event` üretilir.
- **Üretim** (`StructuredDataBuilder`, saf, ayrı birim testli; her tip kendi dosyasında):
  - **`BreadcrumbList`:** mevcut breadcrumb'dan, her içerik detayında.
  - **`NewsArticle`/`Article`:** `headline` (çözümlenmiş başlık), `description` (çözümlenmiş meta açıklama), `image` (çözümlenmiş OG görseli mutlak URL), `datePublished`, `dateModified`, `mainEntityOfPage`, `publisher` (Organization referansı); `author` verisi yoksa **yazılmaz**.
  - **`Event`:** `name`, `startDate`/`endDate` (ISO 8601, UTC), `eventStatus` (`EventScheduled` / iptalse `EventCancelled`), `eventAttendanceMode` (`OfflineEventAttendanceMode` / `OnlineEventAttendanceMode` / `MixedEventAttendanceMode`), `location`: yüz yüze/hibritte `Place` (mekân adı + adres), **salt online'da `VirtualLocation` yalnızca etkinliğin kendi public sayfa URL'siyle**; **`OnlineLink` hiçbir koşulda JSON-LD'de yer almaz** (Faz 4 kuralı), `description`, `image`, `url`. Ücret metin olduğundan `offers` üretilmez.
  - **`FAQPage`:** `FaqPage` türünün **liste yanıtına** eklenir (öğeler satır içi gövdeli, `HasDetailPage=false`): her öğe `Question` (başlık) / `acceptedAnswer` (**düz metne** çevrilmiş gövde, `SearchTextBuilder`).
  - **`Organization`** (`NGO` tipi): `GET /api/v1/public/site` yanıtına `organization` olarak: site adı, `url`, `logo` (mutlak URL), `sameAs` (sosyal medya linkleri), `contactPoint` (telefon/e-posta, varsa), `address`. Boş alanlar **yazılmaz**.
- Public içerik detayına `jsonLd` eklenir (`BreadcrumbList` + türe göre `NewsArticle`/`Article`/`Event`); liste yanıtına yalnızca `FaqPage` türünde. Önizleme (`GetContentPreview`) yanıtında JSON-LD **dönmez**. Tüm URL'ler `PublicSiteBaseUrl` ile mutlak yapılır. Cache: mevcut public cache/invalidation yolunu izler (`SchemaKind` değişimi invalidator çağırır).

**Testler:** Her tip için JSON şeması/alan testi (zorunlu alanların varlığı, boş alanların atlanması); `OnlineLink`'in hiçbir çıktıda bulunmaması (regresyon testi); iptal edilmiş etkinlik; salt online etkinlikte `VirtualLocation`; FAQ düz metin; mutlak URL'ler; `SchemaKind` yönetimi ve seed; önizlemede JSON-LD olmaması; çok dilli içerikte dile göre değerler (integration).

**Commit:** `feat(website): return schema.org structured data for content, events, FAQ and organization`

---

## Görev 7 — İçerik revizyon geçmişi

**Oku:** ADR-024 §4 (`ContentItem`, çeviri, durum/zamanlama), §5 (editör akışı varsa); `ContentItem`/`ContentItemTranslation` ve içerik kaydeden tüm handler'lar (oluşturma, çeviri `PUT`, durum geçişleri, kopyalama, geri alma/çöp); `IHtmlContentSanitizer`; mevcut recurring job deseni.

**Domain/kayıt (`ContentItemRevision`, ayrı entity/aggregate değil kayıt tablosu):** `Id`, `ContentItemId`, `RevisionNumber` (içerik içinde artan), `SavedAtUtc`, `SavedByUserId`, `Kind` (`Created`, `Edited`, `Published`, `Restored`), `ChangedLanguages` (dil kodları), `IsPublishedSnapshot` (yayın anındaki revizyon), `ContentHash` (SHA-256), `SnapshotJson` (aşağıdaki **tüm diller** için anlık görüntü). Anlık görüntü **yalnızca**: her çeviri için `Title`, `Summary`, `Body` (sanitize edilmiş HTML), SEO alanları (`MetaTitle`, `MetaDescription`, `OgTitle`, `OgDescription`, `NoIndex`, `CanonicalUrl` override varsa), `TagIds`; içerik düzeyinde `CategoryIds`. **Anlık görüntüye girmeyenler:** slug/yol, durum, zamanlama, üst içerik, sıra, medya/galeri/ekler, form bağlantısı, etkinlik takvimi (yönetim ayrı uçlarından).

**Yakalama:** `IContentRevisionRecorder` portu, içerik metin/SEO/etiket/kategori değiştiren her handler'dan **aynı Unit of Work içinde** çağrılır; içerik anlık görüntüsü bir önceki revizyonun `ContentHash` değeriyle **aynıysa kayıt oluşturulmaz** (yalnızca `RowVersion`/durum değişimi gürültü üretmesin). İçerik **yayınlandığı** anda revizyon `Kind = Published`, `IsPublishedSnapshot = true` olur; yayındayken yapılan düzenlemeler normal `Edited` revizyondur. Kopyalama kopyanın ilk revizyonunu (`Created`) oluşturur. Kalıcı silme revizyonları siler; çöp kutusunda revizyonlar korunur.

**Saklama:** Günlük `PruneContentItemRevisionsJob` (Hangfire recurring; tek seferde en çok 500 içerik): içerik başına **en yeni 50** revizyon kalır, fazlası silinir; **`IsPublishedSnapshot` olan en yeni revizyon her zaman korunur** (50 sınırının dışında kalsa bile).

**Yönetim uçları (`Website.Content.Manage`):**

- `GET /api/v1/admin/website/contents/{id}/revisions` — sayfalı, yeni→eski; satırda anlık görüntü **yok**: `revisionNumber`, `savedAtUtc`, `savedByUserId`, `kind`, `changedLanguages`, `isPublishedSnapshot`.
- `GET .../revisions/{revisionNumber}` — anlık görüntü (tüm diller).
- `GET .../revisions/compare?from=&to=&lang=` — iki revizyon (veya revizyon ↔ **mevcut içerik**, `to=current`) arasında **alan bazlı** fark: her alan için `changed` bayrağı ve iki değer (metin fark algoritması **yok**, frontend gösterir).
- `POST .../revisions/{revisionNumber}/restore` (`lang`, içeriğin `RowVersion`'ı): yalnızca **o dilin** anlık görüntü alanlarını (ve istenirse kategorileri) mevcut içeriğe geri yazar; `Body` **yeniden sanitize edilir**; slug, durum, yol ve yönlendirmelere **dokunulmaz**; sonuç yeni bir `Restored` revizyondur ve Görev 2'deki indeks ile cache invalidation çalışır. Anlık görüntüde o dil yoksa `404`; dil artık pasif/çevirisi silinmişse çeviri yeniden oluşturulmaz (`409`, `Revision.TranslationNoLongerExists`).

**Testler:** Yakalama tablosu (hangi handler revizyon üretir); aynı hash'te kayıt oluşmaması; yayında `IsPublishedSnapshot`; geri yükleme (yalnızca seçilen dil, slug/durum değişmez, yeniden sanitize, `Restored` revizyon, indeks güncel); 50 sınırı ve yayınlanmış revizyonun korunması (zaman/sayı sabitlenerek, job); `compare` (revizyon↔revizyon, revizyon↔güncel); kalıcı silmede revizyonların silinmesi; yetki; `RowVersion` çakışması `409` (integration).

**Commit:** `feat(website): add content revision history with compare, restore and retention`

---

## Görev 8 — Doküman senkronizasyonu ve tam test

**Oku:** ADR-024 §2, §10, §13, §15, §18; ADR-020; `ARCHITECTURE.md` §8.11; `SECURITY.md`; `PERFORMANCE.md`; bu fazın commit'leri.

- **ADR-024:** `SearchDocument`/`SearchSourceState`, normalizer'ın SharedKernel'e taşınması, belirteç AND eşlemesi ve joker kaçışı, sıralama, `typeCounts`, sorgu metninin saklanmaması; Website içerik indekslemenin transaction içinde yapılması + zamanlama için uzlaştırma job'ı; dış kaynak tam senkron kuralları (kısmi hatada silme yok, izolasyon, bayrak kapalıyken silme, `SearchSourceState`); ilanların varsayılan dilde indekslenmesi (varsayım); sitemap kapsamı, bölme ve `hreflang`; robots ve `AllowSearchEngineIndexing`; `SchemaKind` ve JSON-LD kapsamı (`OnlineLink` yok, `JobPosting` yok); revizyon kapsamı/saklama (50, yayınlanmış korunur)/geri yükleme kuralları; §18 faz tablosunda **Faz 5 ✅**, **kapsam dışı** ve ertelenenler (ilan için `JobPosting`, popüler arama analitiği, otomatik tamamlama, full-text, `PageLayout`/`SiteSettings` revizyonu); §10'daki "indeksli `LIKE`" ifadesinin gerçeği (baştaki joker indeks kullanmaz, ölçümle izlenir).
- **ADR-020:** normalizer'ın SharedKernel'e taşındığını belirten kısa not.
- **SECURITY.md:** aramada kişisel veri olmaması ve sorgunun saklanmaması; sitemap/robots/indeksleme kontrolleri; JSON-LD'de `OnlineLink` yokluğu ve **frontend'in `<` kaçışı zorunluluğu**; revizyonlarda yalnızca editöre görünen içerik (kişisel veri yok), geri yüklemede yeniden sanitize.
- **PERFORMANCE.md:** genel arama `LIKE` yaklaşımı için gecikme izleme notu ve full-text'e geçiş ölçütü; `SearchDocument` büyüme beklentisi.
- **ARCHITECTURE.md §8.11:** Faz 5'in tamamlandığı; sitemap/robots dağıtım notu (IIS URL Rewrite, `PublicSiteBaseUrl`); **Website modülünün tüm fazlarının bittiği**; kalan işler (admin ve public frontend'ler, SSR/SEO kararı).
- **Tam test paketi:** `dotnet test GenclikMerkezi.slnx`, sayılarıyla.

**Commit:** `docs(website): sync ADR-024, ADR-020, security, performance and architecture docs with Faz 5`

**Faz sonu raporu (kısa):** görev başına commit hash'i, tam paket sayıları, tüm sapmalar ve ertelenen işler.

---

## Kapsam dışı

İlan için schema.org `JobPosting` ve ilan detay sitemap'i dışındaki Employer işleri; arama sorgusu analitiği/popüler aramalar; otomatik tamamlama ve yazım düzeltme; SQL Server full-text (ölçüm sonrası ayrı karar); `PageLayout`, `SiteSettings` ve medya revizyonları; metin fark (diff) algoritması; admin ve public frontend'leri; public site SSR/SEO render kararı.
