# ADR-019: SharedKernel'de Generic Dosya Yükleme Altyapısı

## Durum
Kabul edildi

## Bağlam

Candidate modülü, CV fotoğrafı ve CV dosyası (PDF/DOCX) yüklemesi gerektiriyor. Bu ihtiyaç Candidate'a özgü değil — ileride Employer modülü (firma logosu), Website/blog modülü (görsel içerik) gibi başka modüller de dosya yüklemesi yapacak. Bu nedenle, pagination ve caching'de izlenen yaklaşımla tutarlı olarak, dosya yükleme de SharedKernel'de genel bir soyutlama olarak kurulacak.

Şu an için hosting altyapısı (Plesk üzerinde Windows hosting) yerel disk depolamayı destekliyor; S3-uyumlu bir object storage şu an gerekli değil ama ileride gerekebilir.

## Karar

- `IFileStorageService` interface'i SharedKernel'de tanımlanır: `UploadAsync(stream, fileName, contentType, folder)`, `DeleteAsync(fileKey)`, `GetUrlAsync(fileKey)`.
- İlk implementasyon: `LocalDiskFileStorageService` — yapılandırılabilir bir kök dizine (appsettings üzerinden) yazar, dosya adlarını çakışmayı önlemek için GUID ile üretir, orijinal dosya adını meta veride saklar.
- Ortak `FileAttachment` value object/entity: `FileKey`, `OriginalFileName`, `ContentType`, `SizeInBytes`, `UploadedAtUtc`, `OwnerEntityType`, `OwnerEntityId`. Candidate modülünde `CandidateCv.Photo` ve `CandidateCvContent.CvFile` bu tipi kullanır.
- Merkezi validasyon politikası SharedKernel'de: izin verilen uzantı/content-type listesi ve max boyut, her modül kendi limitini override edebilir. İlk sürüm limitleri: CV dosyası (PDF, DOCX — max 5MB), fotoğraf (JPG, PNG — max 2MB).
- İleride S3-uyumlu bir provider'a geçiş ihtiyacı doğarsa, yalnızca `IFileStorageService`'in yeni bir implementasyonu eklenir; domain/application katmanları etkilenmez.

### Fiziksel klasör yapısı

Dosyalar diskte şu şablonla saklanır:

```
UploadedFiles/{category}/{yyyy}/{MM}/{dd}/{guid}.{ext}
```

Örnekler:
- `UploadedFiles/candidate-photos/2026/09/17/3f2a1c9e....jpg`
- `UploadedFiles/candidate-cvs/2026/09/17/8b7e2d41....pdf`
- `UploadedFiles/employer-logos/2026/09/17/a1c9f0aa....png`
- `UploadedFiles/employer-documents/2026/09/17/...`

Gerekçe:
- **Tarih partition'ı (`yyyy/MM/dd`):** tek klasörde birikecek on binlerce dosyayı önler, fiziksel yedekleme ve log-rotasyon benzeri işlemleri kolaylaştırır. Tarih **UTC** olarak hesaplanır (sunucu saat dilimi değişse bile tutarlılığı korumak için).
- **Kategori segmenti (`category`):** aynı tarihte yüklenen aday fotoğrafı, aday CV'si, firma logosu gibi farklı dosya türlerinin karışmasını önler; kategoriye özel bir retention/taşıma politikası gerektiğinde (ör. "sadece CV'leri arşivle") fiziksel olarak ayırt edilebilir olmasını sağlar. `category` değeri çağıran modül tarafından `UploadAsync` çağrısında belirtilir (`IFileStorageService.UploadAsync(stream, fileName, contentType, category)`), sabit bir enum/const listesiyle sınırlandırılır (serbest string değil) — geçersiz/yazım hatalı kategori adının rastgele klasör oluşturmasını önlemek için.
- **GUID dosya adı:** orijinal dosya adı çakışmasını ve path traversal risklerini önler; orijinal ad `FileAttachment.OriginalFileName` alanında ayrıca saklanır.

`FileKey` (DB'de saklanan referans), bu göreli yolun tamamı olur (ör. `candidate-photos/2026/09/17/3f2a1c9e....jpg`) — `GetUrlAsync` bu key'den tam URL/erişim yolunu üretir.

## Sonuçlar

**Artıları:**
- Candidate, Employer, Website gibi modüller aynı soyutlamayı yeniden kullanır, kod tekrarı olmaz.
- Storage provider değişimi (local disk → S3 vb.) yalnızca infrastructure katmanında kalır.

**Eksileri / kabul edilen trade-off'lar:**
- Yerel disk depolama, çoklu sunucu/ölçekleme senaryosunda (load balancer arkasında birden fazla instance) paylaşımlı bir disk veya senkronizasyon gerektirir — şu an tek sunucu olduğu için sorun değil, ileride not edilmesi gereken bir kısıt.
- Yerel diskte yedekleme/disaster-recovery stratejisi ayrıca ele alınmalı (bu ADR kapsamı dışında).

## İlgili Kararlar
- ADR-018: Candidate aggregate tasarımı (fotoğraf ve CV dosyası bu altyapıyı kullanır)

## Ek (ADR-024 Faz 0 Görev 4): Public ve özel depolama kökleri

**Gerekçe:** Tüm yüklemeler tek bir köke (`App_Data/uploads`) yazılıyordu ve Host bu kökü hiçbir zaman statik olarak servis etmiyordu — `GetUrlAsync`'in ürettiği URL'ler her kategori için 404 dönüyordu. Website modülünün görsel/doküman kütüphanesi (Görev 5) ve firma logosu gibi gerçekten herkese açık gösterilmesi gereken dosyalar için bu artık yetersiz; ama kökü toptan statik servise açmak, aday CV'lerini ve fotoğraflarını da açığa çıkarır. Bu nedenle dosyalar **erişim türüne göre** iki ayrı fiziksel köke yazılır. Ayrımın ölçütü "hangi modül sahibi" değil "herkese mi gösteriliyor"dur.

| Kök | Ayar | Varsayılan yol | Servis | Kategoriler |
|---|---|---|---|---|
| Public | `FileStorage:PublicRootDirectory` | `App_Data/webuploads` | Statik, `FileStorage:PublicRequestPath` (`/webuploads`) altından, `Cache-Control: public, max-age=31536000, immutable`, dizin listeleme kapalı, `X-Content-Type-Options: nosniff` | `EmployerLogo`, `WebsiteImage`, `WebsiteDocument` |
| Özel | `FileStorage:RootDirectory` | `App_Data/uploads` | **Asla statik servis edilmez** | `CandidatePhoto`, `CandidateCv`, `EmployerDocument`, `WebsiteFormAttachment` |

**Değişmeyenler:**

- `FileKey`'in formatı değişmedi: hâlâ yalnızca `{category}/{yyyy}/{MM}/{dd}/{guid}.{ext}` — hangi kökte durduğuna dair bir işaret taşımıyor. `DeleteAsync`/`ReadAsync`, kategoriyi `FileKey`'in ilk segmentinden (`FileCategoryExtensions.TryGetCategoryFromFolderSegment`) çözüp doğru kökte çalışıyor; DB'deki mevcut `FileKey` kayıtları etkilenmedi.
- `GetUrlAsync` kategoriden bağımsız kalmaya devam ediyor: her zaman `{PublicBaseUrl}/{fileKey}` döner. Yalnızca public kategoriler için bu URL artık gerçekten çözülüyor; özel kategoriler için döndürülen URL, bu Faz 0 görevinden önce olduğu gibi, hâlâ çözülmüyor.
- Aday fotoğrafı/CV'si ve firma belgeleri için yetkili indirme endpoint'leri bu görevin kapsamında değildir; Candidate/Employer için takip işi olarak not edilmiştir.