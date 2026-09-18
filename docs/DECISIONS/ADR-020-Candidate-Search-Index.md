# ADR-020: Candidate Listeleme, Filtreleme ve Arama — CandidateSearchIndex Read-Model'i

## Durum
Kabul edildi

## Bağlam

Admin ve Advisor kullanıcılarının aday havuzunu listeleyip filtreleyebilmesi gerekiyor. Öncelikli filtreler: İl/İlçe, Eğitim Durumu, Sektör/İş Alanı, Tamamlanma Yüzdesi (ör. %80 üzeri), Ad/Soyad/Email serbest arama.

Bu filtrelerin bir kısmı (İl/İlçe, Ad/Soyad/Email) doğrudan `CandidateCv` aggregate'inde yaşıyor. Ama Eğitim Durumu ve Sektör/İş Alanı, `CandidateCvContent`'in koleksiyonlarında (Deneyim, Eğitim) bulunuyor — bir aday birden fazla deneyim/eğitim kaydına sahip olabilir. Bu filtreleri her liste isteğinde `CandidateCvContent` ve alt koleksiyonlarını join'leyerek sorgulamak, liste ekranının performansını (ADR-018'de bilinçli olarak `CandidateCv`'yi hafif tutma kararıyla çelişecek şekilde) düşürür.

Görev 6'da (Profil Tamamlanma Yüzdesi) kurulan mekanizma — `CandidateCv`/`CandidateCvContent` güncellendiğinde modül-içi senkron MediatR dispatch ile bir read-model'in güncellenmesi (bkz. ADR-018 revizyonu, Uygulama Notu: CAP'in process başına tek `DbContext`'e sabitlenmesi nedeniyle Outbox/RabbitMQ yerine senkron dispatch) — burada da aynı şekilde genişletilebilir.

## Karar

`CandidateSearchIndex` adında, `CandidateCv.Id`'ye 1-1 bağlı, tamamen denormalize bir read-model tablosu eklenir:

| Alan | Tip | Kaynak |
|---|---|---|
| CandidateCvId (`Id`) | Guid (PK) | — |
| FirstName / LastName | string | CandidateCv.Ad / Soyad (orijinal büyük/küçük harfle — görüntüleme için) |
| FullNameNormalized | string | CandidateCv.Ad + Soyad (arama için normalize edilmiş — büyük/küçük harf ve Türkçe karakter duyarsız) |
| Email | string | CandidateCv.Email |
| ProvinceId | Guid? | CandidateCv.İl |
| DistrictId | Guid? | CandidateCv.İlçe |
| CompletionPercentage | int | CandidateCv.CompletionPercentage |
| EducationLevelIds | Guid[] (junction tablo) | CandidateCvContent.Eğitim koleksiyonundaki distinct EğitimDurumu id'leri |
| SectorIds | Guid[] (junction tablo) | CandidateCvContent.Deneyim koleksiyonundaki distinct Sektör id'leri |
| UpdatedAtUtc | DateTime | Son senkronizasyon zamanı |

**Uygulama notu (2026-09-18):** Bu tablodaki tipler, taslak yazıldığındaki varsayımı değil (`int`/`int?`), projenin baştan beri kullandığı Guid tabanlı ReferenceData lookup id konvansiyonunu (bkz. DOMAIN.md §29.1) yansıtacak şekilde düzeltildi. `FirstName`/`LastName` taslakta yoktu — Görev 2 sırasında eklendi: `FullNameNormalized` (büyük harf, Türkçe karakter dönüştürülmüş) görüntülemeye uygun değil, listeleme ekranının orijinal büyük/küçük harfli bir ada ihtiyacı var. `EducationLevelIds`/`SectorIds` için **junction tablo** seçildi (JSON sütunu değil): bu iki alan Görev 2'nin filtrelerinde (`educationLevelId`, `sectorId`) kullanılıyor ve junction tablo, projenin zaten kullandığı sıradan `HasMany/WithOne` ilişki kalıbıyla (bkz. ADR-018'deki Experience/SocialMediaLink) tutarlı, güvenilir şekilde SQL'e çevrilen LINQ (`.Any(...)`) sağlıyor — JSON sütunu tabanlı bir yaklaşımın sorgu çevirisi bu EF Core sürümünde/sağlayıcısında doğrulanmadığı için tercih edilmedi.

- `CandidateCv` ve `CandidateCvContent`'in mevcut domain event'leri (`CandidateCvUpdated`, `CandidateCvContentUpdated` — Görev 6'da eklendi) aynı senkron dispatch handler zincirine bu read-model'i de güncelleyen bir adım eklenir. Tamamlanma yüzdesiyle aynı transaction/işlem akışında güncellenir, ayrı bir event türüne gerek yok.
- Liste endpoint'i (`GET /api/v1/candidates`) yalnızca `CandidateSearchIndex` tablosuna sorgu atar, `CandidateCv`/`CandidateCvContent`'e join yapmaz. Sonuç satırından `CandidateCvId` alınıp, detay ekranı ayrı bir çağrıyla (`GET /api/v1/candidates/{id}`) getirilir.
- Mevcut SharedKernel `PagedRequest/PagedResult<T>` bu endpoint'te kullanılır (ADR-011/pagination kararıyla tutarlı).
- Filtre parametreleri: `provinceId`, `districtId`, `educationLevelId`, `sectorId`, `minCompletionPercentage`, `searchText` (Ad/Soyad/Email serbest arama, `FullNameNormalized`/`Email` üzerinde `LIKE`/`Contains`; arama metni karşılaştırmadan önce `FullNameNormalized` ile aynı normalize fonksiyonundan geçirilir).
- **Yetkilendirme:** Bu endpoint yalnızca Admin ve CareerAdvisor rolleri içindir (literal rol string'leri — Identity.Domain.UserRole'a bağımlılık yok, bkz. ADR-016 pattern'i), adayın kendisi kullanamaz. CareerAdvisor ataması henüz kurulmadığı için (tam CareerAdvisor modülü bekleniyor — atama mekanizması bu ADR kapsamında şimdiden kurulmuyor), CareerAdvisor şimdilik Admin ile aynı kapsamda (tüm adaylar) sonuç görür.

  **İleriye dönük not (CareerAdvisor modülü tasarımı için):** Aday-advisor ataması, Admin'in manuel ataması yerine **otomatik dağıtım (load-balancing)** ile yapılacak — sistemdeki aktif advisor sayısına göre aday ve firma havuzu advisor'lar arasında otomatik dengeli dağıtılacak (ör. round-robin veya en az yüklü advisor'a atama). Bu, CareerAdvisor modülü kurulurken ayrı bir ADR'de (atama algoritması, yeniden dengeleme kuralları, advisor işten ayrılırsa/pasife alınırsa havuzunun nasıl yeniden dağıtılacağı) ele alınacak. Bu ADR'nin (ADR-020) kapsamı, o modül tamamlanana kadar Advisor'ın "tüm adaylar" görünümüyle sınırlı kalır; `CareerAdvisorId` alanı bu ADR'de kullanılmaz/set edilmez.

## Sonuçlar

**Artıları:**
- Liste sorgusu tek, dar bir tabloya bakar; `CandidateCvContent`'in ağır koleksiyonları hiç yüklenmez.
- Aynı senkron dispatch mekanizması yeniden kullanıldığı için yeni bir altyapı paterni eklenmiyor.

**Eksileri / kabul edilen trade-off'lar:**
- Denormalize veri, `CandidateCv`/`CandidateCvContent`'teki güncel veriyle *aynı transaction içinde* senkron güncellendiği için (Outbox/RabbitMQ değil) eventual consistency riski yok — ama bu, yazma işlemlerine ekstra bir güncelleme adımı ekliyor, yazma tarafında hafif bir performans maliyeti var.
- `EducationLevelIds`/`SectorIds` için junction tablo kullanımı, iki ek tablo (`CandidateSearchIndexEducationLevel`, `CandidateSearchIndexSector`) ve bunlara karşılık gelen `INSERT`/`DELETE` demek — tek bir JSON sütununa göre biraz daha fazla yazma maliyeti, ama karşılığında güvenilir filtre sorguları.

**Uygulama notu (2026-09-18):** Taslakta belirtilmeyen bir nokta: liste yanıtının "Ad Soyad (görüntüleme için), Email, İl/İlçe adı, CompletionPercentage" göstermesi gerekiyordu — İl/İlçe **adı** (id değil). `CandidateSearchIndex` yalnızca id tutar (ReferenceData verisini kopyalamaz, bkz. DOMAIN.md §29.1), bu yüzden liste endpoint'i, sayfadaki distinct Province/District id'lerini `IReferenceDataLookupReader`'a eklenen yeni bir `GetByIdsAsync(type, ids)` metoduyla (ADR-016 Decision 2 kapsamında) toplu olarak çözümler. Mevcut `ListAsync`'in sayfa boyutu sınırı (max 100) İlçe'nin ~975 satırını kapsayamadığı için bu yeni metot gerekti.

## İlgili Kararlar
- ADR-016: Cross-module erişim pattern'i (`IReferenceDataLookupReader.GetByIdsAsync` bu ADR kapsamında eklendi)
- ADR-018: Candidate aggregate tasarımı ve completion percentage senkron dispatch mekanizması
- Pagination kararı (SharedKernel `PagedRequest/PagedResult<T>`)
