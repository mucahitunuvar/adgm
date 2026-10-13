# Ahmet Demir Gençlik Merkezi: public web sitesi tasarım promptu

Aşağıdaki metni yeni bir sohbete (ADGM Proje içinde) yapıştır. Sohbet, projedeki `claude/web-frontend-devir-ozeti.md` ve `claude/web-frontend-karar-ozeti.md` dosyalarını okuyabilir; okuyamazsa iki dosyayı ek olarak ver.

---

## PROMPT (buradan başla)

Sen "Ahmet Demir Gençlik Merkezi" için public web sitesinin arayüz tasarımcısısın. Önce `claude/web-frontend-devir-ozeti.md` ve `claude/web-frontend-karar-ozeti.md` dosyalarını oku. Orada menü, ana sayfa akışı, sayfa envanteri, palet, tasarım dili ve teknik sınırlar kesinleşti. Karar verilmiş hiçbir şeyi yeniden tartışma, yeni karar uydurma. Bir şey eksikse sor.

Bu bir tasarım görevi. Kod yazma. Çıktı Claude'un Design türünde bir tasarım tahtası (design canvas) olsun, ben tahtalara bakıp geri bildirim vereceğim.

### Geçmiş geri bildirim (tekrarlama)
İlk taslağım çok yazılı, neredeyse hiç görsel yoktu, menü detaylandırılmamıştı. Bu sefer: görsel ağırlıklı, gerçekçi fotoğraflı, menü açık halleriyle detaylı olsun. "Lorem ipsum" yok; gerçekçi Türkçe içerik yaz.

### Tasarım dili
- "Modern güven": yuvarlak köşe (tek, orantılı radius ölçeği), glass etkisi, açık ve koyu tema, keskin editoryal tipografi, bol boşluk.
- Palet: #071f32, #041725, #102a2e (koyu yüzeyler); #c44325, #92301b, #d94a2b (vurgu); #0a686b (turkuaz, ikincil, ince süsleme); #e6efed, #ffffff (açık yüzeyler).
- Butonlar ve CTA: #c44325. Parlak #d94a2b sadece büyük yazı ve süslemede. Turkuaz ikon, rozet ve küçük dokunuşlarda.
- Glass SADECE üç yerde: sticky header, görsel üstüne binen kartlar (hero, video, başarı hikayesi) ve pop-up. Diğer içerik yüzeyleri mat. Glass altında koyu katman olsun, yazı okunur kalsın. Mobilde blur hafif.
- "Yapay zekâ sitesi" görünümünden kaçın: mor-mavi gradyanlar, aynı boyutta ikonlu üç kart sırası, jenerik illüstrasyonlar yok. Fotoğraf ağırlıklı, ikon sade ve az.
- Tipografi: karakterli bir başlık yazı tipi + okunaklı gövde yazı tipi öner (kendi sunucumuzdan yayınlanabilen, açık lisanslı olmalı, Google Fonts CDN kullanılmayacak). Türkçe karakterleri (ç ğ ı ö ş ü İ) mutlaka ekranda göster.
- Erişilebilirlik: WCAG AA kontrast, görünür odak halkası, hareket azaltma tercihine saygı. Her renk çiftini kontrol et.

### Fotoğraflar
Başlangıçta stok fotoğraf kullanılacak. Müşteriye sunumda etkileyici görünsün diye gerçekçi, gençlik/kariyer/mülakat/atölye/etkinlik temalı, doğal ışıklı, "poz verilmiş stok" hissi vermeyen görseller seç. Ücretsiz ve ticari kullanıma açık kaynaklardan (örneğin Unsplash veya Pexels) kullan. Filigranlı önizleme görseli kullanma. Her görsel yuvası değiştirilebilir olsun: oranı sabit (örneğin 16:9, 4:3, 1:1, 3:4), içerik yerine geçince yerleşim bozulmasın. Tahtada "demo görsel, yayında gerçek görselle değişecek" diye belirt.

### İş akışı (aşamalı, her aşamada onay bekle)
**Aşama 1: Temel.** Tasarım sistemi tahtası: renk (açık/koyu), tipografi ölçeği, radius, buton/form/rozet/kart bileşenleri, glass örnekleri. Sonra header (masaüstü + yapışkan hali), tüm açılır menüler (Kurumsal, Çalışmalarımız, Gençler, Firmalar, Medya), hızlı ilan arama kutusu, Giriş Yap / Üye Ol, tema anahtarı, mobil menü çekmecesi (kapalı + açık + akordeon açık), footer (4 kolon + alt 3 kolon). Açık ve koyu temada.

**Aşama 2: Ana sayfa.** Bölüm bölüm, masaüstü ve mobil, açık ve koyu:
1. Slider (yazı + buton; birkaç slayt örneği; kontroller ve duraklatma)
2. Gençlik Merkezi tanıtımı (creative: görsel kompozisyonu + yazı)
3. Rakamlar şeridi (etiket, değer, ikon; panelden düzenlenir)
4. Öne çıkan ilanlar (slide ya da 3x2 grid: ikisini de göster, ben seçeceğim; şirket adı ve logosu görünür)
5. Nasıl çalışır (aday / firma sekmeli, iyi bir tanıtım)
6. Video tanıtım (creative; tıklayınca YouTube popup'ı açılır: popup halini de çiz)
7. Etkinlikler (creative; kayıt durumu rozetleri: açık, kontenjan dolu, onay bekliyor, geçmiş)
8. Başarı hikayeleri (kart container, solda yazı sağda video, slider)
9. Firmalar için bant (tasarım sana bırakıldı, dikkat çekici ama sade)
10. Haberler (solda son haber büyük görselli, sağda alt alta iki haber)
11. E-bülten (tasarım sana bırakıldı)
12. Ana sayfa pop-up'ı (glass)
Boş durumda ilan/etkinlik/haber/hikaye bölümleri gizlenir; bunu tahtada not et.

**Aşama 3: Şablonlar.** İçerik sayfası (yan menüyle), kart liste + detay (haber örneği), Yönetim ve Ekip (foto, ad, unvan kartları), Faaliyet Raporları (yıla göre dosya listesi), Dökümanlar (kategori filtreli indirme kartları), `/ilanlar` listesi ve ilan detayı, etkinlik listesi ve detayı (tüm kayıt durumları, giriş yapmış aday için dolu form), form sayfası (Gönüllük Formu / İletişim, hata ve başarı durumları), Personel İhtiyacı Bildir tanıtım sayfası, `/giris` ve `/uye-ol` (solda Aday, sağda Firma, mobilde alt alta; şifre alanı YOK, kartlar portala yönlendirir), 404, bakım sayfası, bülten onay sayfası, çerez bandı (zorunlu / analitik kategorileri).

### Kurallar
- Her aşamanın sonunda ne yaptığını iki üç cümleyle özetle, hangi kararı benden beklediğini yaz. Onay gelmeden sonraki aşamaya geçme.
- Aynı bileşeni her yerde aynı biçimde kullan (tutarlılık). Bileşenleri tasarım sistemi tahtasından türet.
- Menü ve içerik metinlerini karar özetindeki adlarla birebir yaz (Çalışmalarımız, Gençler, Firmalar, Medya, İletişim...).
- İçerik uydururken kurum adı "Ahmet Demir Gençlik Merkezi" olsun. Rakamları, haberleri ve ilanları makul örnekle doldur, ama tahtada "örnek içerik" diye işaretle.
- Teknik sınırlar: sadece Türkçe, açık/koyu tema, public sitede şifre alanı yok, site içi arama yok (sadece "hızlı ilan ara").
- Beğenmediğim yön olursa küçük tahtalarda alternatif sun, ana tahtayı şişirme.

Başla: önce iki dosyayı oku, sonra Aşama 1'in planını üç dört cümleyle yaz ve tahtayı kurmaya başla.

## PROMPT (buraya kadar)
