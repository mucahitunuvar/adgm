# Faz 0 Master Prompt: Frontend Temeli

> Claude Code için görev. Önce `docs/frontend/README.md` okuma sırasını uygula. Bu faz bittiğinde **dur** ve rapor ver. Commit/push yapma.

## Bağlam
Backend (`src/`) olgun ve çalışır durumda. Frontend sıfırdan değil; demo repo (`github.com/mucahitunuvar/genclikmerkeziweb`, `frontend/`) başlangıç tabanıdır. Hedef: dört uygulamalı, ortak paketli, **gerçek backend'e bağlanan** frontend temeli. Mock yok. Kararlar: `FRONTEND-KARARLAR.md`. Kurallar: `FRONTEND-AGENTS.md`, `FRONTEND-GUVENLIK.md`. Yapı: `FRONTEND-MIMARI.md`. Görsel sistem: `FRONTEND-TASARIM-SISTEMI.md`. Taşıma haritası: `DEMO-TASIMA-PLANI.md`.

## Ön kontrol (kodlamadan önce)
1. Okuma sırasını tamamla. Çatışma varsa dur (AGENTS §2).
2. Backend ayakta mı: `GET http://localhost:5016/openapi/v1.json` ve `GET http://localhost:5016/api/v1/public/site`. Ayakta değilse nasıl çalıştırılacağını `docs/DEVELOPMENT.md`'den bul, dur ve bana söyle (çalıştırmayı ben yapacağım).
3. Demoyu **depo dışına** klonla (ör. `../_ref/genclikmerkeziweb`, `--depth 1`). Depoya kopyalama; yalnız seçilen dosyaları dönüştürerek al.
4. Node ve pnpm sürümlerini kontrol et; `engines` alanını yaz.
5. Planı yaz: oluşturulacak/değiştirilecek dosya listesi (en fazla 25 satır). Sonra uygula.

## Görevler

### Görev 1: Workspace iskeleti
- `frontend/` altında pnpm workspace: `pnpm-workspace.yaml`, kök `package.json` (betikler: `dev`, `build`, `typecheck`, `lint`, `test`, `e2e`, `api:gen`, `check:contrast`), `tsconfig.base.json` (strict), ortak ESLint, `.editorconfig`, `.gitignore`, `.env.example`.
- `frontend/AGENTS.md` yoksa `docs/frontend/FRONTEND-AGENTS.md` içeriğinden oluştur; `frontend/CLAUDE.md` tek satır: "AGENTS.md ve docs/frontend/README.md dosyalarını oku."
- `pnpm` kilit dosyası, `onlyBuiltDependencies` ile `postinstall` kısıtı.
- Kabul: `pnpm install` temiz çalışır.

### Görev 2: `packages/design-tokens`
- `tokens.css`: açık/koyu tema, yeni palet, türev tonlar, radius, gölge, boşluk, tipografi ölçeği (TASARIM-SISTEMI). Demonun `tokens.css` yapısı (değişken adlandırma) mümkün olduğunca korunur; krem zemin ve eski renkler gider.
- Manrope + Poppins (`@fontsource`, latin + latin-ext), `base.css` (reset, odak halkası, `prefers-reduced-motion`).
- Tema sınıfı yardımcısı: ilk boyamada flash'sız tema atama için küçük satır içi betik metni ve `theme` anahtarıyla `localStorage` (**tek izinli depolama kullanımı**, yalnız tema).
- `scripts/check-contrast`: TASARIM-SISTEMI kontrast tablosundaki çiftleri token dosyasından okuyup WCAG oranını hesaplar; eşiklerin altında hata verir.
- Kabul: `pnpm check:contrast` yeşil; tablodaki oranlarla uyumlu.

### Görev 3: `packages/ui`
- Demodan taşı + uyarla: `Button`/`ButtonLink`, `FormField`, `AccessibleDialog`, `StatusBadge`, `PageHeader`, `PageState`, `ProgressBar`, `Brand`. Bir bileşen bir dosya. Yuvarlak dil, token'lar, açık/koyu tema, tüm durumlar.
- Yeni: `SafeHtml` (yalnız backend'in temizlediği HTML için; tek `dangerouslySetInnerHTML` yeri), `EmptyState`.
- Demonun testleri (`FormField.test.tsx` vb.) taşınır; yeni bileşenlere test eklenir.
- Kabul: `pnpm --filter @gm/ui test` yeşil; ESLint kuralı `dangerouslySetInnerHTML`'i `SafeHtml` dışında yasaklıyor.

### Görev 4: `packages/api-client`
- `api:gen`: `GET {BACKEND_ORIGIN}/openapi/v1.json` → `packages/api-client/openapi.json` (depoya girer) → `openapi-typescript` ile `schema.d.ts`.
- `createApiClient({ baseUrl, getAccessToken, onUnauthorized })`: tipli `fetch` sarmalayıcı (sunucu/tarayıcı varyantı), `ApiError` (ProblemDetails + `X-Correlation-ID`), `AbortSignal`, sayfalama yardımcısı. UI bilmez.
- Örnek tipli çağrı: `getPublicSite()`. Sözleşme testi: üretilen tip derlenir; 401/429/ProblemDetails senaryoları birim testte.
- Kabul: `pnpm api:gen` tekrarlanabilir; `getPublicSite()` gerçek backend'den dönüyor.

### Görev 5: `packages/auth`
- `SessionStore` arayüzü ve **yalnız bellek** uygulaması (K-07). `login`, `refresh` (eşzamanlı isteklerde tek refresh, kilitli), `logout`, `me`; `AuthProvider`, `ProtectedRoute`, `useCurrentUser`, `useCan`.
- Yetki/rol adları OpenAPI'den; `UserRole` değerleri `Admin, Employer, Candidate, CareerAdvisor`. `GET /auth/me` yanıtından yetkileri okuma biçimini bul ve raporla (manager/systemAdministrator ayrımı için DEMO-TASIMA-PLANI §3).
- Testler: token hiçbir depoya (`localStorage`, `sessionStorage`, çerez) yazılmıyor; refresh kilidi; logout çağrısı ve bellek temizliği; 401 sonrası yönlendirme.
- Kabul: testler yeşil. (Gerçek giriş Faz 1'de.)

### Görev 6: Uygulama iskeletleri
- `apps/web` (Next.js, App Router, TS strict): kök layout (tema, font, `lang="tr"`), güvenlik başlıkları (CSP, nosniff, referrer, permissions, frame-ancestors; FRONTEND-GUVENLIK §4), `output: 'standalone'`, `images.unoptimized`, `rewrites` (`/api/*`, `/sitemap.xml`, `/robots.txt`, upload yolları → `BACKEND_ORIGIN`). `/` sayfası `getPublicSite()` ve `GET /public/home` çağırıp ham sonucu sade biçimde gösterir; hata ve boş durum ele alınır. Oturum okumaz.
- `apps/candidate`, `apps/employer`, `apps/admin` (Vite + React 19 + react-router-dom + react-query): `base` yolları (`/portal/aday/`, `/portal/firma/`, `/admin/`), `server.proxy` (`/api` → backend), demodan taşınan kabuk (`PortalLayout`, `AdminLayout`), tek sayfa: "Bağlantı testi" (`getPublicSite()` sonucu). `X-Robots-Tag: noindex` dev/preview başlıklarında.
- Portlar: 3000, 5171, 5172, 5173. `pnpm dev` hepsini başlatır.
- Kabul: dört uygulama açılır; her biri gerçek backend'den site bilgisini gösterir; açık/koyu tema çalışır; Türkçe karakterler doğru.

### Görev 7: Kalite ve güvenlik güvencesi
- ESLint: `dangerouslySetInnerHTML`, `innerHTML`, `eval`, `localStorage/sessionStorage` (tema yardımcısı dışında) yasak; `no-explicit-any`.
- Playwright smoke: dört uygulamada sayfa açılır, axe ihlali yok (açık/koyu), başlıklar doğru, hiçbir depoda token yok.
- `pnpm audit` çalıştır, sonucu raporla.
- Kabul: `pnpm typecheck && pnpm lint && pnpm test && pnpm build` yeşil; e2e smoke yeşil.

### Görev 8: Rapor
`FRONTEND-AGENTS.md` §11 biçiminde. Ek olarak: (a) demodan taşınan/atlanan dosya listesi, (b) kullanılan Node/pnpm sürümleri, (c) `GET /auth/me` yanıt şekli ve rol/yetki bulguları, (d) OpenAPI'de olmayan ama demo ekranlarında bulunan uçlar (`FRONTEND-ACIK-MADDELER.md` §B için), (e) eklediğin her bağımlılık ve gerekçesi, (f) tüm komutların çıktı özeti.

## Yapma
- Mock veri, sahte kullanıcı, `signInAs` benzeri sahte oturum üretme.
- Tailwind ekleme. Onaylı listede olmayan paket ekleme.
- Tasarımı sıfırdan çizme/yeniden icat etme; yalnız demodaki yapıyı yeni token'lara bağla.
- Backend dosyalarını değiştirme. İhtiyaç görürsen dur ve yaz.
- Commit/push.
