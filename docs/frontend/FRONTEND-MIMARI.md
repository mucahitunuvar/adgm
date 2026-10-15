# Frontend Mimarisi

## 1. Genel görünüm

```text
Tarayıcı ──► IIS (tek host)
             ├─ /                      → apps/web        (Next.js, Node servisi)
             ├─ /portal/aday/**        → apps/candidate  (statik SPA)
             ├─ /portal/firma/**       → apps/employer   (statik SPA)
             ├─ /admin/**              → apps/admin      (statik SPA)
             └─ /api/**, /sitemap.xml, /robots.txt, upload yolları → GenclikMerkezi.Api
```

CORS kullanılmaz (aynı origin). Geliştirmede aynı yönlendirmeyi dev sunucular yapar (§5).

## 2. Depo yapısı

```text
adgm/
├── src/  tests/  docs/            # backend (dokunulmaz)
└── frontend/
    ├── AGENTS.md  CLAUDE.md
    ├── pnpm-workspace.yaml  package.json  tsconfig.base.json  eslint.config.js
    ├── apps/
    │   ├── web/         # Next.js public site
    │   ├── candidate/   # Vite SPA, base: /portal/aday/
    │   ├── employer/    # Vite SPA, base: /portal/firma/
    │   └── admin/       # Vite SPA, base: /admin/
    ├── packages/
    │   ├── design-tokens/  # tokens.css (açık/koyu), fontlar, base reset
    │   ├── ui/             # Button, FormField, AccessibleDialog, StatusBadge, PageState, DataTable...
    │   ├── api-client/     # openapi.json, üretilen tipler, fetch sarmalayıcı, ProblemDetails
    │   ├── auth/           # SessionStore arayüzü, Auth sağlayıcı, ProtectedRoute, rol yardımcıları
    │   └── rich-text/      # Tiptap tabanlı zengin metin editörü (A11) - backend whitelist'ine kısıtlı şema
    └── scripts/            # api-gen, seed-dev, check-contrast
```

Uygulamalar birbirini içe aktarmaz. `ui` ← `design-tokens`; `auth` ← `api-client`; uygulamalar → paketler.

## 3. Uygulamalar

### apps/web (Next.js, SSR)
- App Router, TypeScript strict, `output: 'standalone'`, `images.unoptimized: true` (backend sabit boyut varyantları `srcset` ile kullanılır).
- Rotalar: açık rotalar (`/ilanlar`, `/ilanlar/[slug]`, `/etkinlikler`, `/giris`, `/uye-ol`, form ve sistem sayfaları) + `[[...slug]]` yakalayıcı. Yakalayıcı yolu `GET /api/v1/public/routes/resolve` ile çözer (Home/Detail/Listing/Redirect/NotFound).
- Ana sayfa `GET /public/home` bloklarından çizilir; blok tipi → bileşen kayıt defteri (registry). Bilinmeyen tip atlanır, boş bölüm çizilmez.
- Önbellek: sunucu `fetch` ile süreye dayalı ~60 sn (`site`, `home`, içerik). Arama, ilan filtreleri, etkinlik kontenjan durumu, yetkili her şey `no-store`. `NotFound` önbelleğe girmez.
- Oturum okumaz; anonim.
- `sitemap.xml`, `robots.txt` Next'te üretilmez, backend'den proxy'lenir.
- Ayrıntılı API kuralları: `FRONTEND-API-SOZLESMESI.md`.

### apps/candidate, employer, admin (Vite SPA)
- React 19, `react-router-dom`, lazy rota bölme, `base` yolları yukarıdaki gibi.
- Sunucu durumu `@tanstack/react-query`; her sorgu tek bir `queryKey` fabrikasıyla. Yetkili istekler `no-store` semantiği (tarayıcı HTTP önbelleğine güvenilmez, `Cache-Control` backend'den).
- Rota koruması `packages/auth` içindeki `ProtectedRoute` (yalnız UX). 401 gelirse refresh dener, olmazsa girişe yönlendirir; 403 "yetkisiz" sayfasını gösterir.
- Her uygulama `index.html` + hash'li statik varlık üretir; IIS SPA fallback kuralıyla servis edilir.
- **admin** `Admin` ve `CareerAdvisor` rollerini karşılar; menü yetkiye göre; ekranlar özellik klasörleri (`features/website-cms`, `features/career-advisor`, `features/users`, `features/companies`, `features/support`...).

## 4. Paylaşılan paketler
- **design-tokens:** tek kaynak; tüm renk/boyut/radius/tipografi değişkenleri ve tema sınıfları. Uygulamalar buradan CSS içe aktarır.
- **ui:** sunumsal React bileşenleri; iş mantığı, API, yönlendirme bilgisi yok. Next'te de Vite'ta da çalışır (istemci etkileşimi gereken bileşenlerde `'use client'`).
- **api-client:** `openapi.json` (depoda), üretilen `schema.d.ts`, tipli `fetch` sarmalayıcı (ortam: sunucu/tarayıcı), `ProblemDetails` → `ApiError` dönüştürücü, sayfalama ve `Idempotency-Key` yardımcıları, `X-Correlation-ID` okuma. UI bilmez.
- **auth:** `SessionStore` arayüzü (K-07: şimdilik bellek uygulaması), `login/logout/refresh/me` çağrıları, `AuthProvider`, `ProtectedRoute`, `useCurrentUser`. UI içermez (yalnız React bağlama).
- **rich-text:** Tiptap/ProseMirror tabanlı zengin metin editörü (A11). Şema, backend `IHtmlContentSanitizer` whitelist'iyle birebir sınırlı (iframe/YouTube gömme hariç - Faz 3'e ertelendi). API çağrısı içermez; görsel ekleme tüketici uygulamanın verdiği bir geri çağırım (`onRequestImage`) üzerinden MediaPicker'a devredilir.

## 5. Geliştirme ortamı
- Backend: `http://localhost:5016`. Sunucu tarafı fetch ve tarayıcı bu adrese **proxy** üzerinden gider.
- Next: `rewrites` ile `/api/*`, `/sitemap.xml`, `/robots.txt`, upload yolları → `BACKEND_ORIGIN`.
- Vite: `server.proxy` ile `/api` → `BACKEND_ORIGIN`.
- Portlar: web 3000, candidate 5171, employer 5172, admin 5173. Hepsi tek komutla: `pnpm dev` (ya da `pnpm --filter <app> dev`).
- Ortam değişkenleri `frontend/.env.example`'da belgelenir; gerçek `.env` depoya girmez.

## 6. Üretim / dağıtım (Windows Server, Turhost)
- Paneller: `pnpm build` çıktısı (`dist/`) IIS site klasörlerine kopyalanır; URL Rewrite SPA fallback.
- Web: `next build` + `standalone` çıktısı Windows servisi olarak çalışır (WinSW/NSSM seçimi `FRONTEND-ACIK-MADDELER.md` #A4). IIS ARR ters vekil `/` isteklerini Node'a, `/api` ve kök dosyalarını backend'e yollar.
- `Website:PublicSiteBaseUrl` doğru ayarlanmalı (sitemap/JSON-LD URL'leri).
- Ayrıntılı prosedür Faz 6 çıktısıdır (`DEPLOY-WINDOWS.md`).

## 7. Onaylı bağımlılıklar (K-11)

| Paket | Nerede | Neden |
|---|---|---|
| react, react-dom (19) | hepsi | UI |
| next | web | SSR/SEO |
| vite, @vitejs/plugin-react | paneller | derleme |
| react-router-dom | paneller | yönlendirme (demoda var) |
| @tanstack/react-query | paneller | sunucu durumu, önbellek, yeniden deneme |
| openapi-typescript | api-client | tip üretimi |
| lucide-react | hepsi | ikon (az) |
| @fontsource/manrope, @fontsource/poppins | design-tokens | self-host yazı tipi |
| embla-carousel-react | web | slider (MIT) |
| motion | web | ölçülü animasyon (`prefers-reduced-motion`) |
| typescript, eslint, typescript-eslint, eslint-plugin-react-hooks | hepsi | kalite |
| vitest, @testing-library/*, jsdom | hepsi | birim/bileşen testi |
| @playwright/test, @axe-core/playwright | hepsi | e2e ve erişilebilirlik |
| pnpm | araç | workspace |
| @tiptap/core, @tiptap/pm, @tiptap/react, @tiptap/extension-document/paragraph/text/bold/italic/underline/strike/heading/bullet-list/ordered-list/list-item/list-keymap/blockquote/link/image/table/table-row/table-header/table-cell/horizontal-rule/hard-break/history/gapcursor/dropcursor (3.32.0, tam sabitlenmiş) | `packages/rich-text` | Zengin metin editörü (A11, Dilim 2a-2 Görev 1). MIT; hiçbir Pro/cloud/collaboration paketi yok. Şema yalnız backend `IHtmlContentSanitizer` whitelist'ine (ADR-024 §16) karşılık gelen düğüm/işaretleri içerir - `@tiptap/starter-kit` kasıtlı olarak kullanılmaz (whitelist dışı `code`/`codeBlock` getirir). `apps/admin`'de yalnız içerik düzenleme rotasında `React.lazy` ile yüklenir (ölçüm: `FRONTEND-ACIK-MADDELER.md` A11). |

Form kütüphanesi eklenmez: yerel form durumu + backend doğrulaması (ProblemDetails alan hatları). Sürükle-bırak, tarih gibi ihtiyaçlar çıkınca dur ve gerekçeyle öner.

## 8. Performans bütçesi (başlangıç)
`web`: LCP < 2.5 sn (4G mobil profili), ilk yükte rota başına JS < 170 KB gzip, CLS < 0.1. Paneller: ilk yük < 300 KB gzip, rota başına lazy bölme. Ölçüm ve değişiklik gerekçeyle yapılır (AGENTS.md §39 mantığı: ölç, kök neden, düzelt, yeniden ölç).
