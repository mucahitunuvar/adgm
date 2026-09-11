# PROJECT.md

# Gençlik Merkezi — Proje Tanımı

## 1. Proje Özeti

Gençlik Merkezi, iş arayan gençler ile personel ihtiyacı bulunan firmaları bir araya getiren, bu süreci kariyer danışmanları aracılığıyla yöneten ve gençlerin istihdam süreçlerini takip eden bir **kariyer ve istihdam yönetim platformudur**.

Sistem yalnızca iş ilanı yayınlama ve başvuru toplama amacı taşımaz.

Temel amaç;

> **Genç profili → yetenek ve ihtiyaç analizi → uygun firma/pozisyon → kariyer danışmanı eşleştirmesi → görüşme → işe yerleştirme → işe yerleşme sonrası takip**

sürecini uçtan uca yönetmektir.

Platform aynı zamanda Gençlik Merkezi'nin kurumsal web sitesini, faaliyetlerini, projelerini, etkinliklerini, eğitimlerini, duyurularını ve medya içeriklerini yönetebileceği bir **kurumsal web yönetim platformu** olarak da hizmet verecektir.

---

# 2. Projenin Temel Amacı

Gençlik Merkezi'nin temel görevi:

* İş arayan gençleri tanımak,
* Gençlerin eğitim, beceri, deneyim ve kariyer hedeflerini değerlendirmek,
* Gençlerin eksik olduğu alanları belirlemek,
* Uygun eğitim ve gelişim fırsatlarına yönlendirmek,
* İşverenlerin personel ihtiyaçlarını anlamak,
* İşverenlerin ihtiyaçlarına uygun gençleri belirlemek,
* Gençleri uygun firmalara ve pozisyonlara yönlendirmek,
* İşveren ile genç arasındaki görüşmeleri organize etmek,
* İşe yerleşme sürecini takip etmek,
* İşe yerleşen gençlerin devamlılığını takip etmek,
* Gençlerin kariyer gelişimini desteklemek,
* Eğitim, atölye, seminer ve etkinliklerle gençlerin istihdam edilebilirliğini artırmaktır.

Sistem bu amacı dijital ortamda merkezi, ölçülebilir ve sürdürülebilir bir şekilde yönetmeyi sağlar.

---

# 3. Sistemin Temel Çalışma Modeli

Sistemin temel yaklaşımı klasik iş ilanı platformlarından farklıdır.

### Klasik yaklaşım

```text
İlan
  ↓
Başvuru
  ↓
Değerlendirme
  ↓
İşe Alım
```

### Gençlik Merkezi yaklaşımı

```text
Genç Profili
    ↓
Kariyer Değerlendirmesi
    ↓
Yetenek / Eğitim / Deneyim Analizi
    ↓
Kariyer İhtiyaçlarının Belirlenmesi
    ↓
Uygun Firma ve Pozisyonların Belirlenmesi
    ↓
Kariyer Danışmanı Eşleştirmesi
    ↓
Aday Yönlendirme
    ↓
Firma Değerlendirmesi
    ↓
Görüşme Organizasyonu
    ↓
Görüşme Sonucu
    ↓
İşe Yerleştirme
    ↓
İşe Yerleşme Sonrası Takip
```

Bu nedenle sistemin merkezinde **iş ilanı değil, genç ve kariyer danışmanlığı süreci** bulunur.

---

# 4. Sistem Aktörleri

Sistemde dört temel kullanıcı rolü bulunmaktadır:

1. Admin
2. İşveren
3. İş Arayan / Aday
4. Kariyer Danışmanı

---

# 5. Admin

Admin, Gençlik Merkezi'nin sistem üzerindeki en yüksek yetkili kullanıcısıdır.

Admin sistemin tamamını yönetebilir.

## 5.1 Kullanıcı Yönetimi

Admin:

* İşverenleri inceleyebilir.
* İşveren üyeliklerini onaylayabilir.
* İşveren üyeliklerini reddedebilir.
* Adayları inceleyebilir.
* Aday üyeliklerini onaylayabilir veya reddedebilir.
* Kariyer danışmanlarını yönetebilir.
* Kullanıcıların durumlarını değiştirebilir.
* Kullanıcı rollerini yönetebilir.
* Kullanıcılara gerekli yetki ve izinleri verebilir.

## 5.2 İlan Yönetimi

Admin:

* İş ilanlarını görüntüleyebilir.
* İlanları denetleyebilir.
* Uygunsuz ilanları engelleyebilir.
* İlanların yayın durumlarını yönetebilir.
* Gerektiğinde ilanı yayından kaldırabilir.

## 5.3 Kariyer Danışmanı Yönetimi

Admin:

* Kariyer danışmanlarını sisteme tanımlar.
* Danışmanların aktif/pasif durumlarını yönetir.
* Danışmanlara iş arayanları atar.
* Danışmanlara firmaları atar.
* Gerekirse atamaları değiştirebilir.
* Danışmanların iş yükünü takip edebilir.

## 5.4 Web Sitesi Yönetimi

Admin aynı zamanda Gençlik Merkezi'nin kurumsal web sitesindeki içerikleri yönetebilir.

Yönetilebilir içerikler arasında:

* Sayfalar
* Haberler
* Duyurular
* Projeler
* Faaliyetler
* Etkinlikler
* Eğitimler
* Atölyeler
* Gönüllülük faaliyetleri
* Başarı hikayeleri
* Yönetim ve ekip bilgileri
* Faaliyet raporları
* Şeffaflık içerikleri
* Dokümanlar
* İletişim bilgileri

bulunur.

---

# 6. İşveren

İşveren, personel ihtiyacı bulunan firmaları temsil eder.

İşveren sisteme üye olarak firma bilgilerini oluşturur ve personel ihtiyaçlarını Gençlik Merkezi'ne iletebilir.

## 6.1 Firma Profili

İşveren:

* Firma bilgilerini oluşturabilir.
* Firma profilini güncelleyebilir.
* Firma hakkında bilgi paylaşabilir.
* Firmanın web sitesinde yayınlanacak profil bilgilerini yönetebilir.

Onaylanan firma profili, kurumsal web sitesi üzerinden ziyaretçilere gösterilebilir.

## 6.2 İş İlanı

İşveren:

* İş ilanı oluşturabilir.
* Pozisyon bilgilerini girebilir.
* Aranan nitelikleri belirleyebilir.
* Eğitim ve deneyim kriterlerini belirleyebilir.
* İlanı düzenleyebilir.
* İlanın durumunu takip edebilir.

İşveren tarafından oluşturulan ilan doğrudan yayınlanmaz.

İlan önce ilgili kariyer danışmanının değerlendirmesine sunulur.

### İlan süreci

```text
İşveren ilanı oluşturur
        ↓
Kariyer danışmanına gönderilir
        ↓
Danışman inceler
        ↓
     ┌───────────────┐
     ↓               ↓
  Uygun           Uygun değil
     ↓               ↓
  Yayınlanır      Reddedilir
                     ↓
             Düzeltme talebi
```

## 6.3 Personel İhtiyacı Bildirme

İşveren yalnızca ilan oluşturmak zorunda değildir.

İşveren doğrudan Gençlik Merkezi'ne:

> "Şu özelliklerde bir personele ihtiyacımız var."

şeklinde personel ihtiyacı bildirebilir.

Örneğin:

* Pozisyon
* Eğitim seviyesi
* Teknik beceriler
* Deneyim
* Sertifikalar
* Çalışma şekli
* Çalışma lokasyonu
* Çalışma saatleri
* Ücret bilgisi
* Aranan diğer kriterler

gibi bilgiler iletilir.

Kariyer danışmanı bu ihtiyacı değerlendirerek aday havuzundan uygun gençleri belirleyebilir.

Bu süreçte ilan yayınlanması zorunlu değildir.

---

# 7. İş Arayan / Aday

İş arayan kullanıcı, Gençlik Merkezi'nin hizmet verdiği gençleri temsil eder.

Adayın amacı yalnızca ilanlara başvurmak değildir.

Aday sistemde kapsamlı bir kariyer profili oluşturur.

## 7.1 Aday Profili

Aday aşağıdaki bilgileri içerebilen gelişmiş bir CV oluşturabilir:

* Kişisel bilgiler
* Eğitim bilgileri
* İş deneyimleri
* Stajlar
* Yetenekler
* Teknik beceriler
* Yabancı dil bilgileri
* Sertifikalar
* Eğitimler
* Projeler
* Gönüllülük deneyimleri
* Kariyer hedefleri
* Çalışmak istediği alanlar
* Çalışmak istediği pozisyonlar
* Tercih edilen çalışma şekli
* Tercih edilen lokasyon
* Diğer kariyer bilgileri

## 7.2 Kariyer Danışmanı

Aday kendisine atanmış kariyer danışmanını görebilir.

Kariyer danışmanı:

* Adayın profilini inceleyebilir.
* CV bilgilerini değerlendirebilir.
* Adayla görüşme yapabilir.
* Becerilerini değerlendirebilir.
* Kariyer hedeflerini değerlendirebilir.
* Eksik eğitimleri belirleyebilir.
* Eksik sertifikaları belirleyebilir.
* Uygun iş ilanlarını araştırabilir.
* İlan bulunmasa bile uygun firmalara adayı önerebilir.

## 7.3 İş ve Eğitim Önerileri

Sistem adayın profilini analiz ederek:

* Uygun iş ilanlarını,
* Uygun firmaları,
* Uygun pozisyonları,
* Uygun eğitimleri,
* Uygun etkinlikleri,
* Kariyer gelişim fırsatlarını

önerilebilir hale getirecektir.

İlerleyen aşamalarda bu öneriler kural tabanlı veya yapay zeka destekli hale getirilebilir.

---

# 8. Kariyer Danışmanı

Kariyer danışmanı sistemin merkezindeki operasyonel aktördür.

Kariyer danışmanları Gençlik Merkezi çalışanlarıdır.

Temel görevleri iş arayan gençler ile firmalar arasındaki doğru eşleşmeyi sağlamaktır.

## 8.1 Aday Yönetimi

Kariyer danışmanı kendisine atanmış adayları:

* İnceler.
* Profillerini değerlendirir.
* CV'lerini inceler.
* Gençlerle görüşür.
* Beceri ve ihtiyaçlarını değerlendirir.
* Kariyer hedeflerini belirler.
* Eksik eğitimleri tespit eder.
* Eksik sertifikaları tespit eder.
* Uygun iş fırsatlarını belirler.

## 8.2 Firma Yönetimi

Kariyer danışmanı kendisine atanmış firmaları:

* İnceler.
* Personel ihtiyaçlarını değerlendirir.
* İş ilanlarını inceler.
* Firma ihtiyaçlarını analiz eder.
* Uygun adayları belirler.

## 8.3 Aday Yönlendirme

Danışman, adayın profili ile firma ihtiyacını karşılaştırarak uygun adayları firmalara önerebilir.

Bu işlem ilan üzerinden yapılabileceği gibi ilan olmadan da yapılabilir.

```text
Firma Personel İhtiyacı
          ↓
Kriter Analizi
          ↓
Aday Havuzu
          ↓
Uygun Adaylar
          ↓
Kariyer Danışmanı
          ↓
Firmaya Aday Önerisi
```

## 8.4 Görüşme Organizasyonu

Firma veya aday görüşme talebinde bulunabilir.

Kariyer danışmanı:

* Görüşme talebini değerlendirir.
* Aday ve firma ile iletişim sürecini yönetir.
* Görüşme tarihini belirler.
* Görüşme saatini belirler.
* Görüşmeyi sistem üzerinde oluşturur.
* Taraflara görüşme bilgisini bildirir.

Sistem:

> "Aday ve firma arasında X tarihinde, X saatinde görüşme bulunmaktadır."

şeklinde bildirimler oluşturabilir.

## 8.5 Görüşme Sonucu

Görüşme sonrasında sonuç sistemde tutulur.

Örneğin:

* Olumlu
* Olumsuz
* Beklemede
* Tekrar görüşme gerekli

gibi durumlar kullanılabilir.

Danışman ayrıca görüşme ile ilgili not ekleyebilir.

## 8.6 İşe Yerleştirme Takibi

Kariyer danışmanı:

* İşe yerleşme durumunu takip eder.
* İşe başlangıç bilgisini takip eder.
* İşe yerleşme sürecine ilişkin notlar tutabilir.
* İşe yerleşen adayın devamlılığını takip eder.

Bu nedenle süreç görüşme sonucunda bitmez.

---

# 9. Kariyer Danışmanı Atama Sistemi

Her aday ve firma bir kariyer danışmanına atanabilir.

Bu atamalar manuel olarak yapılabileceği gibi otomatik olarak da gerçekleştirilebilir.

Örneğin Gençlik Merkezi'nde beş kariyer danışmanı bulunuyorsa sistem:

```text
Danışman 1 → 20 Firma + 50 Aday
Danışman 2 → 20 Firma + 50 Aday
Danışman 3 → 20 Firma + 50 Aday
Danışman 4 → 20 Firma + 50 Aday
Danışman 5 → 20 Firma + 50 Aday
```

şeklinde dengeli bir dağılım yapabilir.

Atama algoritması yalnızca toplam kullanıcı sayısına göre değil, ilerleyen aşamalarda:

* Danışmanın mevcut iş yükü
* Uzmanlık alanı
* Firma sektörü
* Adayın kariyer alanı
* Lokasyon
* Danışmanın aktif/pasif durumu

gibi kriterleri de dikkate alabilecek şekilde geliştirilebilir.

---

# 10. Eşleştirme Sistemi

Sistemin temel yeteneklerinden biri aday ve firma eşleştirmesidir.

Eşleştirme aşağıdaki veriler üzerinden yapılabilir:

### Aday

* Eğitim
* Deneyim
* Beceri
* Sertifika
* Yabancı dil
* Kariyer hedefi
* Pozisyon tercihi
* Lokasyon tercihi
* Çalışma şekli
* Diğer uygunluk kriterleri

### Firma / Pozisyon

* Pozisyon
* Eğitim kriteri
* Deneyim kriteri
* Beceri kriterleri
* Sertifikalar
* Yabancı dil
* Lokasyon
* Çalışma şekli
* Diğer işe alım kriterleri

Sistem bu verileri kullanarak uygunluk skoru oluşturabilecek şekilde tasarlanabilir.

İlerleyen aşamada bu sistem:

* Kural tabanlı eşleştirme
* Skor tabanlı eşleştirme
* Yapay zeka destekli aday önerisi

modellerine genişletilebilir.

---

# 11. Eğitim ve Kariyer Gelişimi

Gençlik Merkezi yalnızca istihdam hizmeti sunmaz.

Gençlerin kariyer gelişimini desteklemek amacıyla:

* Eğitimler
* Atölyeler
* Seminerler
* Kariyer etkinlikleri
* Gönüllülük faaliyetleri
* Sertifika programları
* İşveren buluşmaları
* Kariyer etkinlikleri

düzenleyebilir.

Bu faaliyetler aday profili ile ilişkilendirilebilir.

Örneğin:

```text
Aday
 ↓
Beceri Analizi
 ↓
Eksik Beceri
 ↓
Önerilen Eğitim
 ↓
Eğitime Katılım
 ↓
Yeni Beceri / Sertifika
 ↓
Yeni İş Fırsatları
```

Bu yapı, sistemin yalnızca iş bulma değil, **kariyer gelişimi** platformu olmasını sağlar.

---

# 12. İşe Yerleşme Sonrası Takip

Sistemin önemli özelliklerinden biri işe yerleştirme sonrasında da sürecin devam etmesidir.

İşe yerleşen aday için:

* İşe başlangıç tarihi
* Firma
* Pozisyon
* Çalışma durumu
* Takip görüşmeleri
* Danışman notları
* Devamlılık durumu
* İşten ayrılma durumu
* Ayrılma nedeni

gibi bilgiler takip edilebilir.

Bu bilgiler Gençlik Merkezi'nin istihdam başarısını ölçmesine yardımcı olur.

---

# 13. Kurumsal Web Sitesi

Platform aynı zamanda Gençlik Merkezi'nin halka açık kurumsal web sitesini barındıracaktır.

## 13.1 Anasayfa

Kurumsal web sitesinin ana giriş sayfasıdır.

## 13.2 Kurumsal

* Hakkımızda
* Misyon, Vizyon ve Değerler
* Yönetim ve Ekip
* Faaliyet Raporları
* Şeffaflık

## 13.3 Çalışmalarımız

* Faaliyetler
* Projeler
* Etkinlikler
* Eğitim ve Atölyeler
* Gönüllülük

## 13.4 İş ve Kariyer

* İş İlanları
* Gençler İçin
* İşverenler İçin
* Profil ve CV Oluştur
* Personel İhtiyacı Bildir

## 13.5 Medya

* Haberler
* Duyurular
* Başarı Hikayeleri
* Doküman Merkezi

## 13.6 İletişim

Gençlik Merkezi'nin iletişim bilgilerinin ve iletişim kanallarının yayınlandığı bölümdür.

## 13.7 Arama

Kullanıcıların web sitesi içerisindeki içerikleri ve uygun şekilde iş/kariyer içeriklerini arayabilmesini sağlayan arama özelliğidir.

## 13.8 Üyelik

Web sitesi üzerinden:

* Giriş Yap
* Ücretsiz Üye Ol

işlemleri gerçekleştirilebilir.

Üyelik türüne göre kullanıcı uygun sisteme yönlendirilir.

---

# 14. Web Sitesi ve Yönetim Platformu İlişkisi

Kurumsal web sitesi ile kariyer/istihdam yönetim platformu aynı ekosistemin parçalarıdır.

Kamuya açık web sitesi:

```text
Ziyaretçi
    ↓
Gençlik Merkezi Web Sitesi
```

Üyelik ve operasyon tarafı:

```text
Aday
İşveren
Danışman
Admin
    ↓
Gençlik Merkezi Platformu
```

İki taraf ortak içerik ve iş verilerini kontrollü şekilde kullanabilir.

Örneğin:

* Yayındaki iş ilanları web sitesinde gösterilebilir.
* Onaylanmış firma profilleri web sitesinde gösterilebilir.
* Etkinlikler web sitesinde yayınlanabilir.
* Eğitimler web sitesinde yayınlanabilir.
* Haberler ve duyurular yönetim panelinden oluşturulabilir.
* Başarı hikayeleri yayınlanabilir.

---

# 15. Temel Kullanıcı Yolculukları

## 15.1 Aday Yolculuğu

```text
Üye Ol
  ↓
Profil Oluştur
  ↓
CV Oluştur
  ↓
Onay / Değerlendirme
  ↓
Kariyer Danışmanı Ataması
  ↓
Kariyer Değerlendirmesi
  ↓
İş / Eğitim Önerileri
  ↓
Aday Yönlendirme
  ↓
Firma Görüşmesi
  ↓
İşe Yerleşme
  ↓
İşe Yerleşme Sonrası Takip
```

## 15.2 İşveren Yolculuğu

```text
Firma Üyeliği
  ↓
Firma Bilgileri
  ↓
Onay
  ↓
Kariyer Danışmanı Ataması
  ↓
Personel İhtiyacı Bildirme
       veya
İş İlanı Oluşturma
  ↓
Danışman Değerlendirmesi
  ↓
Aday Önerileri
  ↓
Görüşme
  ↓
İşe Alım
```

## 15.3 Kariyer Danışmanı Yolculuğu

```text
Danışmana Aday/Firma Ataması
          ↓
Profil ve İhtiyaç Analizi
          ↓
Aday ↔ Firma Eşleştirmesi
          ↓
Aday Önerisi
          ↓
Görüşme Organizasyonu
          ↓
Görüşme Sonucu
          ↓
İşe Yerleştirme
          ↓
İşe Yerleşme Sonrası Takip
```

---

# 16. Sistem Başarı Modeli

Sistemin başarısı yalnızca:

> "Kaç ilan yayınlandı?"

veya

> "Kaç başvuru yapıldı?"

metrikleri ile ölçülmemelidir.

Temel başarı göstergeleri arasında:

* Aktif aday sayısı
* Aktif firma sayısı
* Kariyer danışmanı başına aday sayısı
* Kariyer danışmanı başına firma sayısı
* Aday-firma eşleşme sayısı
* Aday yönlendirme sayısı
* Görüşme sayısı
* Olumlu görüşme oranı
* İşe yerleştirme sayısı
* İşe yerleştirme oranı
* İşe yerleşme sonrası devamlılık
* Eğitim katılımı
* Eğitim sonrası istihdam
* Firma memnuniyeti
* Aday memnuniyeti

gibi ölçümler kullanılabilir.

---

# 17. Projenin Temel Felsefesi

Bu platformun temel yaklaşımı:

> **Doğru genci doğru işle buluşturmak.**

Ancak sistem bunun da ötesinde çalışır.

Amaç yalnızca iş bulmak değil;

> **Gençleri tanımak, geliştirmek, doğru fırsatlarla buluşturmak ve istihdam sürecini sürdürülebilir şekilde takip etmektir.**

Bu nedenle platformun merkezinde:

```text
                 GENÇ
                  │
          ┌───────┴────────┐
          │                │
      Kariyer           Gelişim
      Danışmanlığı       Eğitimleri
          │                │
          └───────┬────────┘
                  │
            Uygun Firma
                  │
              Görüşme
                  │
            İşe Yerleşme
                  │
             Takip
```

bulunur.

---

# 18. Projenin Kapsamı

Proje aşağıdaki ana yetenekleri kapsar:

### Kullanıcı Yönetimi

* Admin
* İşveren
* Aday
* Kariyer Danışmanı

### Kariyer Yönetimi

* Aday profili
* CV
* Beceri yönetimi
* Eğitim ve sertifikalar
* Kariyer hedefleri
* Kariyer danışmanı ataması

### İşveren Yönetimi

* Firma profili
* Personel ihtiyacı
* İş ilanları
* Aday önerileri
* Başvuru ve aday değerlendirme

### Eşleştirme

* Aday-firma eşleştirme
* Pozisyon uygunluk analizi
* Aday önerileri
* Otomatik eşleştirme

### Görüşme Yönetimi

* Görüşme talepleri
* Görüşme planlama
* Bildirimler
* Görüşme sonuçları
* Görüşme notları

### İstihdam Takibi

* İşe yerleşme
* İşe başlama
* Devamlılık
* İşten ayrılma
* Takip görüşmeleri

### Eğitim ve Etkinlik

* Eğitimler
* Atölyeler
* Seminerler
* Etkinlikler
* Gönüllülük

### Kurumsal Web Sitesi

* Kurumsal içerikler
* Haberler
* Duyurular
* Projeler
* Faaliyetler
* Başarı hikayeleri
* Doküman merkezi
* İletişim

---

# 19. Gelecekte Genişletilebilecek Alanlar

Sistem ilerleyen aşamalarda aşağıdaki yeteneklerle genişletilebilir:

* Yapay zeka destekli aday eşleştirme
* Yapay zeka destekli CV analizi
* Kariyer önerileri
* Beceri açığı analizi
* Otomatik eğitim önerileri
* Otomatik aday önerileri
* İşveren için otomatik aday havuzu oluşturma
* Kariyer danışmanı iş yükü optimizasyonu
* SMS bildirimleri
* E-posta bildirimleri
* Mobil uygulama
* Gelişmiş raporlama
* İstihdam istatistikleri
* Veri analitiği
* Dashboard ve KPI takibi

Bu özellikler mevcut temel mimariyi bozmayacak şekilde sisteme sonradan eklenebilmelidir.

---

# 20. Proje İçin Temel İlke

Gençlik Merkezi platformu bir **iş ilanı sitesi** olarak değerlendirilmemelidir.

Platformun temel amacı:

> **Gençleri tanımak, kariyer gelişimlerini desteklemek, uygun firmalarla eşleştirmek, işe yerleştirmek ve istihdam süreçlerini takip etmektir.**

İş ilanları bu sistemin yalnızca bir parçasıdır.

Sistemin asıl değeri:

> **Kariyer danışmanlığı destekli insan kaynağı eşleştirme ve istihdam yönetimidir.**
