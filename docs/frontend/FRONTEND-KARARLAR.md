# Frontend Kararları (ADR biçiminde)

Durum: Kabul edildi. Tarih: 14 Ekim 2026. İsterseniz bu dosyayı `docs/DECISIONS/ADR-025-Frontend-Architecture.md` olarak da kopyalayabilirsiniz.

Öncelik: Bu dosya > `FRONTEND-AGENTS.md` > diğer `docs/frontend/*` > `web-frontend-karar-ozeti.md` > demo reposunun eski dokümanları (`UI_UX.md`, `FRONTEND_*.md`).

## K-01 Başlangıç tabanı: demo repo
**Karar:** `github.com/mucahitunuvar/genclikmerkeziweb` (frontend/) başlangıç tabanıdır. Sıfırdan tasarım çizilmez. Kabuk, bileşenler, CSS yapısı, rota fikirleri ve testler taşınır; mock katmanı silinir; panel ekranları gerçek uçlara göre yeniden yazılır.
**Sonuç:** Demonun `docs/product/UI_UX.md` içindeki görsel yasakları ("cam efekti", "yuvarlak kart") geçersizdir. Demonun `FRONTEND_API_REQUIREMENTS.md` taslağı geçersizdir; gerçek sözleşme `FRONTEND-API-SOZLESMESI.md` ve OpenAPI'dir.

## K-02 Dört uygulama, iki teknoloji
**Karar:**
- `apps/web`: public site, **Next.js (App Router) + React 19, SSR/SEO**.
- `apps/candidate`, `apps/employer`, `apps/admin`: **Vite + React 19 SPA**.
- Ortak paketler: `packages/design-tokens`, `packages/ui`, `packages/api-client`, `packages/auth`.
- Aynı depo (`adgm`), `frontend/` altında pnpm workspace.
**Gerekçe:** Public sitenin arama motoru görünürlüğü gerekir; paneller indekslenmez ve SPA yeterlidir. Demo zaten Vite/React olduğu için paneller en az maliyetle taşınır.
**Admin uygulaması** hem `Admin` hem `CareerAdvisor` rolünü karşılar (menü ve ekranlar yetkiye göre).

## K-03 Tek host, yol bazlı dağıtım
`/` → web (Next), `/portal/aday/**` → candidate, `/portal/firma/**` → employer, `/admin/**` → admin, `/api/**`, `/sitemap.xml`, `/robots.txt`, upload yolları → backend. Üretimde IIS (URL Rewrite + ARR) bu ayrımı yapar. Paneller statik dosya olarak IIS'ten verilir; yalnız `web` Node.js servisi olarak çalışır. Ayrıntı: `FRONTEND-MIMARI.md`.

## K-04 Görsel sistem
- Renkler "bizim" palet: #071f32, #041725, #c44325, #92301b, #e6efed, #ffffff, #102a2e, #0a686b, #d94a2b (+ doğrulanmış türev tonlar, `FRONTEND-TASARIM-SISTEMI.md`).
- Yuvarlak hatlar: orantılı radius (10/16/24/32/pill).
- **Yazı tipi: Manrope (metin) + Poppins (başlık)**, self-host (`@fontsource`). Fraunces/IBM Plex bu projede kullanılmaz.
- Açık/koyu tema; varsayılan sistem tercihi, kullanıcı anahtarı, tarayıcıda saklanan tercih (token değil, yalnız tema).
- **Tailwind kullanılmaz.** Demonun token tabanlı CSS yapısı (CSS değişkenleri + bölünmüş stil dosyaları) korunur. (Önceki "Tailwind" kararı bu kararla geçersizdir; demo taşımayı kolaylaştırır.)
- Glass: varsayılan **kapalı**. Yalnız sticky header bulanıklığı ve görsel üstü kartlarda izin verilir; açık bir ihtiyaç doğarsa token olarak eklenir. (Bkz. `FRONTEND-ACIK-MADDELER.md` #A1.)
- Emoji yok, sahte istatistik yok, boş veri için bölüm gizlenir (public), boş durum bileşeni gösterilir (panel).

## K-05 Gerçek backend, mock yok
OpenAPI (`/openapi/v1.json`) tek sözleşme kaynağıdır. Tipler `openapi-typescript` ile üretilir, elle tip yazılmaz. `MockApiClient`, `mockData.ts`, demo `AuthContext` silinir. Her ekran gerçek uçla çalışır; backend'de olmayan uç için ekran **yapılmaz**, `FRONTEND-ACIK-MADDELER.md` listesine yazılır.

## K-06 Güvenlik
`FRONTEND-GUVENLIK.md` bağlayıcıdır. Backend `SECURITY.md` ile çelişirse SECURITY.md kazanır. Frontend yetkilendirme/güvenlik mekanizması değildir; yalnız kullanıcı deneyimidir (AGENTS.md §25).

## K-07 Oturum (token) modeli: ertelendi
**Karar:** Kalıcı oturum modeli sonra verilecek (aday: HttpOnly refresh çerezi önerilir, backend değişikliği ve CSRF değerlendirmesi gerekir; bkz. SECURITY.md §5 ve §26).
**Geçici kural (Faz 1-5 boyunca):** Access ve refresh token yalnız **JS belleğinde** tutulur. `localStorage`, `sessionStorage`, IndexedDB ve çerez (JS ile) kullanılmaz. Sayfa yenilenince yeniden giriş istenir. Tüm oturum erişimi `packages/auth` içindeki tek `SessionStore` arayüzünden geçer; model değişince yalnız o paket değişir.
**Public site** oturum okumaz (anonim). "Hesabım" ve etkinlik formu ön doldurma, oturum kararı verilene kadar yoktur.

## K-08 Roller (güncellendi: Faz 0 bulgusu)
Backend rolleri: `Admin`, `Employer`, `Candidate`, `CareerAdvisor` (`UserRole`). `GET /api/v1/auth/me` tek bir `role` alanı döner (`userId, email, role, status, createdAtUtc, emailConfirmed`); ayrı yetki listesi **yoktur**. Website yönetim politikalarının hepsi şu an yalnız `Admin` rolünü ister. Demodaki `manager` ve `systemAdministrator` ayrımı backend'de yoktur; ikisi `Admin` altında birleşir. `useCan()` bu yüzden şimdilik **rol tabanlıdır**; backend'e yetki (permission) eklenirse `useCan()` içeriği değişir, çağıran kod değişmez. Rol adları tek bir `roles.ts` içinde, OpenAPI'den gelen değerlerle.

## K-09 Sıra
1. Temel (Faz 0). 2. Giriş/kayıt + Yönetim: Website CMS. 3. Public site (CMS verisiyle). 4. Aday portalı. 5. İşveren portalı. 6. Danışman/eşleştirme ve yönetim operasyonları. 7. Sertleştirme (SEO, erişilebilirlik, performans, dağıtım). Ayrıntı: `FRONTEND-FAZ-PLANI.md`.

## K-10 Public site kararları
Menü yapısı, footer, 12 bölümlü ana sayfa, sayfa envanteri, etkinlik/ilan kuralları, GA4 onay kapısı, pop-up yalnız ana sayfada, sadece Türkçe: `web-frontend-karar-ozeti.md` (13 Ekim) geçerli kalır; bu dosyadaki K-04 (yazı tipi, Tailwind, glass) onu geçersiz kılar. Public sitedeki "Giriş Yap / Üye Ol" yalnız ilgili portalın sayfasına yönlendirir; şifre alanı public sitede yoktur.

## K-11 Bağımlılık disiplini
Backend AGENTS.md §47 mantığı frontend'e de uygulanır: paket eklemeden önce çerçeve zaten sağlıyor mu, bakımlı mı, lisans (MIT/Apache/BSD/ISC) uyumlu mu, gerekli mi sorulur. Onaylı liste `FRONTEND-MIMARI.md` §7'dedir; dışındaki paket için dur ve gerekçe yaz.

## K-12 Dil
Yalnız Türkçe (`<html lang="tr">`). Metinler kodda dağınık değil, ilgili özelliğin içinde sabit; ileride çoklu dil gerekirse public sitede URL öneki (`/en/...`) ile eklenecek şekilde `lang` tek yerde (`siteLanguage.ts`) tutulur.

## K-13 Kapsam dışı (şimdilik)
Docker, mobil uygulama, çoklu dil arayüzü, site içi arama sayfası, public şirket profil sayfası, takvim görünümü, ortak/destekçi bölümü, ödeme/bağış akışı (yalnız IBAN sayfası).
