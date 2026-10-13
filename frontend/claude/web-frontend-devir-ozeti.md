# Public web sitesi frontend: devir özeti (12 Ekim 2026)

## Durum
- Backend: Website modülü ADR-024 Faz 0-5, Faz 4 (etkinlik) ve Faz 5 (arama/sitemap/schema.org/revizyon) bitti; Employer public ilan ön hazırlığı bitti. origin/main son bilinen commit 6a4682e, 2817/2817 test yeşil.
- Frontend: 4 React uygulaması planlı (public site, aday portalı, firma portalı, yönetim portalı). İlk uygulama public site. Sıra: önce mimari/karar, sonra tasarım, sonra kod.
- Kullanıcı mimariyi önce çıkarıp karar verip öyle başlıyor. Hızlı tasarıma geçilmesini istemedi.

## Verilen kararlar
- Görsel yön B "Modern güven": koyu lacivert/mürekkep, tek canlı vurgu (turkuaz ya da limon, limon parlak kalabilir ama denenecek), keskin editoryal tipografi, bol boşluk. Düz "AI sitesi" görünümü istenmiyor.
- Sadece Türkçe, varsayılan dil Türkçe; sonra diğer diller eklenebilir.
- Üst barda "Giriş yap" ve "Üye ol": her ikisinde aday / firma seçimi, ilgili portalın giriş/üye ol sayfasına yönlendirme.
- Eklentiler (Motion, UI UX Pro Max, 21st): ücretsiz ve faydalıysa kullanılır. Benim önerim: Motion ölçülü (MIT, ücretsiz), UI UX Pro Max sadece kontrol listesi (MIT, Python 3 ister), 21st atla (API anahtarı, fiyat belirsiz). Kullanıcı faydasını merak ediyor, karar bekliyor.

## Bu sohbette yapılan taslak (kullanıcı istemedi, bağlayıcı değil)
- Design canvas: https://claude.ai/artifact/NUJcsmb6iKyz9mtSN7khHS ; tek tahta Main.dc.html (ana sayfa).
- Geri bildirim: çok yazılı, neredeyse hiç görsel yok; menü detaylandırılmadı; ana sayfa akışı önce belirlenmeli. Taslak sadece fikir; mimari kararlardan sonra yeniden yapılacak.

## Sıradaki iş (yeni sohbette, tasarımdan ÖNCE)
1. Menü yapısı: ana menü, alt menüler, mobil menü, alt bilgi menüsü; hangi içerik tipi nereye bağlanır (Website modülünde menü yönetimi var).
2. Ana sayfa akışı: bölüm sırası, her bölümün amacı, hangi blok tipi (15 blok kataloğu) ve hangi veri kaynağı.
3. Sayfa envanteri ve şablonlar: içerik liste/detay, etkinlik liste/detay/kayıt durumları, ilan liste/detay, şirket profili, iletişim, arama, 404/bakım, çerez bandı.
4. Teknik kararlar: SSR/SEO yaklaşımı, giriş durumu ve oturum paylaşımı (token saklama, SECURITY.md), portalların aynı host altında olması (reserved yollar: api, admin, portal, ...), frontend bağımlılık kararı (backend AGENTS.md §47 mantığı).
5. Sonra görsel yön: gerçek görsel/fotoğraf stratejisi, ikon/illüstrasyon, eklenti kararı; en son tasarım ve kod.
6. Ekranlar çıkınca backend eksikleri birlikte toplanacak.

## Açık backend işleri
- robots.txt: `Disallow: /` yalnızca `AllowSearchEngineIndexing = false` iken (GetRobotsTxtQueryHandler koşulundan MaintenanceModeEnabled çıkacak; testler ve ADR-024 §13/§15 metni düzelecek). Talimat kullanıcıya verildi, Claude Code'da çalıştırılacak.
- İlana başvuru, mülakat planlama bildirimi, hatırlatma/erteleme, Interview/Employment/Matching ADR'leri, Faz 4 mimari testi (public yanıt EventRegistration'a başvurmasın).
- Bakım modunun 503 + Retry-After döndürdüğü doğrulanmadı.
- docs/prompts altındaki 3 prompt dosyası (Faz4, Faz5, Employer public) kullanıcı tarafından commit edilecek.

## Referans
- Repo: github.com/mucahitunuvar/adgm ; prototip: genclikmerkeziweb (Vite + React 19, merkez adı "Ahmet Demir Gençlik Merkezi").
- Public API: /api/v1/public/...; sitemap.xml ve robots.txt kökte; Job.Slug ilk yayında atanır ve değişmez; ilan detayında askerlik tercihi görünür, cinsiyet tercihi hiçbir public yanıtta yok.

> Not (13 Ekim): "Sıradaki iş" 1-4 ve eklenti kararı tamamlandı, kararlar `claude/web-frontend-karar-ozeti.md` içinde. Bu dosya geçmiş bağlam ve açık backend işleri için tutuluyor.
