# SECURITY.md — Gençlik Merkezi Güvenlik Politikası

## 1. Amaç

Bu doküman, Gençlik Merkezi platformunun güvenli geliştirilmesi, işletilmesi ve bakımında uyulması gereken güvenlik kurallarını tanımlar.

Güvenlik yalnızca authentication katmanında ele alınmaz. Uygulama;

* Authentication
* Authorization
* Resource Authorization
* Veri izolasyonu
* KVKK ve kişisel veri güvenliği
* API güvenliği
* Dosya güvenliği
* Secret yönetimi
* Token yaşam döngüsü
* Audit ve güvenlik kayıtları
* Logging güvenliği
* Rate limiting
* Input validation
* Veri saklama ve silme
* Backup ve erişim kontrolü
* Güvenlik testleri

katmanlarının tamamında korunmalıdır.

Temel prensip:

> **Sisteme giriş yapabilmek, sistemdeki her kayda erişebilmek anlamına gelmez.**

---

# 2. Güvenlik Öncelikleri

Güvenlik kararlarında aşağıdaki öncelik sırası uygulanır:

```text
Security
    ↓
Privacy
    ↓
Authorization
    ↓
Data Integrity
    ↓
Availability
    ↓
Performance
```

Güvenlik ile geliştirme hızı arasında seçim yapılması gerektiğinde güvenlik önceliklidir.

---

# 3. Temel Güvenlik Prensipleri

Sistem aşağıdaki prensipleri takip eder:

* Least Privilege
* Defense in Depth
* Secure by Default
* Fail Secure
* Zero Trust
* Explicit Authorization
* Data Minimization
* Separation of Concerns
* Secure Secret Management
* Auditability

Hiçbir güvenlik kontrolü yalnızca frontend'e bırakılmaz.

Frontend üzerindeki:

* route protection
* button hiding
* menu hiding
* UI permission checks

güvenlik mekanizması değildir.

Gerçek authorization her zaman backend tarafından uygulanır.

---

# 4. Authentication

Authentication işlemleri `Identity` modülü tarafından yönetilir.

Authentication kapsamında:

* Registration
* Login
* Logout
* Password management
* Token management
* Account status
* Account lockout
* Password reset
* Session/token lifecycle

kontrolleri uygulanır.

## 4.1 Password Security

Password değerleri hiçbir zaman plaintext olarak saklanmaz.

Password:

* güvenli ve modern password hashing algoritması ile hashlenir,
* reversible encryption kullanılmaz,
* loglanmaz,
* API response içerisinde döndürülmez.

Password policy merkezi olarak tanımlanmalıdır.

Minimum olarak:

* minimum uzunluk,
* yaygın/kolay password kontrolü,
* başarısız login denemesi kontrolü,
* account lockout veya progressive delay

uygulanmalıdır.

---

# 5. Token Security

Authentication token'ları güvenli şekilde yönetilmelidir.

Access token:

* kısa ömürlü olmalıdır,
* yalnızca gerekli claim'leri içermelidir,
* hassas kişisel veri içermemelidir.

Refresh token kullanılıyorsa:

* güvenli şekilde saklanmalı,
* expiration uygulanmalı,
* rotation desteklenmeli,
* revoke edilebilmeli,
* token reuse tespit edilebilmelidir.

Logout işlemi yalnızca frontend'de token silmekten ibaret olmamalıdır.

Kullanıcı hesabı devre dışı bırakıldığında aktif session/token'ların geçersizleştirilmesi desteklenmelidir.

## 5.1 Bilinen Sınır: Deactivate Sonrası Access Token Geçerliliği

Identity modülünün Admin `Deactivate` işlemi (bkz. ARCHITECTURE.md §27.1) refresh token'ları
iptal etmez; yalnızca `User.Status`'u `Disabled` yapar. `Login` ve `RefreshAccessToken` her ikisi
de `Status != Active` kontrolü yaptığından refresh token bir daha kullanılamaz, ancak deactivate
anında kullanıcının elinde hâlâ **süresi dolmamış bir access token** varsa, bu token — JWT
stateless olduğundan — kendi süresi dolana kadar (en fazla 15 dakika, `Jwt:AccessTokenExpirationMinutes`)
geçerli kalmaya devam edebilir.

Bu **bilinen ve bilinçli olarak kabul edilmiş bir sınırdır**. Ek bir revocation mekanizması
(security stamp, token blacklist/deny-list, vb.) şimdilik eklenmemiştir — 15 dakikalık pencere,
mevcut kullanıcı sayısı ve tehdit modeli için kabul edilebilir görülmüştür. Kullanıcı sayısı veya
"anlık erişim kesme" ihtiyacı büyürse, bu mekanizmalardan biri eklenerek pencere kapatılabilir.

---

# 6. Authorization

Authorization iki temel seviyede uygulanır:

```text
Role / Permission Authorization
            +
Resource Authorization
```

## 6.1 Role-Based Authorization

Sistemde temel roller:

* Admin
* Employer
* Candidate
* CareerAdvisor

rolleridir.

Ancak yalnızca role kontrolü yeterli değildir.

Permission bazlı yetkilendirme desteklenmelidir.

Örneğin:

```text
Candidate.Read
Candidate.Update
Candidate.CV.Read
Candidate.CV.Update

Job.Create
Job.Update
Job.Submit
Job.Approve
Job.Reject

Interview.Create
Interview.Read
Interview.Update

Employment.Read
Employment.Update
```

Permission isimleri ilgili modülün sorumluluğuna göre tanımlanmalıdır.

---

# 7. Resource Authorization

Resource authorization sistemin kritik güvenlik katmanlarından biridir.

Kullanıcının belirli bir role sahip olması, tüm kaynaklara erişebileceği anlamına gelmez.

Örneğin:

```text
CareerAdvisor
    ↓
Assigned Candidate
```

Bir kariyer danışmanı yalnızca kendisine atanmış adayların kayıtlarına erişebilmelidir.

Aynı şekilde:

```text
Employer
    ↓
Own Company
    ↓
Own Jobs
    ↓
Own Applications / Recommendations
```

İşveren başka bir şirketin kayıtlarına erişememelidir.

Authorization her request'te ilgili resource üzerinden doğrulanmalıdır.

Örnek:

```text
GET /api/v1/candidates/{candidateId}
```

sadece:

```text
User authenticated
        +
User has Candidate.Read
        +
User is authorized for this candidate
```

şartları sağlanıyorsa başarılı olmalıdır.

---

# 8. Admin Yetkileri

Admin en geniş yetkiye sahip roldür ancak Admin işlemleri de audit edilmelidir.

Özellikle:

* User role değişiklikleri
* Permission değişiklikleri
* Account activation/deactivation
* Candidate approval
* Employer approval
* Job approval
* Data deletion
* Assignment değişiklikleri
* Security configuration değişiklikleri

audit kaydına alınmalıdır.

Admin yetkisi gereksiz yere diğer kullanıcılara verilmemelidir.

---

# 9. Module ve Database Isolation

Proje database-per-module mimarisini kullanır.

Her modül kendi verisinin sahibidir.

Örneğin:

```text
Identity DB
Candidate DB
Employer DB
Job DB
Interview DB
Employment DB
...
```

Bir modül başka bir modülün:

* DbContext'ine,
* Entity'sine,
* repository'sine,
* doğrudan SQL'ine

erişemez.

Cross-module erişim:

* Contract
* ID
* Integration Event
* Application-level communication

üzerinden yapılmalıdır.

Fiziksel cross-database foreign key kullanımından kaçınılmalıdır.

Bu izolasyon yalnızca mimari prensip değil, aynı zamanda güvenlik sınırıdır.

---

# 10. Company / Employer Isolation

Employer kullanıcıları company scope içerisinde izole edilmelidir.

Bir Employer kullanıcısı:

* yalnızca bağlı olduğu şirketi,
* şirketine ait kullanıcıları,
* şirketine ait job kayıtlarını,
* şirketinin aday süreçlerini,
* şirketinin interview kayıtlarını

görebilmelidir.

ID tahmin edilerek başka şirkete ait resource erişilememelidir.

Özellikle aşağıdaki risk kontrol edilmelidir:

```text
GET /api/v1/jobs/123
```

kullanıcı `123` ID'sine sahip job kaydını tahmin etse bile resource authorization başarısız olmalıdır.

---

# 11. Candidate Data Security

Candidate modülü yüksek miktarda kişisel veri barındırabilir.

Bu nedenle:

* yalnızca gerekli veriler toplanmalıdır,
* gereksiz kişisel veri tutulmamalıdır,
* API response'larında yalnızca gerekli alanlar döndürülmelidir,
* kişisel bilgiler loglanmamalıdır,
* erişimler authorization ile korunmalıdır.

CV, iletişim bilgileri, eğitim ve deneyim gibi bilgiler yalnızca gerekli kullanıcıların erişimine açık olmalıdır.

---

# 12. KVKK ve Personal Data Protection

Sistem Türkiye'de faaliyet göstereceği için kişisel veri işleme süreçleri KVKK gereklilikleri dikkate alınarak tasarlanmalıdır.

Teknik mimaride:

* Data minimization
* Purpose limitation
* Access control
* Retention policy
* Secure deletion
* Auditability
* Encryption where appropriate
* Backup protection

uygulanmalıdır.

Kişisel veri içeren tablolar ve alanlar dokümante edilmelidir.

Bir özelliğin geliştirilmesi sırasında:

> "Bu veriye gerçekten ihtiyacımız var mı?"

sorusu sorulmalıdır.

Gereksiz kişisel veri toplanmamalıdır.

## 12.1 Özel Nitelikli Kişisel Veri (Sensitive Personal Data)

KVKK madde 6 kapsamında sağlık verisi, engellilik durumu ve benzeri bilgiler
**özel nitelikli kişisel veri** sayılır ve genel kişisel verilerden daha sıkı
kurallara tabidir.

Bu kapsama giren alanlar (Candidate.md referans alınmıştır):

```text
Engelli Kategorisi
Engellilik Yüzdesi
Engellilik Açıklaması
Sağlık Raporu Bilgisi
Kullanılan İlaç Bilgisi
Kronik Rahatsızlık Bilgisi
Bulaşıcı Hastalık Bilgisi
Bilinç Kaybı Durumu
```

### Zorunlu Kurallar

* Bu veriler **yalnızca** kullanıcı "Engelliyim" seçeneğini işaretlediğinde
  toplanır; varsayılan olarak istenmez.
* Bu verilerin işlenmesi için genel üyelik sözleşmesinden **ayrı, açık ve
  spesifik bir KVKK rızası** alınmalıdır (ayrı checkbox + ayrı aydınlatma metni).
* Rıza metni; verinin hangi amaçla, kimler tarafından, ne kadar süreyle
  işleneceğini açıkça belirtmelidir.
* Kullanıcı rızasını istediği zaman geri çekebilmelidir; geri çekme, ilgili
  alanların silinmesini/anonimleştirilmesini tetikler.

### Depolama ve İzolasyon

* Bu alanlar Candidate modülünün genel profil tablosunda **değil**, ayrı bir
  tabloda (`CandidateDisabilityInfo` benzeri) tutulmalıdır.
* Bu tabloya erişim, Candidate modülü içinde bile ayrı bir yetki seviyesi
  gerektirmelidir — genel `CandidateProfile` okuma yetkisi bu tabloyu
  otomatik olarak kapsamamalıdır.
* Alan seviyesinde şifreleme (encryption at rest) uygulanmalıdır.

### Erişim Kuralları

* Bu veriye erişebilecekler: **yalnızca** adaya atanmış CareerAdvisor ve
  gerekçeli erişimi olan Admin.
* Employer bu veriye **hiçbir koşulda** doğrudan erişemez.
* Bu veri **public web sitesinde, CV export'ta veya işverene gösterilen
  aday özetinde** kesinlikle yer almaz — yalnızca "engelli istihdamı"
  kapsamında genel/istatistiksel amaçla, kişi bazında olmayan biçimde
  kullanılabilir.
* Bu alanlar **default matching/arama sorgularına** dahil edilmez; yalnızca
  adayın kendi rızasıyla "engelli istihdamı" özel eşleştirme akışında
  kullanılır.

### Audit ve Loglama

* Bu tabloya yapılan her `read` işlemi audit log'a kaydedilmelidir
  (kim, ne zaman, hangi amaçla).
* Bu veriler application log'larına, hata mesajlarına veya exception
  trace'lerine **kesinlikle yazılmamalıdır** (bkz. §13 Sensitive Data).

### Saklama Süresi

* Bu verilerin retention süresi, genel candidate verisinden **daha kısa
  ve daha sıkı** tutulmalıdır; rıza geri çekildiğinde veya hesap
  silindiğinde öncelikli olarak temizlenir.

## 12.2 Yurt Dışına Veri Aktarımı (Üçüncü Taraf Servisler)

Bazı üçüncü taraf servisler, çağrı sırasında ziyaretçinin IP adresi, TLS
parmak izi ve User-Agent gibi bilgileri kendi (yurt dışındaki) altyapılarında
işler. Bu projede bilinen örnek: Website modülünün anonim form/etkinlik
kaydı/bülten uçlarını koruyan **Cloudflare Turnstile** (bkz.
`docs/DECISIONS/ADR-024-Website-Module-Design.md` §12.3).

* Bu tür bir entegrasyon eklenirken, işlenen veri türleri ve yurt dışına
  aktarım açıkça tespit edilmeli ve ilgili aydınlatma metnine yazılmalıdır.
* Aktarım için gereken sözleşmesel/hukuki yükümlülükler (KVKK madde 9)
  hukuk danışmanıyla netleştirilmeden prod'a alınmamalıdır.
* Entegrasyon açık/kapalı yapılabilir olmalı ve devre dışı bırakıldığında
  hiçbir veri o servise gönderilmemelidir (Website'de bu,
  `SiteSettings.BotProtectionEnabled` bayrağıyla sağlanır).

## 12.3 Website Başvuruları: Saklama, Arşiv, Anonimleştirme ve Bildirimler

Website modülünün form başvuruları (`FormSubmission`, ADR-024 §12.2) ve bülten aboneleri
(`NewsletterSubscriber`, ADR-024 §14) kişisel veri taşır; saklama süreleri ve bildirim akışı
KVKK'nın data minimization ve purpose limitation ilkelerini uygulayacak şekilde tasarlanmıştır:

* **İki aşamalı saklama (başvurular):** `Completed`/`Rejected` durumuna geçişten **30 gün** sonra
  başvuru otomatik arşivlenir (içerik korunur, yalnızca ana listeden gizlenir) - bu, aktif yönetim
  ekranlarında gereksiz kişisel veri birikmesini önler. Form bazında tanımlı saklama süresi
  (`FormDefinition.RetentionDays`, **varsayılan 730 gün**, saklama süresi durumdan bağımsız işler)
  dolduğunda yanıtlar, iç not metinleri ve başvuran kimliği kalıcı olarak anonimleştirilir; yalnızca
  referans numarası, form, dil, tarih ve durum (istatistik için) kalır. Dosyalar, veritabanı
  transaction'ı commit edildikten **sonra** depolamadan silinir.
* **Varsayılan saklama süresi (730 gün) teyit edilmelidir:** Bu değer ürün/mühendislik kararıdır;
  prod'a çıkmadan önce **hukuk danışmanınca** KVKK madde 7/Kanun'un ilgili saklama süresi
  gerekçeleriyle teyit edilmelidir. Form bazında farklı bir süre gerekiyorsa (ör. üyelik başvurusu
  vs. iletişim formu), `RetentionDays` form bazında ayarlanabilir.
* **Bülten saklama:** `PendingConfirmation` durumunda 7 gün içinde onaylanmayan ve `Unsubscribed`
  durumunda 30 gün'ü geçen abonelik kayıtları kalıcı olarak silinir - onay bekleyen veya aboneliği
  sona eren bir e-posta adresinin süresiz saklanmasını önler.
* **Kişisel veri erişim kaydı:** Bu veriye yapılan her görüntüleme, dosya indirme ve dışa aktarım
  işlemi auditlenir - bkz. §22.1.
* **Bildirim e-postaları kişisel veri/başvuru içeriği taşımaz:** Başvuranın onay e-postası dışında
  (bkz. ADR-024 §12.2), formun yetkililerine gönderilen bildirim e-postası yalnızca form adı,
  referans numarası ve admin paneline bağlantı içerir - başvurunun yanıtları veya başvuranın
  kişisel bilgileri **e-posta gövdesine hiçbir koşulda yazılmaz**. Böylece e-posta sunucusu/gelen
  kutusu, başvuru verisi için ayrı bir sızıntı yüzeyi haline gelmez.
* **Anonim çerez onay kaydı (`CookieConsentRecord`, ADR-024 §13):** IP adresi, User-Agent ve
  kullanıcı kimliği hiçbir koşulda saklanmaz; yalnızca istemcinin ürettiği rastgele `ConsentId`,
  onaylanan kategoriler, politika sürümü ve eylem (`AcceptAll`/`RejectAll`/`Custom`) tutulur. Kayıt,
  kimliklendirilebilir bir ziyaretçi profiliyle eşleştirilemeyecek şekilde tasarlanmıştır ve 3 yıl
  sonra günlük bir job ile silinir.

## 12.4 Website Etkinlik Kayıtları (`EventRegistration`)

Etkinlik kayıtları (`EventRegistration`, ADR-024 §11.2) kişisel veri taşır; saklama ve erişim
kuralları §12.3'teki başvuru/bülten ilkeleriyle aynı yaklaşımı izler:

* **Kişisel veri erişim kaydı:** `PersonalDataAccessLog.EntityType` değerine `EventRegistration`
  eklenmiştir. Kayıt içeren her okuma ucu (admin liste, detay, katılımcı CSV dışa aktarımı)
  erişimi `View`/`Export` olarak auditler; kişisel veri içermeyen sayaç/özet uçları (ör.
  `remainingSpots`, kontenjan özeti) erişim kaydı yazmaz.
* **Saklama/anonimleştirme süresi (365 gün, teyit edilmelidir):** Etkinlik bitiminden **365 gün**
  sonra günlük bir job kayıttaki `FirstName`, `LastName`, `Email`, `Phone`, `UserId` alanlarını
  temizler (`AnonymizedAtUtc`); durum, tarihler ve durum geçmişi istatistik amacıyla kalır. Bu
  süre ürün/mühendislik kararıdır; §12.3'teki 730 günlük başvuru saklama süresi gibi, prod'a
  çıkmadan önce **hukuk danışmanınca** KVKK madde 7 kapsamında teyit edilmelidir. Anonimleştirilmiş
  bir kayıt üzerinde durum değişikliği yapılamaz; iptal ve doğrulama token'ları bu noktadan sonra
  işlevsizdir (satır silinmez, yalnızca etkisizleşir).
* **Doğrulanmamış kayıt temizliği:** Anonim kayıt, e-postası 24 saat içinde doğrulanmazsa saatlik
  bir job ile kalıcı olarak silinir (bu kayıtlar hiçbir kontenjan sayacında yer tutmadığından
  silme sayaçlara dokunmaz).
* **Online link yalnızca onay e-postasında gider:** `EventSchedule.OnlineLink`, `Confirmed`
  durumuna geçen (`Online`/`Hybrid` formatlı) bir kayda gönderilen onay e-postası dışında **hiçbir
  yerde** bulunmaz - ne public API yanıtlarında (liste, detay, `.ics`), ne diğer bildirim
  e-postalarında (başvuru, yedek listesi, iptal bildirimi).
* **Katılımcı listesi hiçbir koşulda public olmaz:** Public uçlar (`GET /api/v1/public/events`,
  içerik detayı, `.ics`) yalnızca hesaplanmış `registrationState` ve `remainingSpots` döner;
  `EventRegistration`'a ait hiçbir alan (ad, e-posta, telefon) public yanıtta yer almaz. Katılımcı
  CSV dışa aktarımı (`Website.Submissions.View`) yalnızca admin ucundadır.
* **CSV enjeksiyonu kaçışı:** Katılımcı dışa aktarımı, bülten dışa aktarımıyla aynı desende
  (UTF-8 BOM + `=`/`+`/`-`/`@` ile başlayan hücrelerin tek tırnakla kaçışlanması) üretilir.
* **`UserId` yalnızca token'dan alınır:** Kayıt uç noktası, istemcinin gönderdiği herhangi bir
  kimliğe güvenmez (AGENTS §26); giriş yapmış kullanıcının kimliği `ICurrentUserContext`'ten
  okunur, istek gövdesinde `UserId` alanı yoktur.

---

# 13. Sensitive Data

Hassas veya kritik veriler mümkün olduğunca sınırlı tutulmalıdır.

Aşağıdaki bilgiler kesinlikle loglanmamalıdır:

* Password
* Password reset token
* Access token
* Refresh token
* API key
* Secret
* Session credential
* Full authentication header

Kişisel veriler de gereksiz şekilde loglanmamalıdır.

---

# 14. API Security

Tüm API endpoint'leri varsayılan olarak güvenli kabul edilmelidir.

Public endpoint açıkça belirtilmelidir.

Örneğin:

```text
Public
    GET /api/v1/public/content

Authenticated
    GET /api/v1/candidates/me

Permission
    POST /api/v1/jobs

Admin
    POST /api/v1/admin/users
```

Yeni bir endpoint geliştirildiğinde authorization gerekip gerekmediği açıkça değerlendirilmelidir.

Authentication gerektiren endpoint'in yanlışlıkla anonymous bırakılması kritik güvenlik hatasıdır.

---

# 15. Input Validation

Tüm external input backend tarafından doğrulanmalıdır.

Frontend validation yalnızca kullanıcı deneyimi içindir.

Backend:

* FluentValidation
* Domain validation
* Type validation
* Length validation
* Range validation
* Format validation

uygulamalıdır.

Kullanıcıdan gelen hiçbir veri güvenilir kabul edilmemelidir.

---

# 16. Injection Protection

Sistem aşağıdaki saldırılara karşı korunmalıdır:

* SQL Injection
* XSS
* Command Injection
* Path Traversal
* Header Injection
* LDAP Injection
* Template Injection

EF Core kullanılırken parametrized query yaklaşımı korunmalıdır.

Raw SQL kullanılması gerekiyorsa güvenli parametrization uygulanmalıdır.

String concatenation ile SQL oluşturulmamalıdır.

## 16.1 Zengin Metin İçerik Güvenliği (HTML Sanitizer)

Kullanıcının (Admin/içerik editörü) zengin metin editöründen girdiği HTML, veritabanına yazılmadan
önce sunucu tarafında sanitize edilmelidir. Frontend'in kendi doğrulaması (editör kısıtlamaları,
istemci tarafı temizleme) bir güvenlik sınırı sayılmaz — AGENTS.md §19, "Never rely on frontend
validation for security or business correctness" ilkesi burada da geçerlidir.

Website modülünün `IHtmlContentSanitizer` portu ve `HtmlSanitizerContentSanitizer` implementasyonu
(ADR-024 §16, Faz 0 Görev 3) bu politikayı uygular:

* **Beyaz liste, kara liste değil.** İzin verilen etiket ve attribute kümesi HtmlSanitizer
  kütüphanesinin kendi (çok daha geniş) varsayılanlarından değil, sıfırdan kurulu, açıkça listelenmiş
  bir kümeden gelir: `p, br, strong, b, em, i, u, s, h2, h3, h4, ul, ol, li, blockquote, a, img,
  figure, figcaption, table, thead, tbody, tr, th, td, hr, iframe` ve `href, title, target, src, alt,
  width, height` attribute'ları.
* **`href` (`<a>`):** yalnızca göreli linkler (kısıtlama yok) veya `http`/`https`/`mailto`/`tel`
  şemalı mutlak URL'ler kabul edilir. `javascript:`/`data:` gibi şemalar reddedilir. Protokolden
  bağımsız (`//evil.com`) href'ler de mutlak URL sayılarak aynı şema kontrolüne tabi tutulur — aksi
  halde tarayıcı bunu sayfanın kendi şemasıyla çözer ve fiilen bir mutlak URL gibi davranır.
  `target="_blank"` olan linklere otomatik `rel="noopener noreferrer"` eklenir.
* **`src` (`<img>`):** yalnızca bu kurulumun kendi medya kütüphanesinden gelen görsellere izin
  verilir — göreli bir yol (yapılandırılmış `PublicRequestPath` altında) veya medya kütüphanesinin
  `GetUrlAsync`'inin döndürdüğü mutlak URL (yapılandırılmış `PublicBaseUrl` ile eşleşen), ki bu ikincisi
  saklanmadan önce göreli yola normalize edilir. Başka bir kaynaktan (izleyici pikseli, başka bir
  siteden hotlink) gelen `src` kabul edilmez; kabul edilmeyen bir `<img>` yalnızca `src`'siz
  bırakılmaz, etiketin tamamı kaldırılır. Normalize edilmiş göreli yol ayrıca yol atlama (path
  traversal) açısından da doğrulanır: `..`, ters bölü (`\`), `?`, `#` karakterlerini veya bunların
  yüzde-kodlanmış (`%2e`, `%2f`, `%5c`) hallerini içeren bir değer reddedilir — hiçbiri bu kurulumun
  üretebileceği bir `fileKey`'de (ADR-019: `{category}/{yyyy}/{MM}/{dd}/{guid}.{ext}`) doğal olarak
  bulunmaz, varlıkları yalnızca elle hazırlanmış bir değere işaret eder. Bugün genel medya kökü
  dışında hiçbir şey servis edilmiyor olsa da (yalnızca `LocalDiskFileStorageService`'in yazdığı/
  okuduğu dizin), sanitize edilmiş bir `src` hiçbir zaman bu kökün dışına çıkabilecek bir yolu
  tanımlamamalıdır.
* **`src` (`<iframe>`):** yalnızca `https://www.youtube-nocookie.com/embed/...` biçimindeki mutlak
  URL'lere izin verilir; başka her `iframe` içeriğiyle birlikte tamamen kaldırılır.
* **Beyaz listede olmayan zararsız sarmalayıcılar** (`div`, `span`, `section`, `article`, `font`,
  ayrıca beyaz listeye eklenmeyen `h1`/`h5`/`h6` başlık seviyeleri) yalnızca kendi etiketini kaybeder;
  içindeki izinli içerik (kendi sanitizasyon geçişinden ayrıca geçmiş olarak) korunur.
* **Tehlikeli etiketler** (`script`, `style`, `noscript`, `template`, `object`, `embed`, `form`,
  `svg`, `math` ve izin verilmeyen `iframe`) içerikleriyle birlikte tamamen kaldırılır — yalnızca
  etiketin kendisi değil. Bu ayrım kütüphanenin `KeepChildNodes` bayrağının global açılmasıyla elde
  edilemez (o zaman `script`/`style` içeriği de açılır ve sayfada görünmez ama devre dışı düz metin
  olarak kalır); bunun yerine yalnızca zararsız sarmalayıcı etiketler için seçici bir unwrap uygulanır.

## 16.2 CSV Enjeksiyonu

Kullanıcı/ziyaretçi girdisinden türeyen veri (ör. bülten abonesinin e-postası) CSV olarak dışa
aktarılırken, Excel/Google Sheets gibi tabloma uygulamalarının bir hücreyi formül olarak
yorumlamasını (CSV/Formula Injection) önlemek için `=`, `+`, `-`, `@` karakterleriyle **başlayan**
her hücre, başına tek bir tırnak (`'`) eklenerek kaçışlanır (bkz. `NewsletterSubscriberCsvFormatter`,
ADR-024 §14). Bu kaçışlama, CSV'nin kendi alan ayracı/tırnaklama kurallarına (virgül veya tırnak
içeren değerlerin RFC 4180 çift tırnaklaması) **ek olarak** uygulanır, onun yerine geçmez. Dosya
UTF-8 BOM ile başlar (Excel'in Türkçe karakterleri doğru göstermesi için) - bu BOM, kaçışlama
kuralını etkilemez.

---

# 17. File Upload Security

Dosya yükleme, `IFileStorage` soyutlaması (AGENTS.md §34) arkasında her modülün kendi verisi için kullandığı ortak bir altyapıdır (ör. Candidate'ın CV dosyaları, Website'in medya kütüphanesi - bkz. §8.11).

Dosya yükleme işlemlerinde:

* Allowed extension kontrolü
* MIME type kontrolü
* File signature kontrolü
* Maximum file size
* Filename normalization
* Path traversal protection
* Malware/virus scanning uygun mimariyle
* Secure storage
* Authorization

uygulanmalıdır.

Kullanıcı tarafından gönderilen filename doğrudan filesystem path olarak kullanılmamalıdır.

Örneğin:

```text
../../../../appsettings.json
```

gibi path traversal girişimleri engellenmelidir.

## 17.1 Website Form Ekleri

Public form gönderimindeki (`POST /api/v1/public/forms/{key}/submissions`, ADR-024 §12.2) dosya
alanları yukarıdaki tüm kontrollere (uzantı, MIME, **dosya imzası** - `FileSignatureValidator`,
boyut) tabidir; alan başına tek dosya, en fazla 10 MB, form tanımında admin tarafından seçilmiş bir
alt kümeyle (`Pdf`/`Docx`/`Jpg`/`Png`) sınırlıdır. Dosyalar, Website'in **public** medya kökünden
tamamen ayrı, özel bir kategoriye (`FileCategory.WebsiteFormAttachment`, §18'deki private kategori
listesine dahildir) yazılır - hiçbir HTTP yolu bu dosyalara anonim erişim vermez, yalnızca
`Website.Submissions.View` yetkisiyle akış (stream) olarak indirilebilir. Dosyalar veritabanı
kaydından **önce** diske yazılır; başvuru kaydı başarısız olursa az önce yazılan dosyalar hemen
silinir (yarım kalmış, veritabanında karşılığı olmayan dosya biriktirilmez).

---

# 18. File Access

CV ve benzeri dosyalar public URL üzerinden doğrudan erişilebilir olmamalıdır.

Dosya erişimi:

```text
Authenticated User
        ↓
Authorization
        ↓
Resource Access Check
        ↓
File Storage
```

akışı üzerinden yapılmalıdır.

Gerektiğinde kısa ömürlü signed URL kullanılabilir.

Dosyanın storage'da bulunması, kullanıcının dosyaya erişme yetkisi olduğu anlamına gelmez.

**Kasıtlı istisna - herkese açık dosyalar:** `FileCategory` her kategoriyi baştan "public" veya "private" olarak işaretler (`FileCategoryExtensions.TryGetIsPublic`, ADR-019 Ek). Yalnızca public kategoriler (ör. Website'in logo/görsel kütüphanesi) ayrı bir kök dizinden statik dosya olarak, kimlik doğrulama olmadan servis edilir; private kategoriler (CV, sözleşme, Website form eki vb.) için hâlâ herhangi bir HTTP yolu yoktur. Bu, yukarıdaki kuralın ihlali değil, kategori bazında baştan tanımlı bir istisnadır - yeni bir `FileCategory` eklerken hangi grupta olduğu bilinçli seçilmelidir.

---

# 19. Secrets Management

Secret değerleri source code içine yazılmamalıdır.

Aşağıdaki bilgiler repository içerisinde tutulamaz:

* Database password
* JWT signing secret
* RabbitMQ credentials
* SMTP credentials
* External API keys
* Storage credentials

Development ortamında uygun local secret mechanism kullanılmalıdır.

Production ortamında secret management çözümü kullanılmalıdır.

Secret değerleri:

* Git'e commit edilmemeli,
* loglanmamalı,
* exception mesajlarına yazılmamalı,
* API response'larında dönmemelidir.

---

# 20. Configuration Security

Configuration ile secret birbirinden ayrılmalıdır.

Environment-specific configuration:

```text
Development
Test
Staging
Production
```

ayrı yönetilmelidir.

Production credential'ları development configuration içerisinde tutulmamalıdır.

---

# 21. Logging Security

Structured logging kullanılmalıdır.

Ancak log sistemi veri sızıntısı kaynağı haline gelmemelidir.

Loglanmaması gereken bilgiler:

```text
Password
Tokens
Secrets
Authorization headers
Full CV contents
Unnecessary personal information
```

Exception loglarında request body yalnızca gerçekten gerekli olduğu durumlarda ve sensitive-data filtering uygulanarak kullanılmalıdır.

---

# 22. Audit Logging

Security-sensitive ve business-critical işlemler audit edilmelidir.

Örnek:

```text
UserLogin
UserLogout
LoginFailed
PasswordChanged
RoleChanged
PermissionChanged
CandidateApproved
EmployerApproved
JobApproved
JobRejected
AssignmentChanged
InterviewStatusChanged
EmploymentCreated
EmploymentUpdated
DataDeleted
```

Audit kaydı mümkün olduğunca:

```text
Who
What
When
Where
Target
Result
```

bilgilerini içermelidir.

Audit kayıtları normal application loglarından ayrı düşünülmelidir.

## 22.1 Website Kişisel Veri Erişim Kaydı (`PersonalDataAccessLog`)

Identity'deki merkezi audit log diğer modüllerce kullanılamadığı için (bkz. AGENTS.md §9 - modüller
arası doğrudan veritabanı/tablo erişimi yasak), Website modülü kişisel veri içeren kayıtlara erişimi
kendi `PersonalDataAccessLog` tablosunda tutar (ADR-024 §12.2): `UserId`, `AccessedAtUtc`,
`EntityType` (`FormSubmission`, `NewsletterSubscriber`, `EventRegistration` - Faz 4 Görev 4),
`EntityId` (dışa aktarmada `null`), `Action` (`View`, `DownloadFile`, `Export`), opsiyonel `Detail`.

* Kişisel veri içeren **her okuma ucu** (başvuru detayı, dosya indirme, abone listesi, CSV dışa
  aktarım) bu kaydı yazar; kişisel veri **içermeyen** liste uçları (ör. başvuru özet listesi -
  referans numarası, form, durum, tarih; kişisel veri yok) erişim kaydı yazmaz.
* Yazma işlemi, sorgunun kendisi değil **ayrı bir komuttur** - AGENTS.md §13 ("bir sorgu yazmaz")
  gereği, okuma isteğini işleyen handler sonucu döndürdükten sonra bu komutu ayrıca gönderir.
* Kayıt yönetim ucu (`GET /api/v1/admin/website/personal-data-access-log`) `Website.Settings.Manage`
  policy'siyle sınırlıdır - erişim kaydının kendisi de hassas bir veridir (kimin hangi kişisel veriye
  ne zaman eriştiğini gösterir).

---

# 23. Rate Limiting

Özellikle public ve authentication endpoint'lerinde rate limiting uygulanmalıdır.

Öncelikli endpoint grupları:

```text
Login
Register
Password Reset
Token Refresh
Public Search
File Upload
Contact Forms
```

Brute-force saldırılarını azaltmak için login ve password reset işlemlerinde ek koruma uygulanmalıdır.

Rate limit değerleri configuration üzerinden yönetilebilir olmalıdır.

## 23.1 Public-Read Rate Limiting ve Reverse Proxy Güveni

Website modülünün anonim public okuma endpoint'leri (`GET /api/v1/public/contents`,
`/api/v1/public/contents/{id}`, `/api/v1/public/routes/resolve`, `/api/v1/public/videos`,
`/api/v1/public/preview/{token}` vb.) `public-read` rate limiting policy'sine tabidir: istemci
IP'sine göre partitioned, sabit pencereli (1 dakika) bir limiter (`Program.cs`). Limit
(`RateLimiting:PublicReadPermitLimit`) configuration üzerinden yönetilir; bir sayfa görüntülemesinin
birden çok public endpoint'e fan-out olabileceği göz önünde bulundurularak `authenticated`
policy'sinden daha yüksek tutulur.

Bu partitioning'in doğru çalışması, `HttpContext.Connection.RemoteIpAddress`'in **gerçek** ziyaretçi
IP'si olmasına bağlıdır. `ReverseProxySettings` (`ReverseProxy:Enabled`, `SectionName = "ReverseProxy"`)
bu varsayımı korur:

* **Varsayılan: `Enabled = false`.** Bu projenin canlı ortamı (Turhost/IIS) reverse proxy olmadan
  doğrudan çalışır — `RemoteIpAddress` zaten gerçek ziyaretçi IP'sidir, `X-Forwarded-For` header'ı
  hiç işlenmez/güvenilmez.
* **Yalnızca bir proxy/CDN katmanı (ör. Cloudflare) eklenirse `Enabled = true` yapılır** ve
  `KnownProxies`/`KnownNetworks` **yalnızca o sağlayıcının kendi IP adresi/ağ aralıklarıyla**
  doldurulur (`ReverseProxyForwardedHeadersOptionsFactory`). `Enabled = true` iken bu liste boş
  bırakılamaz — Program.cs başlangıçta bunu doğrular.
* Bu iki alan birlikte açık olmadan `X-Forwarded-For` güvenilmemelidir: aksi halde herhangi bir
  istemci bu header'ı göndererek kendi IP'sini sahteleyip hem `public-read` rate limitini hem de
  IP bazlı diğer kontrolleri (ör. audit kayıtlarındaki IP alanı) atlatabilir.
* `UseForwardedHeaders` middleware'i, açıkken, rate limiting dahil IP'ye bakan her şeyden **önce**
  çalışacak şekilde en erken middleware olarak eklenir.

## 23.2 Önizleme Linkleri

Website modülünün içerik önizleme linkleri (`POST .../preview-links` ile üretilir, `GET
/api/v1/public/preview/{token}` ile açılır — ADR-024 §4.5) yayınlanmamış (Draft/Unpublished/Archived)
içeriği anonim bir bağlantıyla gösterdiği için şu üç kural birlikte uygulanır:

* **İmzalı ve süreli:** Token, ASP.NET Core Data Protection ile (`ITimeLimitedDataProtector`)
  imzalanır ve süresi dolar; süresi dolmuş veya üzerinde oynanmış (tampered) bir token, hangi
  sebeple geçersiz olduğu ayırt edilmeden aynı `NotFound` yanıtını döner — anonim çağırana hangi
  başarısızlık türü olduğunu sızdırmamak için.
* **`noindex`:** Yanıt `X-Robots-Tag: noindex, nofollow` header'ıyla döner; arama motorları
  taslak içeriği indekslemez.
* **`no-store`:** Yanıt `Cache-Control: no-store` header'ıyla döner; ne tarayıcı ne de aradaki
  bir proxy önbelleğe almaz — link süresi dolduktan sonra bile eski bir önbellek kopyasından
  içerik sızmaz.

Data Protection anahtarları `App_Data/dataprotection-keys` altında dosya sistemine kalıcı olarak
yazılır (varsayılan bellek-içi/registry saklama, bir IIS application pool recycle'ında tüm açık
önizleme linklerini geçersiz kılardı). Bu klasör:

* **web sitesi genel köküne (`webuploads`/public static-file root'una) açılmamalıdır** — yalnızca
  bu klasörün kendisi, arka planda dosya sisteminde durur; hiçbir HTTP yolu bu dizine işaret etmez.
* **asla repoya girmemelidir** — `.gitignore`'da `**/App_Data/` ile hariç tutulur; bu klasör
  yalnızca çalışan sunucuda bulunur, kaynak kontrolünde değil.
* **yedeklenmelidir** — kaybolması, o ana kadar üretilmiş tüm önizleme linklerini (süreleri
  dolmadan) geçersiz kılar; bu veri kaybı bir güvenlik olayı değildir ama operasyonel bir
  sürekliliği bozar.
* Anahtarların kendisi `Secrets Management` (§19) kapsamında değerlendirilmelidir: bu klasöre
  yetkisiz dosya sistemi erişimi, geçmişte üretilmiş tüm önizleme token'larının taklit
  edilebilmesi anlamına gelir.
* **Windows'ta (IIS/on-prem dağıtım hedefi) diskte DPAPI ile şifreli tutulur**
  (`ProtectKeysWithDpapi()`, `Testing` ortamı hariç) — anahtar dosyası diskten çalınsa bile, aynı
  makine/kullanıcı profili dışında deşifre edilemez. Bunun çalışması için **IIS application
  pool'unda "Load User Profile" `True` olmalıdır**; aksi halde pool her recycle'da DPAPI'nin
  bağlı olduğu kullanıcı profilini kaybeder ve önceki anahtarlar okunamaz hale gelir.
* Geliştirme ortamındaki bir test çalıştırması, şifrelenmemiş bir anahtar dosyasını ve 658 test
  yükleme dosyasını yanlışlıkla public repoya commit etti (bkz. `chore: stop tracking App_Data and
  ignore runtime files`); sızan anahtar **yenilendi** (yerel `dataprotection-keys` klasörü
  silindi — uygulama ilk açılışta yeni bir anahtar üretir). Entegrasyon testleri artık bu klasöre
  hiç yazmıyor, kendi geçici dizinlerini kullanıyor (bkz. `test: write test uploads and data
  protection keys to temp directories`).

## 23.3 YouTube Küçük Resmi ve Ziyaretçi IP'si

Website video kütüphanesi, kapak görseli tanımlanmamış bir video için YouTube'un kendi küçük resim
adresini (`https://i.ytimg.com/vi/{videoId}/hqdefault.jpg`) döner (bkz. ADR-024 §5 — oynatma zaten
`youtube-nocookie.com` üzerinden yapılır, ancak küçük resim ayrı, doğrudan `i.ytimg.com`'a giden bir
istektir). Bu istek tarayıcıdan doğrudan Google'ın altyapısına gider ve ziyaretçinin IP adresini
(ve User-Agent'ını) Google'a iletir — `youtube-nocookie` embed'in kendisinin azalttığı türden bir
veri paylaşımı, küçük resim için geçerli değildir.

* Bu bilgi, KVKK madde 9 (yurt dışına veri aktarımı) kapsamında **aydınlatma metnine eklenmelidir**
  (bkz. §12.2 — Turnstile için uygulanan yaklaşımla aynı ilke).
* Frontend, bu küçük resmi **çerez/üçüncü taraf içerik tercihine bağlı olarak** yüklemelidir —
  `SiteSettings` script yönetimindeki `Marketing`/`Analytics` onay mekanizmasına benzer şekilde,
  ziyaretçi onayı olmadan otomatik yüklenmemelidir.
* **Kapak görseli (cover image) tanımlı olan videolarda küçük resim hiçbir zaman kullanılmamalıdır**
  — bu durumda zaten kendi medya kütüphanesinden servis edilen bir görsel vardır, YouTube'a hiç
  istek atılmasına gerek yoktur.

## 23.4 Public Employer (İlan/Firma) Endpoint'leri

Employer modülünün anonim public uçları (`GET /api/v1/public/jobs`, `/api/v1/public/jobs/{slug}`,
`/api/v1/public/companies/{id}`, `/api/v1/public/companies/{id}/logo` — ADR-023 §6) diğer public
okuma uçlarıyla aynı `public-read` rate limiting policy'sine (§23.1) tabidir.

* **Kişisel veri ve iç alan sızdırmama:** Hiçbir public yanıt firmanın iletişim bilgisini
  (`ContactFirstName/LastName/Email/Phone`), vergi bilgisini (`TaxOfficeId`/`TaxNumber`), atanmış
  danışmanını veya ilanın inceleme/ret/revizyon notlarını içermez. İlan yanıtlarında **cinsiyet
  tercihi hiçbir zaman yer almaz** — bu alan yalnızca danışman/aday eşleştirmesinde kullanılır ve
  public'e açılması ayrımcılık riski taşır (hukuki teyit gerektiren bir konu olarak kasıtlı dışarıda
  bırakılmıştır). Askerlik durumu tercihi yalnızca ilan **detayında** bulunur, listede ve arama
  adaptörünün (`IPublishedJobModuleContract`) özetinde yer almaz.
* **Onay zinciri her istekte yeniden doğrulanır:** Bir ilan yalnızca kendisi `Published` **ve**
  firması `Approved` ise public yanıta girer. Firma sonradan reddedilir/pasife alınırsa veya ilan
  idari olarak askıya alınırsa (`AdminSuspendJob`), ilan bir sonraki public istekte otomatik olarak
  kaybolur — ayrı bir senkronizasyon/temizlik adımına gerek yoktur, çünkü sorgu bu iki durumu her
  seferinde canlı veriden kontrol eder.
* **Firma logosu yalnızca açık onayla servis edilir:** `Company.ShowLogoOnWebsite` varsayılan
  `false`'dur ve logo yüklenmeden `true` yapılamaz (`Company.LogoRequired`). Public logo ucu,
  firma onaylı **ve** `ShowLogoOnWebsite = true` olmadıkça dosyayı döndürmez; dosya yolu istemciden
  gelmez, sunucu tarafında firma kaydından çözülür.
* **`404` tekdüzeliği:** Bulunamayan, yayında olmayan veya firması onaysız bir ilan/firma için tüm
  public uçlar **aynı** `404` yanıtını döner; hangi sebeple erişilemediği (yok/taslak/askıda/firma
  reddedilmiş) anonim çağırana sızdırılmaz — aynı ilke önizleme linklerinde (§23.2) ve etkinlik
  kaydında (§12.4) de uygulanır.
* **`SiteSettings.PublicJobListingsEnabled` bir güvenlik kontrolü değildir:** bu bayrak yalnızca
  Website'in frontend yanıtını ve arama adaptörünü kısıtlar; Employer'ın public uçları bayraktan
  bağımsız olarak her zaman yukarıdaki kurallara göre çalışır.

## 23.5 Genel Arama, Sitemap/Robots ve JSON-LD (ADR-024 §10, §15 — Faz 5)

* **Aramada kişisel veri yoktur ve sorgu metni saklanmaz:** `SearchDocument` yalnızca public içerik
  (Website içeriği, yayındaki ilan özetleri) için üretilir; form başvurusu, kayıt, bülten abonesi,
  firma iç kaydı veya danışman notu tasarım gereği hiçbir zaman indekse girmez. `GET
  /api/v1/public/search`'ün `q` parametresi hiçbir yerde loglanmaz veya veritabanına yazılmaz —
  popüler arama/analitik bu nedenle **kasıtlı olarak yoktur** (kişisel veri ihtimali). Yanıt
  sunucu tarafında cache'lenmez.
* **Arama kaynağı hata mesajları kırpılır:** `SearchSourceState.LastError` en fazla 500 karakter
  ve yalnızca istisna mesajıdır — yığın izi (stack trace) hiçbir zaman saklanmaz/döndürülmez.
* **`SiteSettings.AllowSearchEngineIndexing` ve bakım modu bir erişim kontrolü değildir:** bu
  bayraklar yalnızca `/sitemap.xml` ve `/robots.txt`'in arama motorlarına verdiği sinyali
  değiştirir; `false`/bakım modunda bile içerik uçları (`/api/v1/public/contents/...`) normal
  yetkilendirme kurallarına göre erişilebilir kalır — "indekslenmesin" ile "erişilemesin" ayrı
  kavramlardır.
* **JSON-LD'de `OnlineLink` hiçbir koşulda yer almaz:** Etkinlik `jsonLd` çıktısı (`Event`),
  Faz 4'teki kuralla (bkz. §12.4) tutarlı olarak katılımcıya özel online bağlantıyı hiçbir zaman
  içermez; salt online etkinlikte `VirtualLocation.url` etkinliğin kendi public sayfasıdır, link
  değil. Bu invariant bir regresyon testiyle korunur.
* **Frontend JSON-LD'yi basarken `<` karakterini kaçışlamalıdır:** Backend `jsonLd` alanını
  düz JSON olarak döner; frontend bunu `<script type="application/ld+json">` içine **doğrudan**
  basarsa, içerik içinde `</script>` benzeri bir dizi (ör. bir içerik başlığında geçen `</script>`
  metni) script etiketini kırıp XSS'e yol açabilir. Frontend, basmadan önce `<` karakterini
  `<` olarak kaçışlamalıdır (JSON string değerleri içinde geçerli bir kaçıştır, JSON-LD
  semantiğini değiştirmez).

## 23.6 İçerik Revizyon Geçmişi (ADR-024 §4.6 — Faz 5)

* **Anlık görüntüler yalnızca editöre görünen içerik alanlarını taşır:** `ContentItemRevision.
  SnapshotJson` başlık, özet, gövde, SEO alanları, etiket/kategori kimlikleriyle sınırlıdır — hiçbir
  kişisel veri (başvuru, kayıt, abone bilgisi) içermez; kapsam bilinçli olarak `PageLayout`,
  `SiteSettings` ve `LegalDocument`'ı (zaten kendi sürümlemesi var) dışarıda bırakır.
  Görüntüleme/karşılaştırma/geri yükleme `Website.Content.Manage` yetkisine bağlıdır.
  Başvuru/etkinlik kaydı/abone görüntülemesinin aksine (`Website.Submissions.View`, §12.3/§22.1),
  revizyon erişimi **auditlenmez** — içerdiği veri zaten editör tarafından yazılmış, kişisel veri
  içermeyen yayın içeriğidir.
* **Geri yüklemede gövde yeniden sanitize edilir:** `RestoreContentItemRevisionCommandHandler`,
  anlık görüntüdeki `Body` HTML'ini doğrudan yazmaz — mevcut `IHtmlContentSanitizer` ile (§16)
  yeniden geçirir; böylece eski bir revizyonun kaydedildiği andaki sanitizer politikasıyla değil,
  **güncel** beyaz liste politikasıyla tutarlı kalır.
* **Geri yükleme slug/durum/yolu değiştirmez:** bir revizyonu geri yüklemek içeriği yeniden
  yayından kaldırmaz veya URL'sini değiştirmez; yalnızca seçilen dilin metin/SEO/etiket alanlarını
  günceller ve yeni bir `Restored` revizyonu üretir.

---

# 24. CORS

CORS yalnızca ihtiyaç duyulan frontend origin'lerine izin verecek şekilde yapılandırılmalıdır.

Development ortamında kullanılan geniş wildcard ayarlar production'a taşınmamalıdır.

Production'da mümkün olduğunca explicit origin listesi kullanılmalıdır.

---

# 25. HTTPS

Production ortamında tüm authentication ve kişisel veri trafiği HTTPS üzerinden gerçekleştirilmelidir.

HTTP üzerinden hassas veri aktarımına izin verilmemelidir.

Secure cookie kullanılıyorsa:

* Secure
* HttpOnly
* SameSite

ayarları kullanım senaryosuna göre doğru yapılandırılmalıdır.

---

# 26. CSRF

Authentication yöntemi cookie tabanlı olduğu durumda CSRF koruması uygulanmalıdır.

Bearer token tabanlı API mimarisinde CSRF riski farklı değerlendirilmelidir.

Authentication mekanizması değiştirildiğinde CSRF değerlendirmesi yeniden yapılmalıdır.

---

# 27. Error Handling

Production ortamında kullanıcıya:

* Stack trace
* Internal exception details
* SQL exception
* File path
* Server configuration
* Secret/configuration information

gösterilmemelidir.

API standart `ProblemDetails` formatını kullanmalıdır.

Kullanıcıya güvenli bir hata mesajı döndürülürken detaylar server-side log/audit mekanizmasında tutulmalıdır.

---

# 28. Account Lifecycle

User hesabının yaşam döngüsü kontrollü olmalıdır.

Örneğin:

```text
Pending
Active
Suspended
Locked
Disabled
Deleted
```

gibi durumlar gerektiğinde kullanılabilir.

Disabled veya suspended kullanıcı authentication başarılı olsa dahi uygulama kaynaklarına erişememelidir.

---

# 29. Authorization Cache

Permission veya role bilgileri cache'lendiğinde cache invalidation dikkate alınmalıdır.

Örneğin:

```text
Admin
    ↓
Permission Removed
```

permission kaldırıldıktan sonra eski cache nedeniyle kullanıcının yetkili kalmasına izin verilmemelidir.

Security-sensitive cache verileri için uygun expiration ve invalidation stratejisi uygulanmalıdır.

---

# 30. Data Retention

Kişisel veriler gereğinden uzun süre tutulmamalıdır.

Retention politikaları ilgili veri türüne göre tanımlanmalıdır.

Örneğin:

```text
Active Candidate Data
Archived Candidate Data
Interview Records
Employment Records
Audit Records
Notification Records
Media Files
```

için farklı retention politikaları gerekebilir.

Silme işlemleri:

* authorization kontrollü,
* audit edilebilir,
* geri dönüşü olmayan veri kaybına karşı kontrollü

olmalıdır.

Production verisi üzerinde doğrudan manuel deletion yapılmamalıdır.

---

# 31. Backup Security

Backup'lar da production verisi kadar korunmalıdır.

Backup:

* yetkisiz erişime karşı korunmalı,
* şifrelenmeli,
* erişim yetkileri sınırlandırılmalı,
* retention policy'ye sahip olmalı,
* restore işlemleri test edilmelidir.

Backup bulunması tek başına yeterli değildir.

Restore edilemeyen backup güvenilir backup olarak kabul edilmez.

---

# 32. Dependency Security

NuGet paketleri ve diğer dependency'ler gereksiz yere eklenmemelidir.

Yeni dependency eklenmeden önce:

* gerçekten gerekli mi?
* güvenilir mi?
* aktif olarak destekleniyor mu?
* bilinen güvenlik açığı var mı?
* mevcut mimariyle uyumlu mu?

değerlendirilmelidir.

Deprecated veya güvenlik riski bulunan dependency kullanılmamalıdır.

---

# 33. Security Headers

Production web/API altyapısında uygun security headers değerlendirilmelidir.

İhtiyaca göre:

* Content-Security-Policy
* X-Content-Type-Options
* Referrer-Policy
* Strict-Transport-Security
* Frame protection

gibi mekanizmalar kullanılmalıdır.

Header'lar frontend veya reverse proxy mimarisiyle birlikte değerlendirilmelidir.

---

# 34. Database Security

Database kullanıcıları minimum gerekli yetkiye sahip olmalıdır.

Application'ın kullandığı database credentials:

* yalnızca gerekli database'e,
* yalnızca gerekli işlemlere

erişebilmelidir.

Development ve production database credential'ları kesinlikle ayrılmalıdır.

Migration ve administrative database yetkileri application runtime hesabından mümkün olduğunca ayrılmalıdır.

---

# 35. Cross-Module Security

Bir modül başka modülün verisine erişirken yalnızca teknik erişim kontrolü yeterli değildir.

Örneğin:

```text
CareerAdvisor
    ↓
Candidate
```

erişiminde:

```text
Authentication
+
Permission
+
Assignment
+
Business Rule
```

birlikte değerlendirilmelidir.

Cross-module event veya contract üzerinden gelen veriler de güvenilir kabul edilmeden önce doğrulanmalıdır.

---

# 36. Event ve RabbitMQ Security

RabbitMQ bağlantıları credential ile korunmalıdır.

Production ortamında:

* authenticated connections,
* uygun network isolation,
* minimum permission,
* güvenli connection configuration

kullanılmalıdır.

Event payload'larında gereksiz kişisel veri taşınmamalıdır.

Örneğin event:

```text
CandidateAssignedToAdvisor
```

için tüm candidate profile yerine gerekli ID ve minimum metadata taşınmalıdır.

---

# 37. Hangfire Security

Hangfire dashboard production ortamında public bırakılmamalıdır.

Dashboard erişimi:

```text
Authentication
+
Authorization
```

ile korunmalıdır.

Hangfire job'larında secret veya hassas kişisel veri loglanmamalıdır.

---

# 38. External Integrations

Harici servislerle entegrasyonlarda:

* API credentials güvenli tutulmalı,
* timeout uygulanmalı,
* retry kontrollü kullanılmalı,
* external response doğrulanmalı,
* sensitive data minimum tutulmalı,
* failure durumları güvenli yönetilmelidir.

Harici servisten gelen veri güvenilir kabul edilmemelidir.

---

# 39. Security Testing

Security testleri yalnızca penetration test aşamasına bırakılmamalıdır.

Test seviyeleri:

```text
Unit Tests
Integration Tests
Architecture Tests
Authorization Tests
API Security Tests
Dependency Scanning
Penetration Testing
```

özellikle authorization testleri kritik kabul edilir.

Örnek:

```text
Employer A
    ↓
GET Job belonging to Employer B
    ↓
403 Forbidden
```

ve:

```text
CareerAdvisor A
    ↓
GET Candidate assigned to Advisor B
    ↓
403 Forbidden
```

senaryoları test edilmelidir.

---

# 40. Security Test Matrix

Her protected resource için mümkün olduğunca şu senaryolar test edilmelidir:

```text
Anonymous
Authenticated without permission
Authenticated with permission
Correct resource owner
Wrong resource owner
Correct advisor assignment
Wrong advisor assignment
Admin
Disabled user
Expired token
Revoked token
```

---

# 41. Security Incident

Güvenlik ihlali şüphesinde:

1. Etkilenen erişim belirlenir.
2. Gerekirse ilgili account/session/token devre dışı bırakılır.
3. Audit ve security logları incelenir.
4. Etkilenen resource belirlenir.
5. Gerekirse credentials rotate edilir.
6. Açığın kaynağı tespit edilir.
7. Düzeltme uygulanır.
8. Test edilir.
9. Olay dokümante edilir.

Incident response sırasında production verisi üzerinde kontrolsüz değişiklik yapılmamalıdır.

---

# 42. Secure Development Rules

Claude veya geliştirici aşağıdaki davranışları gerçekleştiremez:

* Authorization kontrolünü frontend'e bırakmak
* Başka modülün DbContext'ine erişmek
* Secret'ı source code'a yazmak
* Password/token loglamak
* Kullanıcı input'una güvenmek
* Raw SQL'i güvenli olmayan şekilde oluşturmak
* Public dosya URL'si ile private CV sunmak
* Resource ownership kontrolünü atlamak
* Production verisini doğrudan değiştirmek
* Güvenlik kontrolünü devre dışı bırakmak
* Security-sensitive davranışı dokümante etmeden değiştirmek

---

# 43. Security Change Protocol

Aşağıdaki değişiklikler yapılmadan önce güvenlik etkisi değerlendirilmelidir:

* Authentication değişikliği
* Authorization değişikliği
* Role/permission değişikliği
* Token mekanizması değişikliği
* Personal data modeli değişikliği
* File upload değişikliği
* Public endpoint eklenmesi
* Cross-module data access
* Database permission değişikliği
* External integration eklenmesi
* Secret/configuration değişikliği

Belirsiz veya yüksek riskli değişikliklerde Claude uygulamaya geçmeden önce durmalı ve açıklama istemelidir.

---

# 44. Security Checklist

Yeni bir feature tamamlanmadan önce:

```text
[ ] Authentication gereksinimi değerlendirildi
[ ] Permission tanımlandı
[ ] Resource authorization uygulandı
[ ] Ownership/assignment kontrol edildi
[ ] Input validation yapıldı
[ ] Sensitive data exposure kontrol edildi
[ ] Logging kontrol edildi
[ ] Audit gereksinimi değerlendirildi
[ ] Rate limiting gereksinimi değerlendirildi
[ ] File access güvenliği kontrol edildi
[ ] Cross-module access kontrol edildi
[ ] Secrets kontrol edildi
[ ] Error handling kontrol edildi
[ ] Security tests yazıldı
[ ] Architecture boundaries kontrol edildi
```

---

# 45. Golden Security Rule

Gençlik Merkezi için güvenliğin temel kuralı:

> **Kimlik doğrulama erişimin başlangıcıdır; yetkilendirme erişimin sınırıdır.**

Ve ikinci temel kural:

> **Bir kullanıcı bir resource ID'sini biliyor diye o resource'a erişme hakkına sahip değildir.**

Son olarak:

> **Güvenlik frontend'de değil, backend'de uygulanır. Frontend yalnızca güvenlik kurallarının kullanıcı arayüzündeki yansımasıdır.**
