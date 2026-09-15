# DOMAIN.md

# Gençlik Merkezi — Domain Tanımı

## 1. Domain Özeti

Gençlik Merkezi'nin temel domain amacı, iş arayan gençler ile personel ihtiyacı bulunan firmaları kariyer danışmanlığı desteğiyle bir araya getirerek gençlerin istihdam edilmesini ve istihdam süreçlerinin takip edilmesini sağlamaktır.

Sistem klasik bir iş ilanı ve başvuru platformu değildir.

Temel domain yaklaşımı:

```text
Genç
  ↓
Profil
  ↓
Kariyer Değerlendirmesi
  ↓
Beceri / İhtiyaç Analizi
  ↓
Uygun Fırsat
  ↓
Eşleştirme
  ↓
Danışman Yönlendirmesi
  ↓
Görüşme
  ↓
İşe Yerleşme
  ↓
İstihdam Takibi
```

İş ilanları bu yapının yalnızca bir parçasıdır.

Sistemin temel değerini oluşturan domain kavramları:

* Candidate
* Employer
* CareerAdvisor
* CandidateProfile
* Job
* PersonnelRequest
* Matching
* Recommendation
* Interview
* Employment
* EmploymentFollowUp
* Assessment
* Skill
* Education
* Training
* Event

oluşturmaktadır.

---

# 2. Temel Domain Aktörleri

Sistemde dört temel kullanıcı aktörü vardır:

1. Admin
2. Employer
3. Candidate
4. CareerAdvisor

Bu roller sistemdeki kullanıcı yetkilerini temsil eder.

Ancak domain açısından kullanıcı ile iş/business kavramları birbirinden ayrılmalıdır.

Örneğin:

```text
User ≠ Candidate
User ≠ Employer
User ≠ CareerAdvisor
```

Bir kullanıcı sisteme giriş yapabilen kimliği temsil eder.

Candidate, Employer ve CareerAdvisor ise sistem içerisindeki domain rollerini ve davranışlarını temsil eder.

---

# 3. Candidate

## 3.1 Tanım

Candidate, Gençlik Merkezi üzerinden kariyer ve istihdam hizmetlerinden yararlanan iş arayan kişidir.

Candidate yalnızca iş ilanlarına başvuran bir kullanıcı değildir.

Candidate;

* Kariyer profiline,
* Eğitim bilgilerine,
* Deneyimlerine,
* Becerilerine,
* Sertifikalarına,
* Kariyer hedeflerine,
* İhtiyaçlarına,
* Tercihlerine

sahip bir kariyer varlığıdır.

Candidate entity alanları için bkz. db\Candidate.md
---

# 4. Candidate Profile

Candidate Profile, adayın kariyer açısından değerlendirilebilmesini sağlayan temel bilgi kümesidir.

Profil aşağıdaki bilgileri içerebilir:

### Kişisel Bilgiler

* Ad
* Soyad
* İletişim bilgileri
* Lokasyon
* Doğum bilgileri
* Diğer gerekli bilgiler

### Eğitim

* Eğitim seviyesi
* Okullar
* Bölümler
* Mezuniyet bilgileri

### Deneyim

* İş deneyimleri
* Stajlar
* Projeler

### Beceri

* Teknik beceriler
* Mesleki beceriler
* Sosyal beceriler
* Yabancı dil

### Sertifika

* Sertifikalar
* Eğitimler
* Yetkinlik belgeleri

### Kariyer

* Kariyer hedefleri
* İlgilenilen sektörler
* İlgilenilen pozisyonlar
* Çalışma tercihleri
* Lokasyon tercihleri

Candidate Profile'ın amacı yalnızca CV oluşturmak değil, adayın **kariyer uygunluğunu değerlendirebilecek zengin bir veri modeli** oluşturmaktır.

---

# 5. CareerAdvisor

CareerAdvisor, Gençlik Merkezi'nin aday ve işverenlerle ilgilenen kariyer danışmanıdır.

Sistemin en önemli domain aktörlerinden biridir.

CareerAdvisor:

* Adayları değerlendirir.
* Firmaları değerlendirir.
* Adaylarla görüşür.
* Kariyer ihtiyaçlarını belirler.
* Beceri eksikliklerini belirler.
* Eğitim ihtiyaçlarını belirler.
* Uygun iş fırsatlarını araştırır.
* Adayları firmalara önerebilir.
* İşverenlerin personel ihtiyaçlarını değerlendirir.
* Görüşmeleri organize eder.
* İşe yerleşme sürecini takip eder.
* İşe yerleşme sonrasında adayı takip eder.

Temel ilişki:

```text
Candidate
     ↕
CareerAdvisor
     ↕
Employer
```

CareerAdvisor bu iki taraf arasındaki koordinasyonu sağlayan merkezi aktördür.

---

# 6. CareerAdvisor Assignment

Adaylar ve firmalar bir CareerAdvisor'a atanabilir.

```text
Candidate → CareerAdvisor
Employer  → CareerAdvisor
```

Atama:

* Manuel
* Otomatik
* Yeniden dağıtım

şeklinde gerçekleştirilebilir.

Otomatik atama sistemi ilerleyen aşamalarda aşağıdaki kriterleri dikkate alabilir:

* Danışmanın mevcut iş yükü
* Aday sayısı
* Firma sayısı
* Uzmanlık alanı
* Firma sektörü
* Adayın kariyer alanı
* Lokasyon
* Danışmanın aktiflik durumu

Atama sisteminin amacı danışmanlar arasında dengeli ve anlamlı bir iş yükü oluşturmaktır.

---

# 7. Employer

Employer, Gençlik Merkezi üzerinden personel ihtiyacını karşılamak isteyen firmayı temsil eder.

Employer:

* Firma profilini oluşturabilir.
* Firma bilgilerini yönetebilir.
* İş ilanı oluşturabilir.
* Personel ihtiyacı bildirebilir.
* Aday önerilerini inceleyebilir.
* Görüşme talep edebilir.
* Aday değerlendirebilir.
* İşe alım gerçekleştirebilir.
* Görüşme ve işe alım süreçlerini takip edebilir.

Employer yalnızca ilan yayınlayan bir kullanıcı değildir.

Employer aynı zamanda Gençlik Merkezi'nin kariyer danışmanlığı hizmetinden yararlanan işveren tarafıdır.

Employer entity alanları için bkz. db\Employer.md
---

# 8. Company Profile

Firma profilinde:

* Firma adı
* Sektör
* Firma açıklaması
* Lokasyon
* İletişim bilgileri
* Web sitesi
* Çalışan sayısı
* Faaliyet alanları
* Logo
* Diğer kurumsal bilgiler

bulunabilir.

Onaylanmış ve yayınlanabilir firma profili, kamuya açık web sitesinde gösterilebilir.

---

# 9. Job

Job, işveren tarafından belirli bir pozisyon için oluşturulan iş ilanıdır.

Bir Job aşağıdaki bilgileri içerebilir:

* Pozisyon
* Açıklama
* Departman
* Lokasyon
* Çalışma şekli
* Çalışma saatleri
* Ücret bilgisi
* Eğitim kriterleri
* Deneyim kriterleri
* Beceri kriterleri
* Sertifika kriterleri
* Yabancı dil kriterleri
* Diğer işe alım kriterleri

Job doğrudan yayınlanamaz.

İlanın yayınlanması için tanımlı onay sürecinden geçmesi gerekir.

---

# 10. Job Lifecycle

Bir Job aşağıdaki yaşam döngülerinden geçebilir:

```text
Draft
  ↓
Submitted
  ↓
UnderReview
  ↓
Approved → Published
  │
  └→ Rejected → RevisionRequested
```

İlan uygun bulunmazsa CareerAdvisor veya yetkili yönetici:

* Red nedeni
* Gerekli düzeltmeler
* Açıklamalar

ile birlikte ilana geri dönüş yapabilir.

İşveren gerekli düzenlemeleri yaptıktan sonra ilan tekrar değerlendirmeye gönderilebilir.

---

# 11. Personnel Request

PersonnelRequest, Employer'ın doğrudan Gençlik Merkezi'ne personel ihtiyacını bildirdiği domain kavramıdır.

Bu yapı Job'dan farklıdır.

Employer:

> "İlan yayınlamak istiyorum."

demek yerine:

> "Şu özelliklerde personel arıyorum."

diyebilir.

Örneğin:

```text
Pozisyon: Kaynak Ustası
Adet: 3
Deneyim: 2 yıl
Eğitim: Meslek Lisesi
Beceri: Kaynak
Lokasyon: Konya
```

Bu talep doğrudan CareerAdvisor tarafından değerlendirilir.

---

# 12. Job ve PersonnelRequest Ayrımı

Bu ayrım domain açısından kritiktir.

```text
Job
↓
Kamuya açık / kontrollü şekilde yayınlanabilen iş ilanı
```

```text
PersonnelRequest
↓
Firma tarafından Gençlik Merkezi'ne iletilen personel ihtiyacı
```

PersonnelRequest'in kamuya açık bir iş ilanına dönüşmesi zorunlu değildir.

Örneğin:

```text
Employer
   ↓
PersonnelRequest
   ↓
CareerAdvisor
   ↓
Aday Havuzu
   ↓
Uygun Adaylar
   ↓
Employer
```

Bu sistemin klasik iş ilanı sitelerinden ayrıldığı temel noktalardan biridir.

---

# 13. Matching

Matching, bir Candidate ile bir Job, PersonnelRequest veya başka bir kariyer fırsatı arasındaki uygunluk ilişkisidir.

Eşleştirme aşağıdaki kriterleri kullanabilir:

* Eğitim
* Beceri
* Deneyim
* Sertifika
* Yabancı dil
* Lokasyon
* Çalışma şekli
* Kariyer hedefleri
* Pozisyon
* Diğer kriterler

Eşleştirme sonucu bir uygunluk skoru veya açıklaması üretilebilir.

Örneğin:

```text
Candidate A
      +
Job B
      ↓
Matching
      ↓
Compatibility Score: 87%
```

Ancak skor tek başına karar mekanizması değildir.

CareerAdvisor gerektiğinde eşleştirme sonucunu inceleyebilir ve karar verebilir.

---

# 14. Recommendation

Recommendation, bir adayın belirli bir firma, pozisyon veya personel ihtiyacı için önerilmesidir.

Recommendation ile Application aynı kavram değildir.

Örneğin:

```text
CareerAdvisor
      ↓
Candidate A
      ↓
Employer B
```

şeklinde danışman tarafından yapılan yönlendirme bir Recommendation'dır.

Bu yönlendirme bir Job üzerinden yapılabileceği gibi PersonnelRequest üzerinden veya ilan olmadan doğrudan da yapılabilir.

---

# 15. Application

Application, adayın belirli bir Job için resmi olarak başvuru gerçekleştirmesidir.

Application sistemde bulunabilir ancak sistemin temel çalışma modeli Application merkezli değildir.

Klasik model:

```text
Job
 ↓
Application
 ↓
Evaluation
```

Gençlik Merkezi modelinde ise:

```text
Candidate
 ↓
Assessment
 ↓
Matching
 ↓
Recommendation
 ↓
Interview
```

veya:

```text
Candidate
 ↓
Application
 ↓
Interview
```

gibi birden fazla yol mümkündür.

Bu nedenle Application, domainin önemli parçalarından biridir ancak **domainin merkezinde tek başına yer almaz**.

---

# 16. Assessment

Assessment, adayın kariyer açısından değerlendirilmesi sürecidir.

CareerAdvisor aday ile görüşerek:

* Becerilerini
* Eğitim durumunu
* Deneyimlerini
* Kariyer hedeflerini
* İhtiyaçlarını
* Eksik yetkinliklerini
* Kariyer beklentilerini

değerlendirebilir.

Assessment sonucu:

* Skill Gap
* Training Recommendation
* Career Recommendation
* Job Recommendation
* Employer Recommendation

gibi sonuçlara dönüşebilir.

---

# 17. Skill

Skill, adayın sahip olduğu veya geliştirmesi gereken yetkinliği temsil eder.

Skill iki farklı açıdan ele alınabilir:

### Candidate Skill

Adayın sahip olduğu beceri.

### Required Skill

Firma veya pozisyon tarafından aranan beceri.

Eşleştirme sırasında:

```text
Candidate Skill
       +
Required Skill
       ↓
    Matching
```

ilişkisi kurulabilir.

---

# 18. Skill Gap

SkillGap, adayın hedeflediği pozisyon veya kariyer alanı için sahip olması gereken ancak eksik olan yetkinlikleri temsil eder.

Örneğin:

```text
Hedef Pozisyon:
Junior Software Developer

Candidate Skills:
C#
SQL

Required Skills:
C#
SQL
Git
Docker

Skill Gap:
Git
Docker
```

Bu bilgiler eğitim ve kariyer önerilerinde kullanılabilir.

---

# 19. Education / Training

Education veya Training, adayın kariyer gelişimini destekleyen eğitim faaliyetlerini temsil eder.

Sistem:

* Eğitim
* Atölye
* Seminer
* Sertifika programı
* Kariyer etkinliği

gibi faaliyetleri yönetebilir.

Adayın SkillGap bilgisine göre uygun eğitim önerilebilir.

```text
Candidate
   ↓
SkillGap
   ↓
Training Recommendation
   ↓
Training
   ↓
Skill Improvement
```

---

# 20. CareerDevelopment

CareerDevelopment, adayın kariyer danışmanı eşliğinde yürüttüğü gelişim
sürecini temsil eder.

Bu domain, "adayın şu anda neyi eksik olduğunu ve bu eksikliği nasıl
kapatacağını" yönetir — iş/pozisyon eşleştirmesinden (Matching) önce gelir.

CareerDevelopment kapsamındaki kavramlar:

```text
CareerDevelopment
    │
    ├── SkillGap
    ├── CareerGoal
    ├── DevelopmentPlan
    ├── TrainingRecommendation
    └── AdvisorRecommendation
```

## CareerGoal

Adayın kariyer hedefini temsil eder (örn. hedeflenen pozisyon, alan,
sektör). Adayın kendisi veya CareerAdvisor tarafından belirlenebilir.

## DevelopmentPlan

CareerAdvisor'ın, aday için SkillGap ve CareerGoal'a dayanarak oluşturduğu
gelişim planıdır. Bir DevelopmentPlan birden fazla TrainingRecommendation
ve AdvisorRecommendation içerebilir.

## TrainingRecommendation

Adayın belirli bir SkillGap'i kapatmak amacıyla belirli bir eğitime
yönlendirilmesini temsil eder.

Önemli sınır:

```text
CareerDevelopment.TrainingRecommendation
        ≠
Website.Training
```

`Website.Training` eğitimin kendisidir (tarih, yer, içerik, kontenjan) ve
Website modülünün sahipliğindedir.

`TrainingRecommendation` ise "bu adaya, şu SkillGap nedeniyle, şu eğitim
önerildi" kaydıdır ve CareerDevelopment modülünün sahipliğindedir.

CareerDevelopment, Website.Training'e yalnızca contract/ID referansı ile
bağlanır; Website'in database'ine doğrudan erişmez veya Training verisini
kopyalamaz (bkz. ADR-003, ADR-008).

## AdvisorRecommendation

CareerAdvisor'ın aday için serbest metin veya yapılandırılmış biçimde
girdiği değerlendirme/öneridir.

Örnek:

```text
"Bu adayın teknik becerisi yeterli ancak Excel konusunda
gelişmesi gerekiyor."
```

Bu girdi bir SkillGap kaydına dönüştürülebilir ve DevelopmentPlan'a
bağlanabilir.

## Akış

```text
Candidate
   ↓
CareerAdvisor Değerlendirmesi
   ↓
SkillGap
   ↓
DevelopmentPlan
   ↓
TrainingRecommendation
   ↓
Training (Website)
   ↓
Skill Improvement
   ↓
Matching
```

## Domain Sınırı

CareerDevelopment, Candidate'ın kariyer gelişim sürecini yönetir; Matching
ise CareerDevelopment sürecinin ürettiği güncellenmiş yetkinlik/hedef
bilgisini girdi olarak kullanır.

CareerDevelopment, Matching'in kendisini gerçekleştirmez ve Matching
algoritmasının bir parçası değildir.

---

# 21. Event

Event, Gençlik Merkezi tarafından düzenlenen etkinlikleri temsil eder.

Örneğin:

* Kariyer günleri
* İşveren buluşmaları
* Seminerler
* Atölyeler
* Eğitimler
* Gönüllülük faaliyetleri
* Kariyer etkinlikleri

Event'ler adaylarla ilişkilendirilebilir.

---

# 22. Interview

Interview, aday ile işveren arasında gerçekleştirilen görüşme sürecidir.

Interview aşağıdaki kaynaklardan oluşabilir:

```text
Job
PersonnelRequest
Recommendation
Matching
Application
```

Görüşme:

* Tarih
* Saat
* Lokasyon
* Görüşme tipi
* Aday
* İşveren
* Pozisyon
* CareerAdvisor
* Durum
* Notlar

gibi bilgileri içerebilir.

---

# 23. Interview Lifecycle

Görüşme yaşam döngüsü örneğin:

```text
Requested
    ↓
Scheduled
    ↓
Confirmed
    ↓
Completed
    ↓
Result
```

şeklinde ilerleyebilir.

Sonuçlar:

* Positive
* Negative
* Pending
* FollowUpRequired

gibi durumlar içerebilir.

Görüşme sonucu CareerAdvisor tarafından notlandırılabilir.

---

# 24. Employment

Employment, adayın bir firma tarafından işe alınması sonucunda oluşan istihdam kaydıdır.

Employment:

* Candidate
* Employer
* Position
* StartDate
* EmploymentStatus

gibi bilgileri içerebilir.

Temel süreç:

```text
Interview
   ↓
Positive
   ↓
Employment
   ↓
Start
```

Ancak işe yerleşme yalnızca bir görüşmenin olumlu sonuçlanmasına bağlı olarak modellenmek zorunda değildir.

Domain, farklı işe yerleşme senaryolarını destekleyebilmelidir.

---

# 25. Employment Follow-Up

Gençlik Merkezi'nin sorumluluğu işe yerleşmeyle sona ermez.

İşe yerleşen adayın devamlılığı ve istihdam durumu takip edilebilir.

Takip kapsamında:

* İşe başladı mı?
* Çalışmaya devam ediyor mu?
* İşveren memnuniyeti
* Aday memnuniyeti
* İşten ayrılma durumu
* İşten ayrılma nedeni
* Takip görüşmeleri
* Danışman notları

gibi bilgiler tutulabilir.

Temel model:

```text
Employment
    ↓
Follow-Up
    ↓
Continues / Left
```

---

# 26. Career Journey

Candidate'ın sistem içerisindeki kariyer yolculuğu tek bir Job başvurusundan ibaret değildir.

Örnek kariyer yolculuğu:

```text
Candidate Registration
        ↓
Profile Completion
        ↓
CV Completion
        ↓
CareerAdvisor Assignment
        ↓
Assessment
        ↓
Skill Analysis
        ↓
Skill Gap
        ↓
Training Recommendation
        ↓
Matching
        ↓
Recommendation
        ↓
Interview
        ↓
Employment
        ↓
Employment Follow-Up
```

Bu yolculuk doğrusal olmak zorunda değildir.

Örneğin aday görüşme sonrasında işe alınmazsa:

```text
Interview
   ↓
Negative
   ↓
Assessment
   ↓
Skill Gap
   ↓
Training
   ↓
New Matching
```

şeklinde süreç devam edebilir.

---

# 27. Employer Journey

İşverenin sistem içerisindeki yolculuğu:

```text
Employer Registration
        ↓
Company Profile
        ↓
Approval
        ↓
CareerAdvisor Assignment
        ↓
PersonnelRequest / Job
        ↓
Candidate Matching
        ↓
Candidate Recommendation
        ↓
Interview
        ↓
Employment
```

---

# 28. CareerAdvisor Journey

CareerAdvisor'ın temel çalışma döngüsü:

```text
Assigned Candidates
        +
Assigned Employers
        ↓
Candidate Assessment
        +
Employer Need Analysis
        ↓
Matching
        ↓
Recommendation
        ↓
Interview
        ↓
Employment
        ↓
Follow-Up
```

CareerAdvisor'ın temel görevi iki tarafın verilerini anlamlı şekilde birleştirmektir.

---

# 29. Candidate ↔ Employer Relationship

Candidate ile Employer arasındaki ilişki doğrudan olmak zorunda değildir.

İlişki aşağıdaki aracılar üzerinden kurulabilir:

```text
Candidate
    ↓
Application
    ↓
Employer
```

veya:

```text
Candidate
    ↓
Recommendation
    ↓
Employer
```

veya:

```text
Candidate
    ↓
Matching
    ↓
Employer
```

veya:

```text
Candidate
    ↓
CareerAdvisor
    ↓
Employer
```

Bu nedenle Candidate ve Employer arasındaki ilişki tek bir entity veya tek bir workflow ile sınırlandırılmamalıdır.

---

# 30. Notification

Notification, sistem içerisinde kullanıcıları önemli olaylardan haberdar etmek için kullanılan domain/capability kavramıdır.

Örneğin:

* İlan onaylandı.
* İlan reddedildi.
* Düzeltme istendi.
* Aday önerildi.
* Görüşme planlandı.
* Görüşme tarihi değiştirildi.
* Görüşme sonucu oluştu.
* İşe yerleşme gerçekleşti.
* Eğitim önerildi.

Bildirim kanalları:

* E-posta
* SMS
* Push Notification
* Sistem içi bildirim

olabilir.

---

# 31. Domain Events

Domain içerisindeki önemli durum değişiklikleri domain event olarak ifade edilebilir.

Örnekler:

```text
CandidateCreated
CandidateProfileCompleted
CandidateAssessmentCompleted

EmployerApproved
PersonnelRequestCreated

JobSubmitted
JobApproved
JobRejected
JobPublished

CandidateRecommended
InterviewRequested
InterviewScheduled
InterviewCompleted

EmploymentCreated
EmploymentStarted
EmploymentEnded
```

Domain event ile integration event aynı kavram değildir.

Domain event, domain içerisindeki bir olayın gerçekleştiğini ifade eder.

---

# 32. Temel Domain İlişkileri

Sistemin temel ilişkileri:

```text
User
 ├── Candidate
 ├── Employer
 ├── CareerAdvisor
 └── Admin
```

```text
Candidate
 ├── CandidateProfile
 ├── Skills
 ├── Education
 ├── Experiences
 ├── Certificates
 ├── Assessments
 ├── Applications
 ├── Recommendations
 ├── Interviews
 ├── Employment
 └── Training / Events
```

```text
Employer
 ├── CompanyProfile
 ├── Jobs
 ├── PersonnelRequests
 ├── Recommendations
 ├── Interviews
 └── Employment
```

```text
CareerAdvisor
 ├── AssignedCandidates
 ├── AssignedEmployers
 ├── Assessments
 ├── Recommendations
 ├── Interviews
 └── EmploymentFollowUps
```

---

# 33. Temel Domain Kuralları

Aşağıdaki kurallar domain açısından temel kabul edilir.

## 33.1 Candidate

* Candidate onaylanmadan aktif kariyer süreçlerine dahil edilemez.
* Candidate kendi profilini yönetebilir.
* CareerAdvisor atanmış olduğu Candidate'ın kariyer verilerini inceleyebilir.
* Yetkili kullanıcılar dışında Candidate'ın hassas bilgileri görüntülenemez.

## 33.2 Employer

* Employer onaylanmadan aktif işveren operasyonlarına dahil edilemez.
* Employer onaylanmadan Job yayınlayamaz.
* Employer onaylanmadan PersonnelRequest oluşturamaz veya aktif hale getiremez.

## 33.3 Job

* Job doğrudan yayınlanamaz.
* Job gerekli onay sürecinden geçmelidir.
* Reddedilen Job için mümkünse red nedeni ve düzeltme talebi tutulmalıdır.
* Düzeltilen Job yeniden değerlendirmeye gönderilebilir.

## 33.4 PersonnelRequest

* PersonnelRequest Employer tarafından oluşturulur.
* CareerAdvisor tarafından değerlendirilebilir.
* PersonnelRequest bir Job'a dönüşmek zorunda değildir.
* PersonnelRequest doğrudan Candidate Recommendation sürecini başlatabilir.

## 33.5 Recommendation

* Recommendation belirli bir Candidate ile belirli bir Employer/Job/PersonnelRequest ilişkisini temsil eder.
* Recommendation, Application ile aynı kavram değildir.
* Aynı aday aynı fırsat için tekrar önerilecekse domain kurallarına göre bunun yeni Recommendation mı yoksa mevcut Recommendation güncellemesi mi olduğu belirlenmelidir.

## 33.6 Interview

* Interview geçerli bir aday-firma ilişkisine dayanmalıdır.
* Interview planlanmadan önce gerekli taraflar belirlenmelidir.
* Tamamlanan Interview için sonuç ve mümkünse değerlendirme notu tutulmalıdır.

## 33.7 Employment

* Employment gerçek bir işe yerleşme durumunu temsil eder.
* Employment oluşturulduğunda ilgili Candidate ve Employer ilişkisi takip edilebilir.
* Employment süreci işe başlangıç ve sonrasındaki takip süreçlerini desteklemelidir.

## 33.8 CareerDevelopment

* SkillGap yalnızca CareerAdvisor tarafından oluşturulabilir veya onaylanabilir; aday kendi SkillGap'ini doğrudan belirleyemez.
* DevelopmentPlan, ilgili Candidate'a atanmış CareerAdvisor tarafından oluşturulur.
* TrainingRecommendation, Website.Training verisini kopyalamaz; yalnızca referans (ID/contract) tutar.
* Bir TrainingRecommendation tamamlandığında (aday eğitimi bitirdiğinde) ilgili SkillGap'in durumu güncellenebilir ancak otomatik olarak kapatılmaz — CareerAdvisor onayı gerekebilir.

---

# 34. Domain'de Olmaması Gereken Yaklaşım

Domain aşağıdaki şekilde modellenmemelidir:

```text
Job
 ↓
Application
 ↓
Hire
```

Bu yaklaşım sistemin yalnızca iş ilanı platformu olarak modellenmesine neden olur.

Doğru yaklaşım:

```text
Candidate
   ↓
Profile
   ↓
Assessment
   ↓
Career Development
   ↓
Matching
   ↓
Recommendation
   ↓
Interview
   ↓
Employment
   ↓
Follow-Up
```

Job ve Application bu yapının destekleyici parçalarıdır.

---

# 35. Otomasyon ve Yapay Zeka

Sistem ilerleyen aşamalarda bazı domain kararlarını otomatikleştirebilir.

Örneğin:

### Aday Önerisi

```text
Candidate Profile
      +
Job / PersonnelRequest
      ↓
Matching Engine
      ↓
Candidate Recommendations
```

### Eğitim Önerisi

```text
Candidate Assessment
      +
Skill Gap
      ↓
Training Recommendation
```

### Danışman Atama

```text
Candidates
+
Employers
+
Advisor Workload
      ↓
Assignment Algorithm
      ↓
Advisor Assignment
```

Otomasyon ve yapay zeka sistemleri **domain kararlarının yerine doğrudan geçmemelidir**.

Özellikle adayın işe yönlendirilmesi gibi kritik süreçlerde CareerAdvisor'ın insan kontrolü korunabilir.

---

# 36. Public Web Domain

Gençlik Merkezi'nin public web sitesi de domainin bir parçasıdır ancak kariyer operasyonlarından farklı bir içerik alanına sahiptir.

Public domain içerisinde:

* Corporate Information
* About
* Mission / Vision / Values
* Management / Team
* Activity Reports
* Transparency
* Activities
* Projects
* Events
* Trainings
* Workshops
* Volunteering
* News
* Announcements
* Success Stories
* Documents
* Contact

gibi içerikler bulunabilir.

Bu içerikler kamuya açık olabilir veya yönetim tarafından yayınlanabilir.

---

# 37. Domain Sınırları

Domain içerisindeki kavramlar birbirleriyle ilişkilidir ancak her kavram tek bir sorumluluğa sahip olmalıdır.

Örneğin:

```text
Candidate
```

adayın kimliğini ve kariyer profilini temsil eder.

```text
Assessment
```

adayın değerlendirilmesini temsil eder.

```text
Matching
```

uygunluk ilişkisini temsil eder.

```text
Recommendation
```

adayın bir fırsata yönlendirilmesini temsil eder.

```text
Interview
```

görüşme sürecini temsil eder.

```text
Employment
```

işe yerleşme durumunu temsil eder.

Bu kavramlar birbirinin yerine kullanılmamalıdır.

---

# 38. Domain Terminolojisi

Kod içerisinde domain kavramları mümkün olduğunca aşağıdaki isimlerle ifade edilmelidir:

| İş Kavramı                 | Domain Adı         |
| -------------------------- | ------------------ |
| Aday                       | Candidate          |
| Aday Profili               | CandidateProfile   |
| İşveren                    | Employer           |
| Firma Profili              | CompanyProfile     |
| Kariyer Danışmanı          | CareerAdvisor      |
| İş İlanı                   | Job                |
| Personel İhtiyacı          | PersonnelRequest   |
| Başvuru                    | Application        |
| Eşleştirme                 | Matching           |
| Aday Önerisi / Yönlendirme | Recommendation     |
| Değerlendirme              | Assessment         |
| Beceri                     | Skill              |
| Beceri Eksikliği           | SkillGap           |
| Eğitim                     | Training           |
| Etkinlik                   | Event              |
| Görüşme                    | Interview          |
| İşe Yerleşme               | Employment         |
| İşe Yerleşme Takibi        | EmploymentFollowUp |
| Bildirim                   | Notification       |

Kod içerisinde aynı domain kavramı için farklı isimler kullanılmamalıdır.

Örneğin:

```text
Candidate
```

yerine aynı kavram için:

```text
Applicant
JobSeeker
Member
Person
```

gibi isimler rastgele kullanılmamalıdır.

---

# 39. Domain'in Temel İlkesi

Gençlik Merkezi domaininin merkezinde Job değil **Candidate + CareerAdvisor + Employer ilişkisi** bulunmaktadır.

Temel model:

```text
                    Candidate
                       │
                       │
                       ▼
                CareerAdvisor
                       │
              ┌────────┴────────┐
              │                 │
              ▼                 ▼
          Employer            Training
              │
              ▼
       PersonnelRequest
              │
              ▼
          Matching
              │
              ▼
       Recommendation
              │
              ▼
          Interview
              │
              ▼
         Employment
              │
              ▼
      EmploymentFollowUp
```

Sistemin temel amacı:

> **Doğru genci doğru fırsatla buluşturmak ve bu süreci kariyer danışmanlığı ile sürdürülebilir bir istihdam sürecine dönüştürmektir.**
