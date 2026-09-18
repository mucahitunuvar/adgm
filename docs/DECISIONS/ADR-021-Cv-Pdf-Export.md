# ADR-021: CV PDF Export — QuestPDF ile Sabit Tasarım Şablonu

## Durum
Kabul edildi

## Bağlam

Adayların CV'lerini (CandidateCv + CandidateCvContent) PDF olarak dışa aktarabilmesi gerekiyor — hem adayın kendi kullanımı için hem de firmalara/advisor'lara paylaşım amacıyla. Başlangıçta tek, sabit bir tasarım yeterli (aday tercih edebileceği çoklu template ileride eklenebilir).

Kütüphane değerlendirmesi: wkhtmltopdf gibi harici binary bağımlılıkları veya PuppeteerSharp gibi headless-Chrome tabanlı çözümler, Plesk/Windows hosting ortamında ekstra süreç yönetimi ve kaynak yükü getiriyor. **QuestPDF** tamamen .NET native, harici process gerektirmiyor, fluent C# API ile bileşen bazlı sayfa tasarımı sağlıyor. Lisans: Community License, yıllık geliri $1M altında olan özel şirketler için ücretsiz (kamu kurumu/halka açık şirket değilse) — proje bu kritere uyuyor, ücretsiz kullanılabilir.

## Karar

- NuGet paketi: `QuestPDF` (Community License).
- `Candidate.Infrastructure`'da (`Infrastructure/Pdf/`) `CvPdfDocument : IDocument` sınıfı — QuestPDF'in `IDocument` arayüzünü implemente eder.
  **Uygulama notu (2026-09-18):** Taslak "CandidateCv + CandidateCvContent DTO'larını girdi alır" diyordu; gerçekte `CvPdfDocument` bu iki aggregate'i hiç görmez. Girdisi, her lookup id'sinin (Sektör, Eğitim Durumu, Dil, Referans Tipi, İl/İlçe, Uyruk, Askerlik Durumu, Engellilik Kategorisi, vb. — ~13 farklı ReferenceData tipi) zaten görüntüleme adına çözümlendiği ayrı bir DTO ağacıdır (`CandidateCvPdfModel`, `Candidate.Application.Abstractions`). Bu ayrım katman kuralı gereği zorunluydu: `CvPdfDocument` (Infrastructure) doğrudan `IReferenceDataLookupReader`'ı çağıramaz — bu orkestrasyon `ExportCandidateCvPdfQuery` handler'ında (Features/ExportCandidateCvPdf) yapılır, lookup id'leri tipe göre gruplanıp `IReferenceDataLookupReader.GetByIdsAsync` ile (bkz. ADR-020) paralel/toplu olarak çözümlenir, sonra `CandidateCvPdfModelBuilder` (saf, I/O'suz bir mapper) domain aggregate'leri + çözümlenmiş isim sözlüğünü modele dönüştürür. `ICandidateCvPdfExportService` (Application'da tanımlı arayüz), handler'ın `CvPdfDocument`/QuestPDF'e doğrudan bağımlı olmasını engeller (AGENTS.md §7).
- Sayfa yapısı, Candidate.md'deki bölüm sırasını izler:
  1. Header: Fotoğraf (varsa; IFileStorageService'ten okunur), Ad Soyad, Ünvan, İletişim (Email, Telefon, Adres, İl/İlçe), Sosyal medya linkleri
  2. Kişisel Bilgiler (Doğum Tarihi, Uyruk, Askerlik Durumu vb. — dolu olanlar gösterilir)
  3. Özet
  4. Deneyim (kronolojik, en yeni üstte)
  5. Eğitim (kronolojik, en yeni üstte)
  6. Bilgisayar Bilgisi
  7. Diller
  8. Sertifikalar
  9. Referanslar
  10. Hobiler
- **Boş bölüm kuralı:** Bir bölümün tüm alanları boşsa (ör. hiç sertifika girilmemişse) o bölüm PDF'te hiç görünmez — sabit boş başlıklar olmaz.
- **Net Maaş Beklentisi PDF'e dahil edilmez** — bu sistem içi (Admin/Advisor) değerlendirme amaçlı, dışa paylaşılan bir dokümanda yer almamalı.
- **Engellilik bilgisi PDF'e dahil edilir** — dolu ise Kişisel Bilgiler bölümünde gösterilir (aday, ayrıcalıklı istihdam olanaklarından faydalanmak için bu bilgiyi CV'sinde paylaşmak istiyor; Candidate.md'deki orijinal amaç bu).
- Üretim **on-demand**'dır (istek anında oluşturulur, önceden üretilip cache'lenmez) — CV içeriği sık değiştiği için cache invalidation karmaşıklığından şimdilik kaçınılıyor. İleride performans sorun olursa, SharedKernel caching altyapısı (mevcut) kullanılarak `CandidateCvContentUpdated` event'inde invalidation eklenebilir.
- Endpoint: `GET /api/v1/candidates/{id}/cv/export` — `Content-Type: application/pdf`, dosya adı `{Ad}_{Soyad}_CV.pdf` formatında. Yetkilendirme: adayın kendisi (kendi CV'si), Admin, CareerAdvisor.
  **Uygulama notu (2026-09-18):** Bu, projede resource-ownership ile role-based erişimin **birlikte** gerektiği ilk endpoint. `ICurrentUserContext` (SharedKernel) bilinçli olarak minimal tutulan bir soyutlama (ADR-017 Decision 2, yalnızca `UserId`) — rol bilgisi taşıyacak şekilde genişletilmedi, çünkü bu onun kapsamını genel cross-cutting "hangi kullanıcı" bilgisinden rol-tabanlı yetkilendirmeye kaydırırdı. Bunun yerine rol kontrolü (`ClaimsPrincipal.IsInRole`) endpoint'te (transport-level concern, AGENTS.md §20) yapılır ve sonucu (`CallerIsPrivileged: bool`) query'ye düz bir alan olarak geçilir; handler yalnızca bu bool'u ownership kontrolüyle birleştirir, hiçbir zaman claim/rol okumaz.
- Fotoğraf gömme, `IFileStorageService`'e eklenen yeni bir `ReadAsync(fileKey)` metoduyla yapılır (ADR-019'un genişletilmesi) — `GetUrlAsync` yalnızca tarayıcının çekebileceği bir URL üretir, PDF'in gömebileceği byte'ları değil. Dosya yoksa `null` döner (silinmiş/eksik fotoğraf `DeleteAsync` ile aynı "hata değil" semantiğini taşır), `Result` sarmalayıcısı kullanılmaz.
- **Test yaklaşımı notu (2026-09-18):** Bu ADR'nin ilk taslağı "QuestPDF'in kendi test yardımcılarını kullan" diyordu — uygulama sırasında kurulan paket (2026.9.0) incelendiğinde QuestPDF'in metin-çıkarma/test yardımcısı **içermediği** görüldü (saf bir üretim kütüphanesi). Kullanıcıyla netleştirilip **PdfPig** (MIT lisanslı, `UglyToad.PdfPig`) test projelerine (yalnızca test projelerine — üretim kodu etkilenmez) eklendi; testler `CvPdfDocument`'in ürettiği PDF'i PdfPig ile geri okuyup gerçek metin içeriğini doğrular.

## Sonuçlar

**Artıları:**
- Harici process/binary bağımlılığı yok, Plesk hosting'de ek kurulum gerektirmiyor.
- Fluent C# API, tasarımın kod içinde versiyonlanabilir/test edilebilir olmasını sağlıyor.

**Eksileri / kabul edilen trade-off'lar:**
- QuestPDF HTML render etmiyor (bilinçli mimari tercih) — tasarım tamamen C# component'leri ile yazılmalı, bir web tasarımcının HTML/CSS ile hızlı iterasyon yapması mümkün değil.
- Lisans, şirket yıllık geliri $1M eşiğini aşarsa (büyüme senaryosunda) ücretli lisansa geçiş gerektirir — bu ADR'de not düşülüyor, ileride bir hatırlatma olarak.

## İlgili Kararlar
- ADR-019: Dosya depolama (fotoğraf okuma için — bu ADR kapsamında `IFileStorageService.ReadAsync` eklendi)
- ADR-018: Candidate aggregate tasarımı
- ADR-020: `IReferenceDataLookupReader.GetByIdsAsync` (lookup id'lerinin görüntüleme adına toplu çözümlenmesi için kullanılır)
