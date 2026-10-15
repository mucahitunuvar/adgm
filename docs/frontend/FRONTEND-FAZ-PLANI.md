# Frontend Faz Planı

Sıra kararı (K-09): Temel → Kimlik + Yönetim kabuğu → Website CMS → Public site → Aday → İşveren → Danışman/eşleştirme ve yönetim operasyonları → Sertleştirme/dağıtım.
Neden CMS, public siteden önce: public site ancak CMS'ten girilen gerçek içerikle anlamlı görünür; CMS ekranları backend'de en olgun alandır.

Boyut: S (birkaç saat), M (1-2 gün), L (3-5 gün), XL (1 hafta+). Boyutlar Claude Code ile çalışırken yaklaşık oturum yoğunluğudur, söz değildir.
Her faz: planı yaz → uygula → doğrula → **dur ve raporla**. Rapor biçimi `FRONTEND-AGENTS.md` §11.

---

## Faz 0: Temel (M-L)
Ayrıntı: `FRONTEND-FAZ0-MASTER-PROMPT.md`.
Kapsam: pnpm workspace, `design-tokens` (yeni palet/radius/Manrope-Poppins/tema), `ui` (demo bileşenleri), `api-client` (OpenAPI tip üretimi, `fetch` sarmalayıcı), `auth` (bellek `SessionStore`), 4 uygulama iskeleti (web: Next; paneller: Vite), dev proxy, kalite araçları (kontrast betiği, güvenlik ESLint kuralları), smoke e2e.
Kabul: `pnpm dev` ile dört uygulama ayağa kalkar; her biri gerçek backend'den anonim bir uç çağırır (`GET /public/site`); `typecheck/lint/test/build` yeşil; kontrast betiği yeşil; token'lar bellekten başka yerde yok (test).

## Faz 0 durumu: tamamlandı (14 Ekim)
4 paket + 4 uygulama, gerçek backend bağlantısı, 22 birim + 15 e2e test, kontrast ve audit yeşil. Bulgu: OpenAPI yanıt şemaları Identity ve Website için eksik (ACIK-MADDELER B15).

## Faz 0.5: Backend OpenAPI düzeltmesi (S-M, backend işi)
`docs/frontend/FRONTEND-OPENAPI-PRODUCES-PROMPT.md`. Dilimli ve faz önkoşulu: Görev 1 Identity (**tamamlandı**) → Faz 1; Görev 3 Website admin → Faz 2; Görev 2 Website public → Faz 3; Görev 4a Candidate+ReferenceData → Faz 4; 4b Employer → Faz 5; 4c CareerAdvisor/admin operasyon/Support → Faz 6.

## Faz 1: Kimlik + Yönetim kabuğu (M)
Kapsam: `apps/admin` giriş, çıkış, parola sıfırlama, e-posta doğrulama; `GET /auth/me` ile menü ve `useCan()`; 401/refresh/403 davranışı; admin kabuğu (AdminLayout, menü, kullanıcı menüsü); kullanıcı yönetimi (`/auth/admin/users`: liste, detay, rol, aktif/pasif, kilit açma) ve denetim kaydı listesi.
Kabul: Gerçek bir Admin ile giriş; süresi dolan access token otomatik yenilenir (eşzamanlı isteklerde tek refresh); yetkisiz sayfa 403 ekranı; yenilemede oturum biter (K-07); token hiçbir depoda yok (e2e ile).
Önkoşul: Geliştirmede ilk Admin kullanıcısı (bkz. ACIK-MADDELER A9).

## Faz 2: Website CMS, yönetim ekranları
**2a Çekirdek (L-XL):** genel ayarlar (`settings/*` tema, iletişim, özellikler, bakım, bot koruması, çerez), diller, **içerik tipleri** ve kategoriler/etiketler, **içerik** (liste, filtre, oluştur/düzenle, çeviri sekmeleri, galeri, video, ek dosya, yayın/zamanlama/arşiv, kopyalama, revizyon geçmişi ve geri yükleme, önizleme bağlantısı), **medya kütüphanesi** (+ klasörler), **menüler** (Header/Utility/Footer, iç içe), **slider**, **rakamlar şeridi**, **ana sayfa düzeni** (blok tipleri `GET /admin/website/block-types`, taslak/yayın/önizleme), **pop-up**.
**2b Etkileşim (L):** formlar (alan tanımı) ve başvurular (liste, atama, durum, not, dosya, arşiv), etkinlikler (takvim bilgisi, kayıt yönetimi: onay/yedek/red/iptal/katıldı/gelmedi, CSV dışa aktarım), yasal metinler (sürüm), bülten aboneleri, yönlendirmeler ve 404 yolları, üçüncü taraf script sağlayıcıları, arama kaynakları, çerez özeti, kişisel veri erişim kaydı.
Zengin metin düzenleyici: **düzenleyici seçimi bu fazda ayrı karar** (lisans: MIT/Apache; çıktı backend sanitizer ile uyumlu; ACIK-MADDELER A11). Seçilene kadar metin alanı düz textarea + önizleme.
Kabul (2a): CMS'ten girilen menü, slider, rakamlar ve ana sayfa düzeni `GET /public/site` ve `/public/home` yanıtında görünür; yayına alınmamış içerik public'te görünmez; iki kullanıcı aynı içeriği düzenlerse 409/rowversion durumu kullanıcıya açıkça gösterilir.

## Faz 3: Public site (`apps/web`) (XL)
Kapsam: Next iskeleti (Faz 0'dan), header/menü/mobil çekmece/footer (backend menüsü), tema anahtarı, çerez bandı, 12 bölümlü ana sayfa (blok registry; **önce blok eşleme tablosu `docs/frontend/BLOK-ESLEMESI.md`, onay için dur**), şablonlar (kart liste+detay, içerik sayfası, Yönetim ve Ekip, Faaliyet Raporları, Dökümanlar, ilan liste/detay, etkinlik liste/detay ve kayıt/doğrulama/iptal, `/giris`, `/uye-ol`, form sayfaları, yasal metin, 404/bakım/teşekkür/bülten onayı), SEO (metadata, JSON-LD, sitemap/robots proxy), GA4 onay kapısı, pop-up.
Kabul: CMS'ten girilen içerik public'te SSR ile görünür (kaynağı görüntüle ile doğrulanır); sitemap/robots backend'den gelir; Lighthouse ve axe taraması (açık/koyu, 390/1440); boş veritabanında bölümler gizlenir; performans bütçesi.
Geliştirme verisi yetersizse `scripts/seed-dev` (gerçek admin uçlarıyla) yazılır.

## Faz 4: Aday portalı (`apps/candidate`) (L-XL)
Kapsam: kayıt (18+ bilgisi backend'e göre), giriş, e-posta doğrulama; dashboard; adımlı profil/CV (kişisel, iletişim, eğitim, deneyim, dil, sertifika, referans, özet, fotoğraf, CV dosyası); CV ön izleme/dışa aktarım; görüşme talepleri (onay/öneri); ayarlar ve parola değiştirme. Backend'de ucu olmayan demo ekranları (fırsatlar, teklifler, bildirimler, veri talepleri) yapılmaz, kayıt altına alınır.
Kabul: Gerçek adayla uçtan uca profil tamamlama; yalnız kendi verisi; hata durumları.

## Faz 5: İşveren portalı (`apps/employer`) (L)
Kapsam: kayıt, giriş; firma (`my-company`, logo, logo görünürlüğü); ilan (taslak, düzenle, danışman onayına gönder); personel ihtiyacı (taslak, gönder); görüşmeler.
Kabul: Gerçek işverenle firma → ilan → gönderim; yayınlanan ilanın `public/jobs` içinde görünmesi (onay sonrası).

## Faz 6: Danışman, eşleştirme ve yönetim operasyonları (`apps/admin`) (XL)
Kapsam: danışman paneli (aday önerileri, aday notları/hedefleri/gelişim planları/beceri açıkları/eğitim önerileri, genel havuz, kendi havuzum, personel ihtiyacı önerileri, ilan inceleme onay/ret/revizyon, görüşme planlama/sonuç, istihdam takibi), yönetim (firma onay/ret/devret/pasife al, ilan askıya al/geri al, danışman yönetimi, aday yeniden atama, toplu bildirim), destek biletleri.
Kabul: Rol bazlı kapsam (danışman yalnız kendi kapsamı) frontend'de yetkiye göre, gerçek doğrulama backend'de.

## Faz 7: Sertleştirme ve dağıtım (M-L)
Kapsam: erişilebilirlik taraması (tüm uygulamalar), performans, CSP/başlık doğrulama, `pnpm audit`, Windows dağıtım (IIS kuralları, Next standalone servisi), `DEPLOY-WINDOWS.md`, ortam değişkenleri belgeleri, hata izleme kararı, yedek/geri dönüş notları.

---

## Çapraz kurallar
- Her fazın ekran listesi OpenAPI'de ucu olanlarla sınırlıdır.
- Faz bitmeden sonrakine geçilmez; ama panel fazları (4, 5, 6) birbirinden bağımsızdır ve sırası backend olgunluğuna göre değişebilir.
- Her faz sonunda: `typecheck`, `lint`, `test`, `build`, `pnpm audit`, axe, kontrast betiği.
- Her faz sonunda `docs/frontend/FRONTEND-ACIK-MADDELER.md` güncellenir.
