# ADR-018: Candidate Modülü — Aggregate Tasarımı, Kayıt Orkestrasyonu ve Profil Tamamlanma Read-Model'i

## Durum
Kabul edildi

## Bağlam

Candidate modülü, iş arayan gençlerin CV bilgilerini yönetmesini sağlayacak. Kaynak gereksinim dokümanı `Candidate.md`, aşağıdaki bölümleri içeriyor: İletişim Bilgileri, Kişisel Bilgiler (+ engellilik alt-bloğu), Özet, Deneyim Bilgileri, Eğitim Bilgileri, Bilgisayar Bilgileri, Dil Bilgileri, Sertifika Bilgileri, Referans Bilgileri, Hobiler.

Tasarım sürecinde şu sorular netleştirildi:

1. **Identity.User ile CV bilgisinin ilişkisi.** Identity.User şu an sadece login bilgisi (email + şifre) tutuyor; Ad/Soyad/Telefon alanları yok. Bu ADR kapsamında Identity.User'a `FirstName`, `LastName`, `PhoneNumber` eklenmesi kararlaştırıldı (bkz. "İlgili Görev" bölümü — küçük ölçekli olduğu için ayrı ADR açılmadı).
2. **CV'deki İletişim Bilgileri (Ad/Soyad/Email/Telefon), User'daki bilgiyle aynı kaynağı mı paylaşmalı?** Hayır — aday, iş başvurusu için User hesabından farklı bir ad-soyad-email-telefon kullanmak isteyebilir (ör. Identity'de kişisel telefonunu paylaşmak istemeyip CV'de iş telefonunu göstermek gibi). Kayıt anında User'daki bilgiler CV'ye **bir kerelik seed** edilir, sonrasında bağımsız olarak düzenlenebilir. Senkron değildir.
3. **Aggregate sınırları.** CV içeriği zamanla büyüyen, sık güncellenen collection'lar (deneyim, eğitim, dil, sertifika, referans) içeriyor; buna karşılık İletişim/Kişisel Bilgiler bölümü nispeten durağan ve liste/arama ekranlarında gösterilecek "header" niteliğinde. Tek aggregate'te birleştirmek, liste sorgularında gereksiz ağır collection'ların yüklenmesine ve concurrency çakışmalarının artmasına yol açar.
4. **Profil tamamlanma yüzdesi.** Hem header hem içerik alanlarına bağlı bir hesap olduğu için tek bir aggregate'in computed property'si olamaz. Query-time hesaplama (her istekte iki aggregate okuyup hesaplama) yerine, mevcut Outbox/RabbitMQ altyapısı (Notification modülünde zaten kurulu) kullanılarak senkronize edilen bir **read-model** olarak kurulmasına karar verildi.
5. **Firma adı autocomplete ve okul adı lookup.** Firma adı autocomplete'i Employer modülü henüz yokken kurulamaz — ileride bir "firma eşleştirme/linkleme" iyileştirmesi olarak ertelendi, şimdilik serbest metin. Okul adı ise ReferenceData'da bir `School` lookup'u (admin-managed + kullanıcı serbest metin fallback) olarak ele alınacak.
6. **CareerAdvisor ataması.** CareerAdvisor modülü henüz yok; ilgili alan nullable/deferred bırakılıyor.

## Karar

### 1. Aggregate ayrımı: CandidateCv + CandidateCvContent

**`CandidateCv`** (header aggregate — nadiren değişen, liste/arama ekranlarında kullanılan alanlar):
- `Id`, `UserId` (Identity.User'a 1-1 FK)
- İletişim Bilgileri: FotoğrafUrl (IFileStorageService referansı), Ad, Soyad, Email, Telefon, ÜlkeId, İlId, İlçeId, Adres, Sosyal medya linkleri (koleksiyon: Platform + Url)
- Kişisel Bilgiler: Ünvan, Cinsiyet, DoğumTarihi, SürücüBelgesi, UyrukId, NetMaaşBeklentisi, AskerlikDurumu
- `DisabilityInfo?` — nullable owned value object: Kategori, Yüzde, Açıklama, SağlıkRaporuVarMı, İlaçKullanımıVarMı, KronikRahatsızlıkVarMı, BulaşıcıHastalıkVarMı, BilinçKaybıDurumu
- `CareerAdvisorId` (nullable, CareerAdvisor modülü gelene kadar deferred)
- `CompletionPercentage` (read-model tarafından güncellenen denormalize alan)

**`CandidateCvContent`** (detay aggregate — sık güncellenen, collection ağırlıklı; `CandidateCv.Id`'ye FK):
- Özet (metin)
- Deneyim (koleksiyon): FirmaAdı (serbest metin — bkz. Karar §5), PozisyonId, BaşlangıçTarihi, BitişTarihi, HalenÇalışıyorum, SektörId, İşAlanıId, ÇalışmaŞekliId, ÜlkeId, ŞehirId, İşTanımı
- Eğitim (koleksiyon): EğitimDurumuId, BaşlangıçTarihi, DevamEdiyorum/Terk, BitişTarihi, DiplomaNotSistemiId, DiplomaNotu, OkulAdı (School lookup + serbest metin fallback — bkz. Karar §5), Şehir, Açıklama
- BilgisayarBilgisi (metin)
- Diller (koleksiyon): DilId, SeviyeId, AnadilMi
- Sertifikalar (koleksiyon): Ad, Kurum, Tarih, Açıklama
- Referanslar (koleksiyon): TipId, DilId, Ad, Soyad, Firma (opsiyonel), Pozisyon, Email, Telefon
- Hobiler (metin)
- CvDosyaUrl (IFileStorageService referansı)

### 2. Kayıt orkestrasyonu: RegisterCandidateCommand

Orkestrasyon **Candidate modülünde** yaşar (`Candidate.Application.Commands.RegisterCandidateCommand`), çünkü "adaya kayıt olma" iş akışı Candidate modülünün sorumluluğunda; Identity yalnızca hesap altyapısını sağlar.

Akış:
1. Handler, Identity'nin **public contract'ı** üzerinden (ADR-016'daki cross-module erişim pattern'i — doğrudan DB erişimi değil) senkron olarak `CreateUserAsync(email, password, firstName, lastName, phoneNumber)` çağırır ve `UserId` döner. Email benzersizlik kontrolü bu adımda yapılır (kullanıcıya anında hata dönebilmek için senkron olmalı).
2. User başarıyla oluşturulduktan sonra aynı handler `CandidateCv` aggregate'ini oluşturur: Ad/Soyad/Email/Telefon adım 1'deki User bilgisinden seed edilir, `UserId` FK olarak bağlanır. `CandidateCvContent` boş olarak oluşturulur.
3. **Telafi (compensation):** Adım 2 (Candidate DB'ye yazma) başarısız olursa, adım 1'de oluşturulan User, Identity'nin mevcut `Deactivate`/`Delete` yeteneği kullanılarak geri alınır. Dağıtık transaction yerine try/catch + compensating action ile ele alınır (saga-lite yaklaşım).
4. Identity'nin mevcut register akışındaki email-verification event'i (Outbox/RabbitMQ → Notification modülü) değişmeden çalışmaya devam eder.

### 3. Profil tamamlanma yüzdesi: CompletionPercentage read-model

- **Uygulama notu (2026-09):** Bu karar ilk yazıldığında "mevcut Outbox pattern'i" üzerinden yayınlanan bir domain event öngörülüyordu. Uygulama sırasında CAP'in process başına yalnızca tek bir instance'ı desteklediği ve bu instance'ın `IdentityDbContext`'e sabitlendiği ortaya çıktı (ADR-014 Amendment, 2026-09-16) — Candidate modülü kendi transactional outbox'ını kuramaz. Bu çelişki AGENTS.md §6 (Architectural Conflict Rule) kapsamında değerlendirildi ve **modül-içi (in-process) MediatR domain event dispatch'i** (Identity'nin audit-log handler'larıyla aynı `DomainEventDispatcher` + `INotificationHandler<DomainEventNotification<TEvent>>` pattern'i) ile çözüldü — aşağıdaki madde bu nihai kararı yansıtır.
- `CandidateCv` ve `CandidateCvContent` güncellendiğinde, aggregate `CandidateCvUpdatedDomainEvent` / `CandidateCvContentUpdatedDomainEvent` raise eder; bu event'ler `DbContext.SaveChangesAsync` sırasında `DomainEventDispatcher` tarafından modül-içi olarak (MediatR `IPublisher`, RabbitMQ/Outbox'a çıkmadan) dispatch edilir.
- Candidate modülü içinde bir `INotificationHandler` bu event'leri dinler, tamamlanma yüzdesini yeniden hesaplar ve `CandidateCv.CompletionPercentage` alanına yazar (`CandidateCv.UpdateCompletionPercentage` — bilerek yeni bir domain event raise etmez, aksi halde sonsuz döngü oluşur).
- Hesaplama kriterleri (ilk sürüm — genişletilebilir): Fotoğraf var mı, Adres dolu mu, Sosyal medya en az 1 girilmiş mi, Özet dolu mu, en az 1 Deneyim var mı, en az 1 Eğitim var mı, en az 1 Dil var mı, CV dosyası yüklü mü. Her kriter eşit ağırlıkta başlar, ileride ağırlıklandırma eklenebilir.

## Sonuçlar

**Artıları:**
- Liste/arama ekranları hafif `CandidateCv` sorgusuyla çalışır, `CandidateCvContent`'in ağır collection'ları join edilmez.
- Profil tamamlanma yüzdesi her istekte yeniden hesaplanmaz, tek okuma ile döner.
- Kayıt akışında User ile CV bilgisi ayrışık kaldığı için aday, hesap bilgisiyle iş başvuru bilgisini bağımsız yönetebilir.

**Eksileri / kabul edilen trade-off'lar:**
- İki aggregate + read-model senkronizasyonu, tek aggregate'e göre daha fazla dosya/handler/event demek.
- Read-model senkronizasyonu modül-içi MediatR ile aynı istek/transaction sınırları içinde senkron çalışır (bkz. Uygulama notu, Karar §3) — bu nedenle eventual consistency değildir, ancak recalculation her `SaveChangesAsync` sonrası ek bir sorgu/yazma turu (candidate CV + content'i yeniden okuyup yazma) demektir.
- Kayıt orkestrasyonunda distributed transaction olmadığı için telafi mantığının doğru çalışması kritik; entegrasyon testleriyle ayrıca doğrulanmalı.

## İlgili Görev (ADR gerektirmez)
Identity.User'a `FirstName`, `LastName`, `PhoneNumber` alanlarının eklenmesi (migration + register command/endpoint güncellemesi + testler) — Candidate modülü master prompt'undan önce tamamlanacak küçük bir ön-görev.

## İlgili Kararlar
- ADR-016: Cross-module erişim pattern'i (Identity çağrısı için kullanılacak)
- ADR-019: Generic file storage (fotoğraf ve CV dosyası yüklemeleri için)
