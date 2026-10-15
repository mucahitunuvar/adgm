# Demo Taşıma Planı (genclikmerkeziweb → adgm/frontend)

Kaynak: `github.com/mucahitunuvar/genclikmerkeziweb`, `frontend/` (Vite 8, React 19, react-router-dom 7, Vitest, Playwright; 140 dosya, ~1.350 satır sayfa kodu, mock veri).
Hedef: `adgm/frontend/` (pnpm workspace, K-02).

Karar türleri: **Taşı** (bileşen/kod yeni yapıya uyarlanıp alınır), **Yeniden yaz** (fikir alınır, gerçek uçlarla baştan), **Sil**.

## 1. Katman katman

| Demo | Karar | Hedef / Not |
|---|---|---|
| `src/styles/tokens.css` | Yeniden yaz | `packages/design-tokens/tokens.css`: yeni palet, radius 10/16/24/32, Manrope/Poppins, açık+koyu tema, türev tonlar (bkz. TASARIM-SISTEMI). Krem `#f6f3ec` zemin kalkar. |
| `src/styles/base.css`, `layouts.css`, `pages.css`, `inner-pages.css`, `public-layout.css`, `home.css`, `index.css` | Taşı + dönüştür | Bölünmüş CSS yapısı korunur. Keskin kenarlar → radius token'ları; sabit renk değerleri (`#071f32`, `#fff`...) token değişkenlerine; 1px grid ızgaralar (hızlı erişim) kartlara dönüşür. Dosyalar özellik/uygulama bazında dağılır. |
| `components/ui/Button.tsx`, `FormField.tsx`, `AccessibleDialog.tsx`, `StatusBadge.tsx`, `PageHeader.tsx`, `PageState.tsx`, `ProgressBar.tsx`, `PublicPageHero.tsx` | Taşı | `packages/ui`. Testler (`FormField.test.tsx`) birlikte. `PublicPageHero` → `apps/web`. |
| `components/layout/PublicLayout.tsx` | Yeniden yaz | `apps/web` header/menü/footer: menü verisi **backend `GET /public/site`**'ten; demoda sabit. Mobil menü Header'dan türetilir. Yapı/dil demodan. |
| `components/layout/PortalLayout.tsx` | Taşı | `apps/candidate` ve `apps/employer` kabuğu (ortak parçalar `packages/ui`). |
| `components/layout/AdminLayout.tsx` | Taşı | `apps/admin` kabuğu; menü `GET /auth/me` yetkilerine göre. |
| `components/layout/Brand.tsx` | Taşı | `packages/ui` (logo yer tutucusu; gerçek logo gelince değişir). |
| `features/auth/AuthContext.tsx` | **Sil** | Demo `signInAs(role)` sahte oturum. Yerine `packages/auth`. |
| `features/auth/ProtectedRoute.tsx` | Yeniden yaz | `packages/auth`; yetki `useCan()`. |
| `features/shared/mockData.ts`, `services.ts`, `models.ts`, `ServiceContext.tsx` | **Sil** | Yerine OpenAPI tipleri + `api-client`. |
| `lib/api/ApiClient.ts`, `MockApiClient.ts`, `ApiError.ts`, `MockApiClient.test.ts` | Yeniden yaz | `packages/api-client` (gerçek `fetch`, ProblemDetails). `ApiError` fikri alınır. |
| `lib/hooks/useAsyncData.ts` | Sil | `@tanstack/react-query` (paneller) / sunucu bileşenleri (web). |
| `app/App.tsx`, `routes.ts` | Yeniden yaz | Uygulama başına rota tanımı; demo rota kataloğu fikir olarak alınır (`FRONTEND_ROUTE_MAP.md`). |
| `main.tsx`, `index.html`, `vite.config.ts`, `tsconfig*`, `eslint.config.js`, `playwright.config.ts` | Taşı + uyarla | Her panel uygulamasına; `base` yolları, proxy, ortak `tsconfig.base.json`. |
| `e2e/*.spec.ts` | Taşı + uyarla | Gerçek backend ile çalışacak şekilde; axe testleri korunur. |
| `public/media/*.jpg` | Geçici | Yalnız geliştirme yer tutucusu; lisansı belirsiz (raporda işaretle). |

## 2. Sayfa sayfa

### Public (`apps/web`)
| Demo sayfa | Karar | Not |
|---|---|---|
| `HomePage.tsx` (192 satır: hero slider, hızlı erişim, duyuru, etkinlik, ilan) | Yeniden yaz (bölüm yapıları taşınır) | `GET /public/home` blokları → registry. Hero slider davranışı (ok, nokta, duraklat) alınır; Embla'ya geçer. Hızlı erişim ızgarası backend'de blok karşılığı yoksa kaldırılır. 12 bölümlü akış karar özetinden. |
| `ListingPage`, `DetailPage`, `PublicContentPage` (25-41 satır) | Yeniden yaz | İçerik tipi liste/detay + içerik sayfası; veri `contents`, `routes/resolve`. |
| `PublicFormPage` (54 satır) | Yeniden yaz | Form motoru: `forms/{key}` alan tanımı, token, honeypot, Turnstile. |
| `LegalPage`, `SystemPages` | Yeniden yaz | `legal-documents`, 404/bakım/teşekkür. |
| `publicContent.ts` | **Sil** | Sabit metinler; içerik backend'den. |

### Giriş/kayıt
| Demo | Karar | Not |
|---|---|---|
| `pages/auth/AuthPage.tsx` (54 satır, rol seçerek demo oturum) | Yeniden yaz | Her panelin kendi giriş/kayıt/e-posta doğrulama/parola sıfırlama sayfaları, gerçek `/auth/*` uçlarıyla. Public `/giris` ve `/uye-ol` yalnız yönlendirme. |

### Aday portalı (`apps/candidate`)
Demo sayfalar: `CandidateDashboardPage`, `CandidateProfilePage`, `ProfileStepPage`, `CandidateOperationPage` (15-48 satır, genel şablon). **Yeniden yaz.** Fikir alınır: adımlı profil sihirbazı, ilerleme çubuğu, CV ön izleme. Gerçek kaynaklar: `personal-info`, `contact-info`, `content/*`, `photo`, `cv/export`, `meeting-requests`, `interviews`. Demodaki teklif/yerleştirme/fırsat ekranları, ilgili backend uçları OpenAPI'de olduğu ölçüde yapılır.

### İşveren portalı (`apps/employer`)
Demo: `EmployerDashboardPage`, `EmployerFormPage`, `EmployerOperationPage`. **Yeniden yaz.** Gerçek kaynaklar: `my-company`, `jobs` (+ `submit`), `personnel-needs`, `interviews`, `logo-visibility`.

### Yönetim (`apps/admin`)
Demo: `AdminDashboardPage`, `AdminListPage`, `AdminDetailPage`, `AdminOperationPage`, `MatchingPage` (18-32 satır). **Genel liste/detay/işlem kalıpları taşınır, ekranlar yeniden yazılır.** Önce Website CMS (`/admin/website/*`), sonra kullanıcılar, firmalar, ilanlar onayı, danışman/eşleştirme.

## 3. Rol eşlemesi
Demo: `candidate, employer, advisor, manager, systemAdministrator`. Backend: `Admin, Employer, Candidate, CareerAdvisor`. `manager`/`systemAdministrator` ayrımı backend'de yetki ise `GET /auth/me` ve politika tanımlarından okunur (`WebsitePolicies.cs` vb.); değilse `Admin` altında birleşir. Karar Faz 1'de OpenAPI/Identity koduna bakılarak raporlanır.

## 4. Taşıma sırası
1. Workspace + `design-tokens` (renkler, yazı tipi, radius) → demo CSS bu token'lara bağlanır.
2. `packages/ui` bileşenleri + testleri.
3. `api-client` + `auth`.
4. Paneller: kabuklar (layout'lar), sonra ekranlar (Faz 1+).
5. Web: Next iskeleti, header/footer, ana sayfa (Faz 3).
6. Demo mock dosyaları silinir; son kontrol: depoda `mock`, `demo`, `Demo kayıt` ifadeleri kalmadığı (grep) raporlanır.

## 5. Kabul
- Yeni depoda `MockApiClient`/`mockData`/demo `AuthContext` yok.
- Tüm ekranlar gerçek uçla veya "uç yok" notuyla (`FRONTEND-ACIK-MADDELER.md`) çalışır.
- Renkler yalnız token'lardan; sabit hex kalmadı (token dosyası hariç) — ESLint/stylelint veya grep ile doğrulanır.
- Demonun testleri (en az `FormField`) yeni pakette geçiyor.
