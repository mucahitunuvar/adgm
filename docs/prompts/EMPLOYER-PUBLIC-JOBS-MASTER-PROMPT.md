# Employer Modülü — Public İlan Altyapısı (Website Faz 5 Ön İşi) Master Prompt

## 0. Nasıl kullanılır

Bu dosya **görev görev** uygulanır. Her görev ayrı bir Claude Code oturumunda yürütülür:

1. Kullanıcı `/clear` yapar ve şunu gönderir: *"docs/prompts/EMPLOYER-PUBLIC-JOBS-MASTER-PROMPT.md dosyasındaki §1'i ve Görev N'i oku, yalnızca Görev N'i uygula."*
2. Yalnızca **§1** ve **ilgili görevi** okursun. Görevin **"Oku"** listesi dışındaki dokümanları okuma; AGENTS.md'nin yalnızca belirtilen bölümlerine bak.
3. Önceki görevlerin kodu commit'lenmiştir; ihtiyaç duyduğunda `git log` ve kaynak dosyalarından öğren.
4. Görev bitince commit'le, push'la, kısa raporunu yaz ve **dur**.

**Neden ayrı bir prompt:** ADR-024 §18 "Paralel iş" bu işi Website kapsamı dışında, **Employer modülüne ait** sayar. Website Faz 5'in ilan arama adaptörü (`WEBSITE-FAZ5-MASTER-PROMPT.md`) bu işin bittiğini varsayar; bu prompt **Faz 5'ten önce** uygulanır.

---

## 1. Genel kurallar (her görevde geçerli)

**Kapsam:** Yalnızca `src/Modules/Employer`, `src/BuildingBlocks/Contracts/Employer`, Host kayıtları, testler ve dokümanlar. **Website modülüne dokunulmaz** (Employer, Website'e bağımlı olamaz; tersi de yoktur — bağ yalnızca BuildingBlocks contract'ı ve Host adaptörüdür).

**Kullanıcı kararları:**

- Public ilan uçları Employer'da yaşar ve anonimdir (`public-read` rate limit policy'si; Host'ta tanımlı).
- `SiteSettings.PublicJobListingsEnabled` Website'e aittir; **Employer bunu okumaz.** Bayrak, frontend (public site yanıtı) ve Website'in arama adaptörü tarafından uygulanır. Employer public uçları bayraktan bağımsız çalışır.
- Public yanıtlara yalnızca **yayında (`Published`) ve firması onaylı (`Approved`)** ilanlar girer. Firma sonradan reddedilir/pasife alınırsa ve ilan askıya alınırsa (`AdminSuspendJob`) ilan hemen public'ten kaybolur.
- Public ilan yanıtında **cinsiyet tercihi yer almaz** (ayrımcılık riski; yalnızca danışman/aday eşleştirmesinde kullanılır; hukuki teyit gerektiren bir konudur). **Askerlik durumu tercihi yalnızca ilan detayında görünür** (kullanıcı kararı; listede ve arama özetinde yer almaz). Eğitim düzeyi, ehliyet, dil, deneyim düzeyi detayda yer alır.
- Firma iletişim bilgisi, vergi bilgisi, atanmış danışman, inceleme notu, ret/revizyon notu **hiçbir public yanıtta yer almaz.**
- Firma logosu yalnızca firmanın açık onayı varsa (`ShowLogoOnWebsite`) public servis edilir.

**Mimari:** Mevcut Employer desenleri: vertical slice, `RowVersion` ile iyimser eşzamanlılık (`409`), `TimeProvider`, projection + `AsNoTracking`, Result deseni, tek tip-tek dosya (AGENTS §11). Public uçlar `/api/v1/public/...`. Modüller arası okuma yalnızca `BuildingBlocks/Contracts` altındaki contract'larla (ADR-016; `ICompanyModuleContract` örneği).

**Test komutu (görev içinde):**

```bash
dotnet test GenclikMerkezi.slnx --filter "FullyQualifiedName~Employer|FullyQualifiedName~Architecture" --verbosity quiet --nologo
```

Her görevde unit testlerin yanında **integration testleri zorunludur** (SQLite + HTTP). Tam paket yalnızca Görev 5'te çalışır.

**Commit:** Açık dosya listesiyle stage et (`git add .` kullanma). Push'tan sonra `git log origin/main -1` ile doğrula.

**Çelişki:** Kod veya ADR ile çelişki bulursan kendin karar verme; AGENTS.md §6 formatında dur ve raporla.

**Rapor (kısa):** commit hash, migration, filtreli test sayıları, sapmalar ve varsayımlar.

---

## Görev 1 — İlan slug'ı

**Oku:** ADR-023 (Job bölümü); `Job.cs`, `JobStatus.cs`, ilanı yayınlayan handler (`ApproveJobCommandHandler`) ve `AdminReinstateJob`; `JobConfiguration`.

- `Job.Slug` (maks 120, **benzersiz indeks**, nullable: taslak ilanda `null`). Slug **yalnızca ilk yayınlandığı anda** atanır ve sonra **değişmez** (URL kararlılığı; yayındaki ilanın başlığı zaten düzenlenemez, yeniden yayınlama/`AdminReinstateJob` slug'ı korur).
- Biçim: `{slug-başlık}-{Job.Id'nin ilk 8 hex karakteri}`. Başlık kısmı: küçük harf, Türkçe karakterler ASCII'ye (`ş→s, ğ→g, ü→u, ö→o, ç→c, ı→i, İ→i`), harf/rakam dışındakiler `-`, ardışık `-` teke iner, uçlardan kırpılır, **en çok 60 karakter** (kelime ortasında kesilebilir, sonda `-` kalmaz); boş kalırsa `ilan`. Id eki çakışmayı pratikte imkânsız kılar; benzersiz indeks emniyet kemeridir.
- Üretim `JobSlugGenerator` (Employer.Domain, saf statik; `Job` yayın metodu çağırır). Website'in `Slug` VO'suna bağımlılık yoktur — kasıtlı küçük tekrar, SharedKernel'e taşıma yok.
- **Migration + geri doldurma:** `Slug` sütunu nullable eklenir (filtrelenmiş benzersiz indeks: yalnızca `Slug IS NOT NULL` satırlar). Mevcut `Published` ve daha önce yayınlanmış (`PublishedAtUtc` dolu) ilanlara slug, tek seferlik, idempotent bir Employer-içi komutla atanır (`BackfillJobSlugsCommand`; SQL'de değil, aynı `JobSlugGenerator` ile; SQLite ve SQL Server uyumlu). Komutun nasıl tetiklendiğini (başlangıçta mı, admin ucu mu) raporda belirt.

**Testler:** Üretim kuralları (tablo testi: Türkçe karakter, uzun başlık, simge, boş başlık); ilk yayında atanma; yeniden yayınlamada değişmeme; benzersiz indeks; taslakta `null` (integration).

**Commit:** `feat(employer): assign immutable slug to jobs on first publish`

---

## Görev 2 — Firma public profili ve logo onayı

**Oku:** ADR-023 (Company bölümü); `Company.cs`, `UploadCompanyLogo` slice'ı ve dosya servisi kullanımı; ADR-019 (`IFileStorageService`); `GetMyCompany`.

- `Company.ShowLogoOnWebsite` (bool, varsayılan `false`). Firma kendi hesabından açıp kapatır: `PUT /api/v1/employer/companies/me/logo-visibility` (`{ showLogoOnWebsite, RowVersion }`; mevcut firma yetki policy'si). Logo yoksa `true` yapılamaz (`Company.LogoRequired`). `GetMyCompany` yanıtına eklenir.
- Public firma profili: `GET /api/v1/public/companies/{id}` (`public-read`). Yalnızca **onaylı** firma; yoksa/onaysızsa `404`. Alanlar: `id`, `name`, sektör adı, il adı, `foundedYear`, `employeeCount`, `websiteUrl`, `aboutHtml` (zaten sanitize edilmiş olmalı; değilse okuma anında `IHtmlContentSanitizer` benzeri mevcut Employer sanitizer'ı kullan, yoksa **dur ve raporla**), `hasLogo` (= logo var **ve** `ShowLogoOnWebsite`), `publishedJobCount`. **İletişim, adres, vergi, danışman yok.**
- Logo servisi: `GET /api/v1/public/companies/{id}/logo` — yalnızca onaylı firma + `ShowLogoOnWebsite = true` iken dosyayı akış olarak döner (`Cache-Control: public, max-age=3600`, doğru `Content-Type`); aksi halde `404`. Dosya yolu istemciden gelmez.
- Ad/sektör/il adları `IReferenceDataLookupReader` ile çözülür (toplu; N+1 yok).

**Testler:** Logo olmadan `true` reddi; onaysız firma `404`; `ShowLogoOnWebsite=false` iken logo `404`; yanıtta yasaklı alanların bulunmaması (yanıt tipinin alan listesi testi); yetki (başka firma değiştiremez) (integration).

**Commit:** `feat(employer): add public company profile and consent-based logo exposure`

---

## Görev 3 — Public ilan listesi ve detayı

**Oku:** Görev 1–2'nin kodu; `GetPublishedJobs` slice'ı (bu uç **firma/danışman içindir**, sayfalamasızdır, iç alanları döner — **değiştirilmez**); `JobResponse`; `PagedRequest/PagedResult<T>` (SharedKernel); `IReferenceDataLookupReader`.

- `GET /api/v1/public/jobs` (`public-read`): sayfalı (`PagedRequest`, varsayılan 20, maks 100). Filtreler: `provinceId`, `employmentTypeId`, `workLocationTypeId`, `positionId`, `departmentId`, `companyId`, `isForDisabledCandidates`, `q` (başlıkta ve firma adında; 2–100 karakter; `ILIKE` benzeri büyük/küçük harf duyarsız `LIKE`, Türkçe karakter duyarlılığı için mevcut yaklaşımı kullan, yoksa basit `Contains` ve raporla). Sıralama: `PublishedAtUtc` azalan, eşitlikte `Id`.
  - Satır: `id`, `slug`, `title`, firma `{id, name, hasLogo}`, il adı, çalışma şekli adı, çalışma tercihi adı, pozisyon adı, `isForDisabledCandidates`, `publishedAtUtc`, `summary` (açıklama HTML'inden **düz metne** çevrilmiş ilk **200** karakter, kelime sınırında).
- `GET /api/v1/public/jobs/{slug}`: tam detay — yukarıdakilere ek: `descriptionHtml` (sanitize edilmiş), departman, deneyim düzeyi adı, eğitim düzeyi adları, ehliyet adları, **askerlik durumu tercihi adları** (`IReferenceDataLookupReader` ile), dil gereksinimleri `{languageName, levelName}`. **Cinsiyet tercihi, danışman, inceleme/ret/revizyon alanları yok.** Bulunamayan/yayında olmayan/firması onaysız → `404` (neden ayrımı sızdırılmaz).
- Yalnızca public kurallara uyan (§1) ilanlar. Sorgular `AsNoTracking` + projection; arama alanlarına indeks ihtiyacını değerlendir ve rapora yaz (yeni indeks için gerekçe).
- HTML'den düz metin üretimi tek yerde (`JobSummaryTextBuilder`, Employer.Domain veya Application); Görev 4 de bunu kullanır.

**Testler:** Public kuralları tablo testi (Published/Draft/UnderReview/Suspended × firma durumu); filtre kombinasyonları; sayfalama sınırları; `q` doğrulaması; yanıtta yasaklı alanların bulunmaması (cinsiyet tercihi hiçbir yanıtta yok; askerlik durumu tercihi yalnızca detayda var, listede ve `PublishedJobSummary`'de yok); özet üretimi (HTML etiketleri, varlıklar, kelime sınırı); `404` tekdüzeliği (integration).

**Commit:** `feat(employer): add public job listing and slug-based detail endpoints`

---

## Görev 4 — Arama adaptörü için public contract

**Oku:** `src/BuildingBlocks/Contracts/Employer/ICompanyModuleContract.cs` ve implementasyonu/Host kaydı; ADR-016 (contract deseni); Görev 3'ün sorgu kodu; ADR-024 §10 (pull modeli: sayfalı, kaynak durumu tam senkron için kararlı sıralı).

- Yeni contract: `BuildingBlocks/Contracts/Employer/IPublishedJobModuleContract`:

  ```csharp
  Task<PagedResult<PublishedJobSummary>> GetPublishedJobSummariesAsync(
      int page, int pageSize, CancellationToken cancellationToken = default);
  ```

  `PublishedJobSummary` (ayrı dosya, BuildingBlocks): `JobId`, `Slug`, `Title`, `CompanyName`, `ProvinceName`, `EmploymentTypeName`, `WorkLocationTypeName`, `PositionName`, `Summary` (Görev 3'teki düz metin, **500** karaktere kadar), `PublishedAtUtc`. Yalnızca public kurallara uyan ilanlar (§1); **kişisel veri, iç alan yok**.
- Sıralama **kararlı**: `PublishedAtUtc` artan, sonra `Id` (sayfa kayması olmasın). `pageSize` en çok 200.
- Implementasyon `Employer.Infrastructure`'da; Host'ta tek yerde kayıtlı. Contract, `ICompanyModuleContract` gibi modül iç tiplerini sızdırmaz.
- Website bu contract'ı **bilmez**: adaptör Faz 5'te Host'ta yazılır.

**Testler:** Public kurallar (yalnızca uygun ilanlar), kararlı sıralama ve sayfalar arası kayıpsız/çiftsiz gezinme (250 ilan, sayfa 100 → toplam eşleşmesi), `pageSize` üst sınırı, özet uzunluğu; mimari test (contract tipleri Employer iç tiplerine referans vermez) (integration).

**Commit:** `feat(employer): expose published job summaries through a module contract`

---

## Görev 5 — Doküman senkronizasyonu ve tam test

**Oku:** ADR-023 (Job, Company); ADR-024 §10, §18 ("Paralel iş"); `SECURITY.md` Employer bölümü; bu işin commit'leri.

- **ADR-023:** İlan slug'ı kuralları (ilk yayında atanır, değişmez, biçim); `ShowLogoOnWebsite` ve public logo kuralı; public ilan/firma uçları ve **yanıttan hariç tutulan alanlar** (cinsiyet tercihi, iletişim, vergi, danışman, inceleme notları) ile gerekçesi ve askerlik durumu tercihinin yalnızca detayda gösterilmesi kararı; `PublicJobListingsEnabled`'ın Employer'ı değil frontend ve Website arama adaptörünü kısıtladığı; `IPublishedJobModuleContract`.
- **SECURITY.md:** Public ilan/firma yanıtlarında kişisel veri ve iç alan olmaması; logo yalnızca onayla; `404` tekdüzeliği.
- **ADR-024 §18:** "Paralel iş (Employer)" notunu **tamamlandı** olarak işaretle (Website kodu değişmez; yalnızca bu satırın doküman güncellemesi).
- **Tam test paketi:** `dotnet test GenclikMerkezi.slnx`, sayılarıyla.

**Commit:** `docs(employer): document public job infrastructure and sync ADR-023, security and ADR-024 notes`

**Rapor (kısa):** görev başına commit hash'i, tam paket sayıları, tüm sapmalar ve ertelenen işler.

---

## Kapsam dışı

Başvuru (Application) akışı; ilan için schema.org `JobPosting` verisi; ilan sitemap girişi (Website Faz 5 arama kaydından türetilir); firma slug'ı (firma URL'si Id tabanlıdır); ilan arama için SQL Server full-text; Website'in herhangi bir kodu.
