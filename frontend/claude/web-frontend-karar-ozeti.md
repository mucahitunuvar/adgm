# Public web sitesi frontend: karar özeti (13 Ekim 2026)

Bu dosya `web-frontend-devir-ozeti.md` (12 Ekim) dosyasının devamıdır. Yeni sohbet bu iki dosyayı birlikte okumalı. Sıra: önce mimari/karar, sonra tasarım, sonra kod. Tasarıma geçilmedi.

## 1. Görsel yön ve tasarım dili
- Yön: "Modern güven". Yuvarlak köşe (orantılı radius), glass, açık/koyu tema.
- Tema: varsayılan sistem tercihini izler, ziyaretçi anahtarla değiştirir, seçim tarayıcıda saklanır.
- Palet: #071f32, #041725, #c44325, #92301b, #e6efed, #fff, #102a2e, #0a686b, #d94a2b.
- Vurgu #c44325 (buton, CTA). Turkuaz (#0a686b) ikon, rozet, ince süslemede. Parlak #d94a2b sadece büyük yazı ve süslemede (beyaz yazıyla küçük metinde kontrast yetmez).
- Glass sadece: sticky header, görsel üstü kartlar (hero, video, başarı hikayesi), pop-up. Düz içerik bölümleri mat. Glass altına koyu katman şart.
- Sadece Türkçe, kökte ön eksiz URL. Sonradan diller `/en/...` altına eklenir.

## 2. Menü
Ana menü: Kurumsal (Hakkımızda, Yönetim ve Ekip, Şeffaflık, Faaliyet Raporları, Dökümanlar) | Çalışmalarımız (Faaliyetler, Projeler, Etkinlikler, Gönüllük Formu) | Gençler (Kariyer Danışmanlığı, Eğitim ve Atölye, CV ve Mülakat Rehberi, Nasıl Çalışır) | Firmalar (Neden Biz, Nasıl Çalışır, Personel İhtiyacı Bildir) | Medya (Etkinlikler, Haberler, Duyurular, Başarı Hikayeleri) | İletişim.
- Etkinlikler iki menüde de var, tek sayfa (`/etkinlikler`). Nasıl Çalışır iki ayrı slug.
- Sağda: "Hızlı ilan ara" (sadece ilan arar, `/ilanlar?q=`; site içi arama yok), tema anahtarı, Giriş Yap, Üye Ol.
- Giriş Yap / Üye Ol: ayrı sayfalar `/giris` ve `/uye-ol`; solda Aday, sağda Firma kartı (mobilde alt alta); her kart ilgili portalın kendi sayfasına gider. Public sitede şifre alanı yok.
- Menü öğesi tipleri: içerik sayfası, içerik tipi liste sayfası, sistem rotası, dış URL, bağlantısız grup başlığı.
- Mobil: tam ekran çekmece, akordeon, giriş CTA'ları altta sabit.

Footer: 4 kolon (1 logo+açıklama+sosyal, 2 Kurumsal alt sayfaları, 3 İş ve Kariyer: İş İlanları, Kariyer Danışmanlığı, Personel İhtiyacı Bildir, CV ve Mülakat Rehberi, 4 İletişim: telefon, mail, adres). Altında 3 kolon: solda KVKK/çerez, ortada copyright, sağda "Design & Software: Alaz Karbon".

## 3. Ana sayfa akışı (12 bölüm)
1. Header
2. Slider (yazı + buton; masaüstü görseli zorunlu, mobil görsel opsiyonel, yoksa masaüstü kullanılır; sınır yok)
3. Gençlik Merkezi tanıtımı (görsel + yazı, creative)
4. Rakamlar şeridi (panelden düzenlenir: etiket, değer, ikon)
5. Öne çıkan ilanlar (en yeni N ilan, N ayarlanabilir; slide ya da 3x2 grid)
6. Nasıl çalışır (aday / firma sekmeli)
7. Video tanıtım (Videolar bölümünden seçilir; tıklayınca YouTube popup)
8. Etkinlikler (creative)
9. Başarı hikayeleri (kart container, solda yazı sağda video, slider)
10. Firmalar için bant (tasarım Claude'a bırakıldı)
11. Haberler (solda son haber büyük, sağda alt alta iki haber)
12. E-bülten (tasarım Claude'a bırakıldı)
13. Footer

Pop-up sadece ana sayfada. Boş bölümler (ilan, etkinlik, haber, hikaye) gizlenir. Ortak/destekçi bölümü yok.
Blok sistemi KALIYOR: ana sayfa bölümleri backend blok listesinden çizilir, ileride yeni blok eklenebilir. Frontend'de blok tipi → bileşen kaydı (registry) olacak, bilinmeyen tip güvenle atlanır. Bölümlerin 15 blokluk ADR-024 kataloğuyla eşlemesi tasarım sonrasına bırakıldı (blok anlamı kullanıcıya açıklandı).

## 4. Sayfa envanteri ve şablonlar
- Ana sayfa; içerik sayfası (hiyerarşi için yan menü); kart liste + detay (Faaliyetler, Projeler, Haberler, Duyurular, Başarı Hikayeleri; galeri, video listesi, paylaş, ilgili içerik).
- Yönetim ve Ekip: sadece kart (foto, ad, unvan), detay yok.
- Faaliyet Raporları: yazı + yıla göre dosya eki. Şeffaflık: yazı sayfası. Dökümanlar: kategorili indirilebilir medya listesi (logo, belge).
- Faaliyetler ve Projeler ayrı içerik tipi olarak başlar (ContentType dinamik, sonra birleştirilebilir).
- Etkinlik: sadece liste (takvim yok), detayda kayıt durumları: açık, onay bekliyor, onaylandı, kontenjan dolu, kapalı, geçmiş. Giriş yapmış aday varsa form otomatik dolar.
- İlan liste `/ilanlar` (hızlı arama, filtre, sayfalama); ilan detay `/ilanlar/{slug}`: şirket adı ve logosu görünür, public şirket profil sayfası YOK, askerlik tercihi görünür, cinsiyet tercihi yok, Başvur → aday portalı.
- Personel İhtiyacı Bildir: public sitede tanıtım + CTA, form firma portalında.
- Form sayfaları: Gönüllük Formu, İletişim (form motoru + Turnstile).
- Sistem sayfaları: 404, bakım (503), form teşekkür, bülten onay/çıkış, çerez bandı.

## 5. Teknik kararlar
**Render:** Public site Next.js (App Router), sunucuda render + süreye dayalı önbellek (ana sayfa/içerik ~60 sn). Etkinlik kontenjan ve kayıt durumu tarayıcıda canlı çekilir. Aday/firma/yönetim portalları düz React SPA (Vite). sitemap.xml ve robots.txt backend'den (ters vekil API'ye yönlendirir); schema.org backend yanıtından basılır.
**Yollar (tek host):** `/api/**` backend; `/sitemap.xml`, `/robots.txt` backend; `/admin/**` yönetim; `/portal/aday/**`, `/portal/firma/**` portallar; geri kalan her şey public site. Bu yollar Website modülünün rezerve slug listesinde olmalı (doğrulanacak).
**Oturum:** Public site anonim çalışır; giriş bilgisi sadece header ("Hesabım") ve etkinlik formu ön doldurması için, sayfa yüklendikten sonra tarayıcıda çözülür (sayfalar önbelleğe girebilsin). Token saklama önerisi: HttpOnly+Secure+SameSite çerez (localStorage değil). KESİNLEŞMEDİ: Identity modülündeki mevcut giriş sistemi ve SECURITY.md ile birlikte, o aşamaya gelince kontrol edilecek.
**Bağımlılıklar:** Next.js + React 19 + TypeScript strict, Tailwind (tema CSS değişkenleri), Embla Carousel (MIT), Motion (ölçülü, prefers-reduced-motion saygılı), openapi-typescript ile OpenAPI'den tipli istemci, Lucide ikon, yazı tipi self-host (Google Fonts yok), kendi "tıklayınca yükle" YouTube bileşeni (`youtube-nocookie`). Next görsel optimizasyonu kapalı, backend'in sabit boyut varyantları srcset ile kullanılır. UI UX Pro Max sadece kontrol listesi olarak, 21st kullanılmaz.
**Güvenlik:** HTML sadece backend'in temizlediği alandan basılır; CSP Turnstile ve youtube-nocookie dışında kapalı.
**Depo:** Aynı depo (`mucahitunuvar/adgm`), pnpm workspace: `apps/web`, `apps/candidate`, `apps/employer`, `apps/admin`, ortak `packages/api-client`, `packages/design-tokens`. Koda başlamadan önce frontend AGENTS.md yazılacak (klasör yapısı, bileşen kuralları, bağımlılık kuralı, erişilebilirlik, performans bütçesi).
**Barındırma:** Windows sunucu, Turkiye'de, Turhost. Next.js `standalone` çıktısı Windows servisi olarak çalışır, IIS (URL Rewrite + ARR) ters vekil. Kullanıcı sunucunun düz Windows Server olduğunu ve Node.js çalıştırabildiğini doğruladı (13 Ekim). Frontend kuralları `frontend/AGENTS.md` ve `frontend/CLAUDE.md` içinde toplandı (taslak dosyalar: frontend-AGENTS.md, frontend-CLAUDE.md; depoya `frontend/` altına AGENTS.md ve CLAUDE.md adıyla konacak). Stok fotoğraf ile başlanır, ikon sade ve az. Tasarım eklentileri (UI UX Pro Max, 21st) bağlanmadı ve projenin bağımlılığı değil; Claude Code VS Code'da kullanılıyor.
**Analitik:** KARAR: Google Analytics 4. Çerez bandında "analitik" kategorisi açık onaya (opt-in) bağlı; onaydan önce GA scripti yüklenmez. KVKK aydınlatma metninde yurt dışı aktarım belirtilir (Turnstile ile birlikte). GA tek bir bileşen olarak onay durumuna bağlanır, ileride Matomo/Umami'ye geçmek gerekirse sadece o bileşen değişir. Onay tercihi tarayıcıda saklanır, ziyaretçi sonradan değiştirebilir (footer'da "Çerez tercihleri" bağlantısı).

## 6. Açık kararlar
1. (KARARLANDI) Analitik: GA4, onaya bağlı. Ölçüm kimliği (G-XXXX) yönetimden ya da ortam ayarından verilecek.
2. (KARARLANDI) Sunucu düz Windows Server, Node.js çalışır. Kalan: Windows servis sarmalayıcı (WinSW/NSSM) seçimi ve dağıtım prosedürü.
3. Oturum/token saklama: Identity modülü + SECURITY.md okunarak.
4. Blok kataloğu eşlemesi (tasarım sonrası).

## 7. Backend eksikleri listesi (toplanan)
- Public ilan listesinde anahtar kelime (`q`) filtresi var mı doğrula; yoksa ekle.
- Faaliyet Raporları için içeriğe dosya eki bağlama ve public yanıtta dosya listesi doğrula.
- Dökümanlar için kategorili public doküman listesi yanıtı doğrula.
- Rakamlar şeridi için veri/blok (panelden düzenlenebilir) doğrula.
- Rezerve slug listesi (api, admin, portal...) doğrula.
- Kapanmış ilan için public davranış (404/410) kararı.
- Bakım modunun 503 + Retry-After döndürdüğünü doğrula (önceki açık iş).
- İsteğe bağlı: içerik yayınlanınca frontend önbelleğini temizleyen webhook.
- Önceki özetteki açık işler geçerli: robots.txt koşulu (Disallow yalnızca AllowSearchEngineIndexing=false iken), ilana başvuru, mülakat planlama bildirimi, hatırlatma/erteleme, Interview/Employment/Matching ADR'leri, Faz 4 mimari testi, docs/prompts commit'i.

## 8. Sıradaki iş
1. Frontend AGENTS.md ve CLAUDE.md taslağı yazıldı; kullanıcı gözden geçirip `frontend/` altına koyacak.
2. Görsel yön: stok fotoğraf, sade ikon (kararlandı). Kalan: stok fotoğraf kaynağı/lisansı ve görsel slot oranları.
3. Windows servis sarmalayıcı seçimi ve dağıtım prosedürü (ilk dağıtım görevinde).
4. Tasarım (design canvas), sonra kod. Ekranlar çıkınca backend eksikleri birlikte toplanacak.
