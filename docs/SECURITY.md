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

---

# 17. File Upload Security

Media modülü dosya yüklemelerini yönetir.

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
