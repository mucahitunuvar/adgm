# Website Modülü — Faz 2 (Sunum: Menü, Slider, Partner, Göstergeler, Sayfa Düzeni, Pop-up) Master Prompt

## 0. Nasıl kullanılır (token tasarrufu)

Bu dosya **görev görev** uygulanır. Her görev ayrı bir Claude Code oturumunda yürütülür:

1. Kullanıcı `/clear` yapar ve şunu gönderir: *"WEBSITE-FAZ2-MASTER-PROMPT.md dosyasındaki §1'i ve Görev N'i oku, yalnızca Görev N'i uygula."*
2. Sen yalnızca **§1 (genel kurallar)** ve **ilgili görevin** bölümünü okursun. Diğer görevleri okumana gerek yok.
3. Görevin **"Oku"** listesindeki dosya ve bölümler dışında doküman okuma. AGENTS.md'yi baştan sona okuma; yalnızca listede belirtilen bölümlerine bak.
4. Önceki görevlerin kodu commit'lenmiştir; ihtiyaç duyduğun yerde `git log` ve ilgili kaynak dosyalarından öğren.
5. Görev bitince commit'le, push'la, kısa raporunu yaz ve **dur**. Bir sonraki göreve geçme.

---

## 1. Genel kurallar (her görevde geçerli)

**Kullanıcı kararları (Faz 2 için alındı):**

- Menü konumları: `header`, `utility`, `footer`. Footer tek menüdür; birinci seviye öğeleri sütun başlığıdır. **Mobil menü ayrı tutulmaz**, frontend header menüsünden üretir.
- Sayfa düzeni: **ana sayfa** ve `SupportsBlockLayout` açık içerik türleri (şu an `page`).
- `JobList` bloğu yalnızca ayar tutar; veriyi frontend Employer API'sinden çeker. Employer'daki eksikler ayrı bir işte yapılacak.
- Etki göstergeleri manuel girilir; dönem ve kaynak zorunludur.

**Mimari:**

- Tüm yeni endpoint'ler Faz 0/1'deki desenleri izler: vertical slice, `RowVersion` ile iyimser eşzamanlılık (`409`), `TimeProvider`, projection + `AsNoTracking`, Result deseni.
- Yönetim uçları `/api/v1/admin/website/...`, public uçlar `/api/v1/public/...` (anonim, `public-read` rate limit).
- **Yetkilendirme:** Bu fazdaki tüm yönetim işlemleri `Website.Design.Manage` policy'sini kullanır.
- **Çeviri kuralı:** Dile bağlı metin alanları `...Translation` child entity'lerinde durur; varsayılan dilde çeviri zorunludur. Bir öğenin istenen dilde çevirisi yoksa o öğe public yanıtta **gösterilmez** (varsayılan dile düşülmez; ADR-024 §3).
- **Görsel referansları** `MediaImageReferenceGuard` ile doğrulanır. Yeni her görsel referansı için bir `IMediaUsageProvider` eklenir (kullanımdaki medya silinemez).
- **Zengin metin** alanları handler'da `IHtmlContentSanitizer` ile temizlenir.
- **Zamanlama:** `PublishAtUtc`/`UnpublishAtUtc` taşıyan her yeni öğe için görünürlük kuralı, Faz 1b'deki `ContentItemVisibility` deseninde **tek bir expression** olarak tanımlanır; kopyalanmaz.

**Cache:**

- Bu fazdaki her mutasyon handler'ı commit **sonrasında** hem `WebsiteCacheInvalidator.InvalidatePublicContent` hem `InvalidatePublicSite`'ı çağırır (kaba taneli temizlik). Tek çağrıda ikisini yapan bir `InvalidateAllPublic` yardımcısı eklemek serbesttir.
- **Önemli:** Menüler (Görev 1) ve pop-up'lar (Görev 6) `GET /api/v1/public/site` yanıtına girecek. Bu yanıt artık içeriklere bağlı (menüdeki bir içerik yayından kalkarsa link gizlenmeli). Bu yüzden Görev 1'de **içerik, tür ve kategori mutasyonlarının da public site cache'ini temizlemesi** sağlanır.
- `PublicContentCacheInvalidationTests` mimari testi, bu fazda eklenen tüm mutasyon handler'larını kapsayacak şekilde güncellenir.

**Test komutu (görev içinde):**

```bash
dotnet test GenclikMerkezi.slnx --filter "FullyQualifiedName~Website|FullyQualifiedName~Architecture" --verbosity quiet --nologo
```

Tam paket (`dotnet test GenclikMerkezi.slnx`) yalnızca **Görev 7**'de çalıştırılır.

**Commit kuralları:** Açık dosya listesiyle stage et. `ADR-023` ve kullanıcının diğer stage'deki dosyalarını hiçbir commit'e dahil etme. Commit'ten sonra `git push` ve `git log origin/main -1` ile push'u doğrula.

**Çelişki:** Bu prompt ile kod veya ADR arasında çelişki bulursan kendin karar verme; AGENTS.md §6 formatında dur ve raporla.

**Rapor (her görev sonunda, kısa):** commit hash, eklenen migration, filtreli test sayıları, sapmalar ve varsayımlar. Uzun mimari özet yazma.

---

## Görev 1 — Link hedefi ve menüler

**Oku:** ADR-024 §7 ve §17; `Features/GetPublicSite/*`; `Application/Abstractions/WebsiteCacheKeys.cs`, `WebsiteCacheInvalidator.cs`; `Domain/ContentItemVisibility.cs`; `Domain/ContentCacheTtlCalculator.cs`; `Application/RouteResolution/` (yol üretimi için); AGENTS.md §10–§13.

### 1.1 `LinkTarget` value object (Görev 2, 5 ve 6 da kullanacak)

- Türler: `Content` (ContentItem ID), `ContentTypeListing` (ContentType ID), `InternalPath`, `ExternalUrl`.
- `InternalPath`: `/` ile başlar; `//`, `\`, şema (`:`) içeremez; maks 500 karakter. Örn. `/portal/giris`.
- `ExternalUrl`: mutlak `https`, `http`, `mailto:` veya `tel:`; maks 1000 karakter.
- Link hedefini public yanıta çeviren **tek bir Application servisi** (`LinkTargetResolver` veya işlevini anlatan bir ad): istenen dil için `href` üretir (dil önekiyle), hedef görünmüyorsa `null` döner:
  - `Content`: içerik görünür değilse (atalar dahil), türü pasifse, türün `HasDetailPage = false` ise veya o dilde çevirisi yoksa → `null`.
  - `ContentTypeListing`: tür pasifse, `HasListingPage = false` ise veya o dilde çevirisi yoksa → `null`.
- Çözümleme toplu çalışmalıdır: bir menüdeki tüm içerik hedefleri tek sorguda çözülür (N+1 yok).

### 1.2 Menü

- `Menu` aggregate: `Location` (`Header`, `Utility`, `Footer`; her konumda tek menü), `RowVersion`, audit alanları. Üç menü migration ile boş olarak seed edilir.
- `MenuItem` (child entity, ağaç): `ParentId`, `SortOrder`, `IsActive`, `LinkTarget` (opsiyonel; yoksa öğe **grup başlığıdır**), `OpenInNewTab`, `IconKey` (opsiyonel, `[a-z0-9-]`, maks 50), dile göre `Label` (maks 100).
- Kurallar: döngü yok; maksimum derinlik 3; menü başına en fazla 200 öğe; `Utility` menüsü tek seviyelidir.
- **Güncelleme tüm ağacı değiştirir:** `PUT /api/v1/admin/website/menus/{location}` — öğe ağacının tamamı + `RowVersion` (editörde sürükle-bırak tek kayıtla yapılır). Öğeler istemcide geçici ID taşıyabilir; parent ilişkisi bu ID'lerle kurulur.
- `GET /api/v1/admin/website/menus` ve `GET /api/v1/admin/website/menus/{location}` — öğelerle birlikte, her `Content`/`ContentTypeListing` hedefi için güncel durum ("yayında değil", "tür pasif" gibi) bilgisiyle; editör kırık linkleri görebilsin.
- **Kullanım koruması:** Bir içerik kalıcı silindiğinde (çöp kutusu job'ı dahil) ona bağlı menü öğelerinin hedefi kaldırılır ve öğe pasife alınır (grup başlığına dönüşmesin); bu işlem aynı transaction'da yapılır ve loglanır.

### 1.3 Public yanıt

- `GET /api/v1/public/site` yanıtına `menus` alanı eklenir: `header`, `utility`, `footer`, her biri öğe ağacı (`label`, `href` veya `null`, `openInNewTab`, `iconKey`, `children`).
- Public'te gizlenenler: pasif öğeler ve alt ağaçları; o dilde etiketi olmayan öğeler; hedefi çözümlenemeyen (`null`) link öğeleri; **hiç görünür çocuğu kalmayan grup başlıkları**.
- **Cache:** public site cache'i artık içeriklere bağlı:
  - Faz 1'deki tüm içerik, tür ve kategori mutasyonları public site cache'ini de temizler (§1).
  - Public site TTL'i, **site genelindeki en yakın zamanlama anına** göre kısaltılır: tüm içeriklerin (tür fark etmeksizin) gelecekteki en yakın `PublishAtUtc`/`UnpublishAtUtc`'si. Görev 2 ve 6 bu hesaba slide ve pop-up zamanlamalarını ekleyecek; hesabı genişletilebilir tasarla.

**Testler:** `LinkTarget` doğrulamaları (geçersiz iç yol ve şemalar); çözümleyicinin görünmez içerik, pasif tür, çevirisi olmayan dil ve atası görünmeyen içerik için `null` dönmesi; derinlik, öğe sınırı ve `Utility` tek seviye kuralı; tüm ağaç değiştirme ve concurrency; public menüde boş grup başlığının gizlenmesi; bir içerik yayından kaldırıldığında public site yanıtındaki menü linkinin kaybolması (cache temizliği uçtan uca); kalıcı silinen içeriğe bağlı menü öğesinin pasife alınması.

**Commit:** `feat(website): add link targets and location-based menus`

---

## Görev 2 — Slider

**Oku:** ADR-024 §8.2; Görev 1'de eklenen `LinkTarget` ve `LinkTargetResolver`; `Domain/ContentItemVisibility.cs`; mevcut bir `IMediaUsageProvider` örneği.

- `Slider` aggregate: `Key` (`[a-z0-9-]`, maks 50, benzersiz, değiştirilemez; örn. `home-hero`), dile göre `Name` (yönetim için), `RowVersion`, audit alanları.
- `Slide` (child entity): `DesktopImageMediaId` (zorunlu), `MobileImageMediaId` (opsiyonel), `LinkTarget` (opsiyonel), `SortOrder`, `IsActive`, `PublishAtUtc`, `UnpublishAtUtc`; dile göre `Eyebrow` (maks 100), `Title` (maks 150), `Text` (maks 400), `ButtonLabel` (maks 50), `AltTextOverride` (maks 250). Slider başına en fazla 20 slide.
- Kurallar: `ButtonLabel` varsa `LinkTarget` zorunlu; `UnpublishAtUtc > PublishAtUtc`. Slide'ın görünürlüğü tek bir expression olarak tanımlanır (`IsActive` + zaman aralığı).
- Alt metin çözümü: `AltTextOverride` → masaüstü görselinin medya kütüphanesindeki o dildeki alt metni → boş (Faz 1b galeri kuralıyla aynı).
- **Yönetim:** `GET/POST /api/v1/admin/website/sliders`, `GET/PUT .../sliders/{id}` (ad çevirileri), `PUT .../sliders/{id}/slides` (tüm slide listesini değiştirir, `RowVersion`), `DELETE .../sliders/{id}` (Görev 4'te bir sayfa düzeni kullanıyorsa `409`; bu görevde silme serbest, kontrol portu kurulur: `ISliderUsageChecker`).
- **Public:** Ayrı bir public endpoint yok; slider verisi Görev 5'te blok verisi olarak döner. Bu görevde public yanıt için kullanılacak bir sorgu servisi yazılır: bir slider'ın istenen dildeki görünür slide'ları, görsel varyant URL'leri ve çözümlenmiş link'leriyle. Link'i çözümlenemeyen slide'ın butonu gizlenir, slide gösterilir.
- **Medya kullanımı:** `SliderMediaUsageProvider`.
- **Cache:** Public site TTL hesabına slide zamanlamaları eklenir (Görev 1'deki genişletme noktası).

**Testler:** Key kuralları; slide sınırı; buton–link kuralı; zaman aralığı; görünürlük; alt metin zinciri; kullanımdaki görselin silinememesi; slide listesi değiştirme ve concurrency.

**Commit:** `feat(website): add sliders with scheduled slides`

---

## Görev 3 — Partner logoları ve etki göstergeleri

**Oku:** ADR-024 §8.2; mevcut `Video` aggregate'i (benzer basit aggregate deseni); `MediaImageReferenceGuard`.

### 3.1 Partner

- `Partner` aggregate: `LogoMediaId` (zorunlu), `WebsiteUrl` (opsiyonel, mutlak `https`), `SortOrder`, `IsActive`, `RowVersion`, audit; dile göre `Name` (maks 150), opsiyonel `Description` (maks 300).
- Yönetim: `GET` (sayfalı, `search`, `isActive`), `GET/{id}`, `POST`, `PUT/{id}`, çeviri `PUT`/`DELETE`, `activate`/`deactivate`, `DELETE`.
- Public: `GET /api/v1/public/partners?lang=` — aktif partnerler, `SortOrder` sıralı, logo varyant URL'leriyle. (İş birlikleri sayfası için; logo şeridi bloğu da Görev 5'te aynı sorgu servisini kullanır.)
- `PartnerMediaUsageProvider`.

### 3.2 Etki göstergesi (`ImpactMetric`)

- Aggregate: `Value` (decimal, ≥ 0, en fazla 2 ondalık), `IconKey` (opsiyonel), `SortOrder`, `IsActive`, `RowVersion`, audit; dile göre `Label` (maks 100), `Unit` (opsiyonel, maks 30; örn. "genç", "%"), `Period` (maks 100; örn. "2026 1. yarı"), `Source` (maks 200; örn. "Merkez iç kayıtları").
- **Kural:** Bir gösterge aktif edilebilmesi için varsayılan dilde `Period` ve `Source` dolu olmalıdır (doğrulanmamış rakam yayınlanmaz). Aktifken bu alanlar boşaltılamaz.
- Yönetim uçları partnerle aynı şekilde.
- Public: ayrı endpoint yok; Görev 5'te blok verisi olarak döner. Sorgu servisi bu görevde yazılır.

**Testler:** URL ve değer doğrulamaları; aktivasyon kuralı (dönem/kaynak yokken reddedilmesi); public partner listesi (pasif ve çevirisiz olanlar görünmez); kullanımdaki logonun silinememesi.

**Commit:** `feat(website): add partners and impact metrics`

---

## Görev 4 — Blok kataloğu ve sayfa düzeni (yönetim tarafı)

**Oku:** ADR-024 §8.1; Görev 1–3'te eklenen aggregate'ler ve sorgu servisleri; `ContentType` bayrakları; çöp kutusu kalıcı silme servisi (`ContentItemPermanentDeletionService`); AGENTS.md §10, §47.

### 4.1 Blok tipi kaydı

- Blok tipleri **kodla tanımlanır**, veritabanında tutulmaz. Her blok tipi tek bir yerde tanımlanır ve şunları içerir:
  - `Key` (sabit, örn. `hero-slider`)
  - dilden bağımsız ayarlar için tipli bir C# kaydı (`Settings`)
  - dile bağlı metinler için tipli bir C# kaydı (`Texts`), metin yoksa boş
  - bu iki kaydın doğrulayıcısı
  - bloğun kullanılabileceği hedefler (`Home`, `Content`)
  - referans verdiği varlıkların listesini çıkaran bir metot (medya, video, slider, içerik türü, kategori, gösterge, içerik ID'leri) — kullanım kontrolü ve referans doğrulaması için
- Ayarlar ve metinler veritabanında JSON olarak saklanır. Deserialize ederken **bilinmeyen alanlar reddedilir** (`System.Text.Json`, `UnmappedMemberHandling.Disallow`); tip uyuşmazlığı `400` döner.
- Zengin metin alanları sanitize edilir.
- `GET /api/v1/admin/website/block-types` — her blok tipi için anahtar, kullanılabildiği hedefler ve alan açıklamaları (ad, tip, zorunlu mu, dile bağlı mı, sınırlar). Bu bilgi C# kayıtlarından **tek kaynaktan** üretilmeli; elle ikinci bir şema yazılmamalı. Yöntemi sen seç ve raporda gerekçelendir.

### 4.2 Başlangıç kataloğu

Tüm link alanları `LinkTarget`'tır. "(dil)" işaretli alanlar `Texts` kaydına aittir.

| Key | Ayarlar | Metinler (dil) |
|---|---|---|
| `hero-slider` | `sliderId` | — |
| `logo-strip` | `maxItems` (1–30) | `title` (ops.) |
| `quick-links` | `items[]` (1–8): `iconKey`, `link` | `items[]`: `label`, `description` (ops.) |
| `content-list` | `contentTypeKey`, `count` (1–12), `featuredOnly`, `categoryId` (ops.), `view` (`cards`/`list`) | `title`, `moreLabel` (ops.) |
| `upcoming-events` | `contentTypeKeys[]` (`SupportsEvent` türler), `count` (1–12) | `title`, `moreLabel` (ops.) |
| `job-list` | `count` (1–12) | `title`, `moreLabel` (ops.) |
| `feature-mosaic` | `items[]` (1–5): `imageMediaId` (ops.), `link` | `items[]`: `eyebrow` (ops.), `title` |
| `process-steps` | `stepCount` (2–8) | `title`, `steps[]`: `title`, `text` |
| `video-feature` | `videoId` | `eyebrow` (ops.), `title`, `text` (ops.), `link` + `linkLabel` (ops.) |
| `impact-stats` | `metricIds[]` (boşsa tüm aktifler, en fazla 8) | `title` (ops.) |
| `cta` | `buttons[]` (1–3): `link`, `style` (`primary`/`secondary`/`quiet`) | `eyebrow` (ops.), `title`, `buttons[]`: `label` |
| `rich-text` | — | `body` (sanitize) |
| `image-text` | `imageMediaId`, `imagePosition` (`left`/`right`), `link` (ops.) | `title`, `body` (sanitize), `linkLabel` (ops.) |
| `faq` | `contentTypeKey` (varsayılan `faq`), `categoryId` (ops.), `count` (1–30) | `title` (ops.) |
| `gallery` | `mediaIds[]` (1–30) | `title` (ops.) |

Ek kurallar: dizi uzunlukları dilden bağımsız ayarlar ile dile bağlı metinler arasında eşleşmelidir (örn. `quick-links` öğe sayısı); `content-list`'teki kategori o türe ait olmalı; `upcoming-events` türleri `SupportsEvent` olmalı; `process-steps`'te `steps` sayısı `stepCount`'a eşit olmalı. `job-list` yalnızca `Home` hedefinde kullanılabilir. Diğerleri her iki hedefte.

### 4.3 Sayfa düzeni (`PageLayout`)

- Aggregate: `TargetKind` (`Home` / `Content`), `ContentItemId` (Content için), `RowVersion`, audit; iki blok listesi: **taslak** ve **yayındaki**; `PublishedAtUtc`, `PublishedByUserId`, `HasUnpublishedChanges`.
- `LayoutBlock` (child entity): `BlockTypeKey`, `SortOrder`, `IsActive`, `SettingsJson`, dile göre `TextsJson`. Düzen başına en fazla 30 blok.
- Kurallar:
  - `Home` için tek bir düzen vardır (migration ile boş seed edilir).
  - `Content` hedefi yalnızca `SupportsBlockLayout` türündeki, çöp kutusunda olmayan içerik olabilir; içerik başına tek düzen.
  - Blok kaydedilirken: blok tipi tanımlı olmalı, hedefte kullanılabilir olmalı, ayarlar ve **varsayılan dil** metinleri geçerli olmalı, referans verilen tüm varlıklar var olmalı (türleri de doğru olmalı; örn. `imageMediaId` bir `Image`).
  - Diğer dillerin metinleri opsiyoneldir; varsa geçerli olmalıdır.
- **Uçlar:**
  - `GET /api/v1/admin/website/layouts/home` ve `GET /api/v1/admin/website/layouts/content/{contentItemId}` — taslak ve yayındaki blokların listesi, `HasUnpublishedChanges`, `RowVersion`. İçerik için düzen yoksa boş taslak döner (oluşturulmamış sayılır).
  - `PUT .../layouts/home/draft` ve `PUT .../layouts/content/{contentItemId}/draft` — taslak blok listesinin tamamı (`RowVersion`). İçerik düzeni ilk kayıtta oluşur.
  - `POST .../publish` — taslağı yayındaki listeye kopyalar (bu anda referans doğrulaması tekrar yapılır).
  - `POST .../discard-draft` — taslağı yayındaki haline döndürür.
- **Kullanım koruması:** Blokların (taslak **ve** yayındaki) referans verdiği medya, video, slider ve göstergeler silinemez. Bunun için `LayoutMediaUsageProvider`, Görev 2'deki `ISliderUsageChecker`'ın gerçek implementasyonu ve Faz 1b'deki `IVideoUsageChecker`'a düzen kaynağı eklenir. Kullanım açıklaması düzenin hedefini ve blok türünü belirtir.
- İçerik türü veya kategori silinemediği / pasife alınabildiği için onlara referans blokta kalabilir; public tarafta (Görev 5) bu durum bloğun gizlenmesiyle ele alınır.
- İçerik kalıcı silindiğinde düzeni de aynı transaction'da silinir.

**Testler:** Her blok tipi için geçerli bir örnek ve en az bir geçersiz örnek (tablo testi); bilinmeyen JSON alanının ve tanımsız blok tipinin reddi; hedef kısıtı (`job-list` içerikte); dizi uzunluğu eşleşmesi; referans doğrulaması; taslak → yayın → taslakta değişiklik → `HasUnpublishedChanges` → discard akışı; concurrency; kullanımdaki medya/video/slider/göstergenin silinememesi; `block-types` uç noktasının her tip için alan açıklamalarını döndürmesi; içerik kalıcı silinince düzenin silinmesi.

**Commit:** `feat(website): add block type registry and draft/published page layouts`

---

## Görev 5 — Public sayfa düzeni ve blok verisi

**Oku:** Görev 4'ün kodu; Faz 1b'deki `GetPublicContents` ve `GetPublicContentById` handler'ları (liste sorgusu, SEO çözümü, cache ve TTL); Görev 1–3'teki public sorgu servisleri.

### 5.1 Uçlar

- `GET /api/v1/public/home?lang=` — ana sayfanın **yayındaki** blokları + site varsayılan SEO'su (`ContentSeoResolver` ile; ana sayfa için başlık site adıdır).
- `GET /api/v1/public/contents/{id}` (Faz 1b) yanıtına, türü `SupportsBlockLayout` ise ve yayındaki düzeni varsa `blocks` alanı eklenir. Faz 1b'deki içerik önizleme uç noktası **taslak** blokları döner (editör kaydetmeden önce düzeni görebilsin).
- Ana sayfa düzeninin taslağını görmek için: `GET /api/v1/admin/website/layouts/home/preview?lang=` (`Website.Design.Manage`) — public yanıtla aynı biçimde, taslak bloklarla; cache'lenmez.

### 5.2 Blok yanıtı

Her blok: `type`, `settings` (yalnızca frontend'e gereken alanlar; iç ID'ler yerine çözümlenmiş veri), `texts` (istenen dil), `data` (çözümlenmiş veri).

| Blok | `data` |
|---|---|
| `hero-slider` | Görev 2'nin sorgu servisiyle görünür slide'lar |
| `logo-strip` | aktif partnerler (`maxItems` kadar) |
| `quick-links`, `feature-mosaic`, `cta`, `video-feature`, `image-text` | çözümlenmiş link'ler (`LinkTargetResolver`), görsel varyant URL'leri, video `embedUrl` ve kapak |
| `content-list` | Faz 1b liste sorgusuyla, türün sıralama moduna göre ilk `count` öğe (öğe biçimi public liste öğesiyle aynı) + türün liste sayfası yolu (`moreHref`) |
| `upcoming-events` | seçilen türlerden görünür içerikler; Faz 4'e kadar etkin yayın tarihine göre azalan (Faz 1b'deki `EventDateAsc` geçici davranışı) |
| `job-list` | yalnızca ayarlar; `data` boş |
| `impact-stats` | aktif göstergeler (değer, birim, dönem, kaynak) |
| `faq` | seçilen tür/kategoriden görünür içerikler (başlık ve gövde) |
| `gallery` | görseller (alt metin medya kütüphanesinden) |

**Gizleme kuralları (hata değil, bloğun yanıttan çıkarılması):**

- Blok pasifse.
- Bloğun istenen dilde metni yoksa (metin kaydı olan tiplerde; `rich-text` ve metinsiz tipler dahil).
- Zorunlu referans artık yoksa veya kullanılamıyorsa: slider silinmiş ya da hiç görünür slide'ı yok; `content-list`/`faq` türü pasif; video pasif ya da o dilde çevirisi yok; `image-text` görseli silinmiş (medya silme korumalı olduğu için bu yalnızca veri tutarsızlığında olur).
- `data` listesi boş kalan listeleme blokları (`content-list`, `upcoming-events`, `logo-strip`, `impact-stats`, `faq`, `gallery`).
- Çözümlenemeyen link'ler: tekil link'lerde ilgili buton/öğe gizlenir; `quick-links`, `feature-mosaic` ve `cta`'da link'i çözümlenemeyen öğe çıkarılır, öğe kalmazsa blok gizlenir.

### 5.3 Performans ve cache

- Bir düzenin tüm blokları için referanslar **toplu** çözülür: tüm medya ID'leri tek sorguda, tüm içerik link'leri tek sorguda vb. Blok başına ayrı sorgu (N+1) yapılmaz. Liste tipi bloklar (`content-list`, `upcoming-events`, `faq`) kendi sorgularını çalıştırır; bu kabul edilebilir.
- `public/home` yanıtı ve `blocks` içeren içerik detayı cache'lenir. TTL, düzendeki blokların bağlı olduğu tüm zamanlamaların (içerikler, slide'lar) en yakın gelecek anına göre kısaltılır. Pratikte Görev 1'deki **site geneli en yakın zamanlama** hesabını kullanmak yeterlidir.
- Invalidation §1'deki kaba taneli kurala göre.

**Testler:** Her blok tipinin `data` çözümü; tüm gizleme kuralları; taslak ile yayındaki ayrımı (taslaktaki değişiklik public'te görünmez, önizlemede görünür); içerik detayında `blocks` alanı; bir slide'ın yayın zamanı geldiğinde (zaman sabitlenemiyorsa TTL hesabının birim testiyle) public ana sayfaya girmesi; toplu çözümlemenin sorgu sayısını sınırladığı (EF Core'un loglanan komut sayısı veya bir sayaç interceptor'ı ile; yöntemi sen seç).

**Commit:** `feat(website): add public home page and resolved block data`

---

## Görev 6 — Pop-up ve duyuru şeridi

**Oku:** ADR-024 §9; Görev 1'deki `LinkTarget`, public site yanıtı ve TTL genişletme noktası; `ContentItemVisibility`.

- `Popup` aggregate: `DisplayMode` (`Modal` / `Banner`), `ImageMediaId` (opsiyonel; yalnızca `Modal`), `LinkTarget` (opsiyonel), `Targeting` (aşağıda), `DeviceTarget` (`All`/`Desktop`/`Mobile`), `PublishAtUtc`, `UnpublishAtUtc`, `IsActive`, `DelaySeconds` (0–60; `Banner`'da 0), `Frequency` (`EveryVisit` / `OncePerSession` / `EveryNDays`) + `FrequencyDays` (1–365, yalnızca `EveryNDays`), `Dismissible` (yalnızca `Banner`; `Modal` her zaman kapatılabilir), `Priority` (0–100), `RowVersion`, audit.
- Dile göre: `Title` (maks 150; `Banner`'da opsiyonel), `Body` (sanitize; `Banner`'da maks 300 karakter düz metin, HTML yok), `ButtonLabel` (maks 50).
- **Hedefleme:** `AllPages`, `HomeOnly`, `Contents` (ContentItem ID listesi, en fazla 50), `Paths` (en fazla 20 yol; her biri tam yol ya da `/*` ile biten önek, örn. `/haberler/*`; dil öneki içermez).
- Kurallar: `ButtonLabel` varsa `LinkTarget` zorunlu; zaman aralığı; aynı anda en fazla 20 aktif ve süresi dolmamış pop-up. Görünürlük tek expression.
- **Yönetim:** `GET` (sayfalı, `displayMode`, `isActive`), `GET/{id}`, `POST`, `PUT/{id}` (`RowVersion`), çeviri `PUT`/`DELETE`, `activate`/`deactivate`, `DELETE`.
- **Public:** `GET /api/v1/public/site` yanıtına `popups` alanı eklenir: o anda görünür ve istenen dilde çevirisi olan tüm pop-up ve şeritler, `Priority` azalan sıralı; her biri hedefleme bilgisiyle (`Contents` hedeflemesi, frontend eşleştirebilsin diye istenen dildeki yollara çevrilir; görünmeyen içerikler listeden çıkarılır). **Hangi pop-up'ın gösterileceğine frontend karar verir**: sayfa yoluna uyan en yüksek öncelikli bir `Modal` ve bir `Banner`. Bu kural `ARCHITECTURE.md`'ye yazılır (Görev 7).
- `PopupMediaUsageProvider`. Public site TTL hesabına pop-up zamanlamaları eklenir.

**Testler:** Mod bazlı kurallar (Banner'da görsel ve gecikme olmaması, düz metin gövde); sıklık alanları; hedefleme doğrulaması (yol biçimi, `/*` kuralı, dil öneki reddi, sınırlar); aktif pop-up sınırı; public yanıtta zamanlama, dil ve `Contents` yollarının çözümlenmesi; kullanımdaki görselin silinememesi.

**Commit:** `feat(website): add scheduled pop-ups and announcement banners`

---

## Görev 7 — Doküman senkronizasyonu ve tam test

**Oku:** ADR-024 §7, §8, §9, §17, §18; `ARCHITECTURE.md` §8.11; bu fazın commit'leri (`git log`).

- **ADR-024:** Faz 2'de netleşen kararlar — menü konumları ve mobil menünün header'dan üretilmesi; `LinkTarget` ve çözümleme kuralları (görünmeyen hedeflerin gizlenmesi, boş grup başlıkları); içerik kalıcı silindiğinde menü öğelerinin pasife alınması; slide alanları ve alt metin zinciri; gösterge aktivasyon kuralı; blok kataloğu tablosu (§4.2), JSON'da bilinmeyen alanların reddi, taslak/yayın akışı, bloğu gizleme kuralları; pop-up alanları, hedefleme biçimi, gösterim kararının frontend'de olması; public site cache'inin artık içerik mutasyonlarıyla da temizlenmesi ve site geneli TTL kısaltması.
- **ARCHITECTURE.md §8.11:** Faz 2'nin tamamlandığı; public site ve ana sayfa yanıtlarının içeriği; Faz 3'ün kapsamı.
- **Tam test paketi:** `dotnet test GenclikMerkezi.slnx` çalıştır, sayıları raporla.

**Commit:** `docs(website): sync ADR-024 and architecture docs with Faz 2`

**Faz sonu raporu (kısa):** Görev başına commit hash'i, tam paket sayıları, fazdaki tüm sapmalar ve ertelenen işler.

---

## Kapsam dışı

Yasal metinler, form motoru ve içerik–form bağlantısı, bülten, script/çerez yönetimi (Faz 3); etkinlik takvimi ve kaydı (Faz 4); arama, sitemap/robots, schema.org, revizyon geçmişi (Faz 5); Employer ilan API'sindeki eksikler; admin ve public frontend'leri; göstergelerin diğer modüllerden otomatik beslenmesi.
