# ADR-023: Employer Modülü Tasarımı — Company, Job, PersonnelNeed

## Durum

Taslak (kullanıcı onayı bekliyor).

## Bağlam

Employer modülü şu ana kadar kodda tamamen boş durumda (yalnızca `.gitkeep` dosyaları). Bu durum CareerAdvisor modülünün Görev 6 (Genel Havuz) ve Görev 7 (Matching modülü) işlerini bloke ediyor, çünkü ADR-022 §5'te `PersonnelNeed` aggregate'ine eklenen havuz alanları (`Status`, `PooledByAdvisorId`, vb.) somutlaşmış bir `PersonnelNeed` entity'sine muhtaç.

`docs/db/Employer.md`, firma tarafının doldurması istenen alanları üç grup halinde tanımlıyor: Firma Profili + Hesap Bilgileri, İlan Bilgileri, Personel İhtiyaç Bildirimi. `DOMAIN.md` (§7-12, §14-15, §27, §33.2-33.4) bu grupları kavramsal olarak **Company Profile**, **Job** ve **PersonnelRequest** olarak adlandırıyor ve Job için ayrı bir onay lifecycle'ı (`Draft → Submitted → UnderReview → Approved → Published` / `Rejected → RevisionRequested`) tanımlıyor.

Bu ADR ile şu isimlendirme çakışması da çözülüyor: `DOMAIN.md`/`PROJECT.md`'deki `PersonnelRequest` ile `ADR-022`'deki `PersonnelNeed` aynı kavramı temsil ediyor; bu ADR ile birlikte kod tarafında ve dokümantasyonda `PersonnelNeed` adı standart kabul edilir (ADR-022 zaten kabul edilmiş durumda olduğu için).

Application (adayın Job'a resmi başvurusu, `DOMAIN.md` §15) bu ADR'nin kapsamı dışında bırakılmıştır; Employer modülünde yaşayacağı kararlaştırılmış olmakla birlikte ayrı bir ADR'de ele alınacaktır.

## Karar

### 1. Company aggregate

```
Company
├── Id
├── UserId                    (FK → Identity.User)
├── CareerAdvisorId           (nullable; kayıt anında otomatik atanır)
├── Name
├── Logo                      (FileAttachment, 400x400 crop — ADR-019 IFileStorageService)
├── SectorId                  (FK → ReferenceData.Sector)
├── FoundedYear
├── EmployeeCount
├── WebsiteUrl
├── CountryId                 (FK → ReferenceData.Country)
├── ProvinceId                (FK → ReferenceData.Province)
├── DistrictId                (FK → ReferenceData.District)
├── Address
├── AboutHtml
├── ContactFirstName
├── ContactLastName
├── ContactEmail
├── ContactPhone
├── TaxOfficeId               (FK → ReferenceData.TaxOffice — zaten Province'e bağlı, ayrı bir "Vergi Dairesi İli" alanına gerek yok)
├── TaxNumber
├── MarketingConsent          (bool)
├── Status                    (PendingApproval / Approved / Rejected / Deactivated)
├── ApprovedByUserId          (nullable; yalnızca Admin rolü)
├── ApprovedAtUtc
├── RejectionReason
├── DeactivatedAtUtc
├── DeactivatedByUserId
└── CreatedAtUtc
```

**Registration akışı** — Candidate modülündeki desenin birebir aynısı: `RegisterEmployerCommand` önce `Identity.User`'ı oluşturur, ardından `Company`'yi oluşturur; `Company` oluşturma adımı başarısız olursa `User` deaktive edilerek compensate edilir.

**CareerAdvisor ataması** — kayıt anında, Admin onayından **bağımsız ve önce** çalışır. `CareerAdvisor` modülünün public contract'ından aktif danışman listesi senkron çekilir, en az yüklü danışmana (mevcut Candidate deseniyle aynı algoritma) atanır.

**Admin onayı** — ayrı bir adım, yalnızca Admin rolü verebilir (CareerAdvisor onaylayamaz). `Company.Status = PendingApproval` başlar, Admin `Approved` veya `Rejected` yapar.

**Red/deaktivasyon davranışı** — bir Company `Rejected` veya `Deactivated` durumuna geçtiğinde, atanmış olduğu danışmanın **aktif** listesinden düşer (danışmanın iş yükü hesaplamasına dahil edilmez), ancak atama kaydı geçmiş olarak korunur — `Identity.User` deaktivasyonunda kullanılan "erişim kapanır, kayıt silinmez" prensibiyle birebir aynı.

### 2. Job (İlan) aggregate

```
Job
├── Id
├── CompanyId                     (FK → Company)
├── Title
├── IsForDisabledCandidates       (bool)
├── EmploymentTypeId              (FK → ReferenceData.EmploymentType — "Çalışma Şekli": Tam/Yarı Zamanlı)
├── WorkLocationTypeId            (FK → ReferenceData.WorkLocationType — "Çalışma Tercihi": Uzaktan/Hibrit/Ofis)
├── PositionId                    (FK → ReferenceData.Position)
├── DepartmentId                  (FK → ReferenceData.Department)
├── ProvinceId                    (FK → ReferenceData.Province)
├── DescriptionHtml
├── GenderPreferences[]           (FK[] → ReferenceData.Gender, multi-select)
├── MilitaryStatusPreferences[]   (FK[] → ReferenceData.MilitaryStatus, multi-select)
├── ExperienceLevelId             (FK → ReferenceData.ExperienceLevel)
├── EducationLevelPreferences[]   (FK[] → ReferenceData.EducationLevel, multi-select)
├── DrivingLicensePreferences[]   (FK[] → ReferenceData.DriversLicenseType, multi-select)
├── LanguageRequirements[]        (LanguageId + LanguageLevelId çiftleri, satır satır)
├── Status                        (Draft → Submitted → UnderReview → Approved → Published / Rejected → RevisionRequested)
├── ReviewedByAdvisorId           (yalnızca Company.CareerAdvisorId ile aynı kişi olabilir — invariant)
├── ReviewedAtUtc
├── RejectionReason
├── RevisionNotes
├── PublishedAtUtc
└── CreatedAtUtc
```

**İnceleme kısıtı** — Job'ı yalnızca firmanın **kendi atanmış** CareerAdvisor'ı inceleyip onaylayabilir/reddedebilir; başka bir danışman bu işlemi yapamaz. Bu, `Company.CareerAdvisorId == Job.ReviewedByAdvisorId` invariant'ı ile command handler seviyesinde doğrulanır.

### 3. PersonnelNeed aggregate

```
PersonnelNeed
├── Id
├── CompanyId                     (FK → Company)
├── EmploymentTypeId              (FK → ReferenceData.EmploymentType)
├── WorkLocationTypeId            (FK → ReferenceData.WorkLocationType)
├── PositionId                    (FK → ReferenceData.Position)
├── DepartmentId                  (FK → ReferenceData.Department)
├── Quantity                      (int — kaç kişi aranıyor)
├── ProvinceId                    (FK → ReferenceData.Province — lokasyon)
├── GenderPreferences[]           (FK[] → ReferenceData.Gender)
├── MilitaryStatusPreferences[]   (FK[] → ReferenceData.MilitaryStatus)
├── ExperienceLevelId             (FK → ReferenceData.ExperienceLevel)
├── EducationLevelPreferences[]   (FK[] → ReferenceData.EducationLevel)
├── DrivingLicensePreferences[]   (FK[] → ReferenceData.DriversLicenseType)
├── DetailsText
├── Status                        (Taslak / KendiHavuzunda / GenelHavuzda / Karsilandi — ADR-022 §5)
├── PooledByAdvisorId             (ADR-022 §5)
├── PooledAtUtc                   (ADR-022 §5)
├── ClosedByAdvisorId             (ADR-022 §5)
├── ClosedAtUtc                   (ADR-022 §5)
├── FulfilledByCandidateCvId      (ADR-022 §5)
└── CreatedAtUtc
```

**İlan'dan farkı** — PersonnelNeed hiçbir zaman yayınlanmaz, kamuya açık değildir; doğrudan firmanın atanmış danışmanına düşer ve `KendiHavuzunda`/`GenelHavuzda` akışı ADR-022'de zaten tanımlı. Bu ADR yeni pool mantığı eklemez, yalnızca aggregate'in temel (havuz-dışı) alanlarını ve CRUD'unu tanımlar.

### 4. ReferenceData — yeni lookup gerekmiyor

Yukarıdaki tüm alanlar mevcut 23 lookup tipiyle karşılanıyor (`Sector`, `Country`/`Province`/`District`, `TaxOffice`, `EmploymentType`, `WorkLocationType`, `Position`, `Department`, `Gender`, `MilitaryStatus`, `ExperienceLevel`, `EducationLevel`, `DriversLicenseType`, `Language`/`LanguageLevel`). Employer modülü hiçbir yeni lookup migration'ı gerektirmez.

### 5. İsimlendirme birleştirmesi

`DOMAIN.md` ve `PROJECT.md`'deki `PersonnelRequest` referansları `PersonnelNeed` olarak güncellenecek (ADR-022 zaten kabul edilmiş olduğu için o taraf değişmiyor). Bu ADR'nin bir parçası olarak `DOMAIN.md` §11, §12, §27, §33.4 satırlarındaki `PersonnelRequest` metinleri `PersonnelNeed` ile değiştirilir.

## Sonuçlar

- CareerAdvisor Görev 6 (Genel Havuz) ve Görev 7 (Matching modülü) için gereken `PersonnelNeed` entity'si artık somutlaşmış durumda; blokaj kalkıyor.
- Company/Job/PersonnelNeed, Candidate modülündeki desenlerle (registration orkestrasyonu, ReferenceData lookup kullanımı, deaktivasyon davranışı) tutarlı.
- Job'ın yalnızca kendi danışmanı tarafından incelenebilmesi, ileride "danışman değişince bekleyen Job'lar ne olacak" sorusunu gündeme getirebilir — bu ADR kapsamında ele alınmamıştır, ADR-022'deki `ReassignOrphanedCompaniesCommand` akışına bir "bekleyen Job review'larının devri" adımı eklenmesi gerekebilir (ileri faz notu).
- Application (Job'a resmi başvuru) akışı bilinçli olarak bu ADR kapsamı dışında bırakılmıştır; ayrı bir ADR'de ele alınacaktır.

## Reddedilen Alternatifler

1. **PersonnelRequest adının korunması, ADR-022'nin PersonnelNeed'e uyarlanması** — ADR-022 zaten "Kabul edildi" durumda ve kod tarafında referans alınacağı için tersine uyarlama (yeni kararı eskiye uydurmak yerine eskiyi yeni karara uydurmak) tercih edildi.
2. **Job incelemesinin herhangi bir CareerAdvisor tarafından yapılabilmesi (Genel Havuz mantığına benzer)** — reddedildi; Job onayı, firmanın kendi danışmanlık ilişkisinin bir parçası olarak kalmalı, havuz mantığı yalnızca PersonnelNeed'e özgü.
3. **CareerAdvisor'ın da Company onayı verebilmesi** — reddedildi; onay yetkisi yalnızca Admin'de kalıyor, danışman ataması onaydan bağımsız çalışıyor.
