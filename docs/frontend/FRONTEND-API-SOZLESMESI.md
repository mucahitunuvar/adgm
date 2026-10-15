# Frontend–Backend Sözleşmesi

**Kaynak gerçek:** OpenAPI (`GET /openapi/v1.json`, geliştirmede). Bu belge uçların **haritasıdır** ve frontend'in uyacağı kuralları verir; ayrıntı (alanlar, kodlar) için üretilen tipler esastır. Çelişkide OpenAPI kazanır; fark bulunursa `FRONTEND-ACIK-MADDELER.md`'ye yazılır.
Harita 14 Ekim 2026'da `adgm` ana dalından (commit 6a4682e) çıkarılmıştır.

## 1. Ortak kurallar
- Önek `/api/v1`. JSON `camelCase`. Tarihler ISO 8601 UTC; arayüzde Europe/Istanbul.
- Hatalar: 400/404/409 `application/problem+json` (ProblemDetails; alan hataları `errors`). **401, 403 ve 429 middleware'den boş gövdeli gelir**, ProblemDetails varsayma. 422 backend'de kullanılmıyor. 5xx güvenli genel hata.
- Yanıtta `X-Correlation-ID` varsa kullanıcıya "destek kodu" olarak gösterilir, log'a yazılır.
- Sayfalama biçimini OpenAPI belirler (liste yanıtı tipinden üretilir); elle varsayma.
- Rate limit politikaları: `public-read`, `public-forms`, `public-search`. 429'da `Retry-After`'a saygı, otomatik yeniden deneme yok (kullanıcıya bildir).
- Mutasyonlarda çifte gönderimi önle; backend gerektiriyorsa `Idempotency-Key`.

## 2. Kimlik (`/auth`, `/candidates/register`, `/employer/register`)
| Uç | Not |
|---|---|
| `POST /auth/login` | Yanıt: `accessToken`, `accessTokenExpiresAtUtc`, `refreshToken`, `refreshTokenExpiresAtUtc`, `emailConfirmed`. Token modeli K-07. |
| `POST /auth/refresh-token` | Refresh döngüsü (rotation); aynı token iki kez gönderilmez. |
| `POST /auth/logout` | Backend tarafında oturumu sonlandırır. |
| `GET /auth/me` | Kullanıcı, rol(ler), yetkiler. Menü ve `useCan()` buradan. |
| `POST /auth/register`, `POST /candidates/register`, `POST /employer/register` | Kayıt akışları; hangisinin hangi portalda kullanıldığı OpenAPI/Identity dokümanına göre. |
| `POST /auth/verify-email`, `GET /auth/verify-email`, `POST /auth/resend-verification-email` | E-posta doğrulama. Token loglanmaz. |
| `POST /auth/forgot-password`, `POST /auth/reset-password`, `PUT /auth/change-password` | Parola akışları; anti-enumeration: "e-posta kayıtlıysa gönderildi" mesajı sabit. |
| `/auth/admin/users*`, `/auth/admin/audit-log` | Kullanıcı yönetimi (aktif/pasif, rol, kilit açma), denetim kaydı. |

Roller: `Admin`, `Employer`, `Candidate`, `CareerAdvisor`.

## 3. Public site (`/public/...`, anonim)
`site`, `home`, `routes/resolve`, `contents`, `contents/{id}`, `preview/{token}`, `events`, `events/{id}/calendar.ics`, `events/{id}/registrations`, `event-registrations/{verifications|verification-resends|cancellations}`, `jobs`, `jobs/{slug}`, `companies/{id}` (kullanılmaz), `companies/{id}/logo`, `forms`, `forms/{key}`, `submission-tokens`, `forms/{key}/submissions`, `newsletter/{subscriptions|confirmations|unsubscriptions}`, `legal-documents/{key}` (+ sürüm), `cookie-consents`, `videos`, `partners` (kullanılmaz), `search` (kullanılmaz). Kökte `sitemap.xml`, `robots.txt`.

Kurallar (ADR-024):
- **Rota çözümleme:** `[[...slug]]` → `routes/resolve?path=`; Home/Detail/Listing/Redirect/NotFound. Kanonik yönlendirme (301) aynen izlenir. Bulunamayan yol backend'de 404 kaydıdır (frontend ayrıca bir şey yapmaz).
- **Ana sayfa:** `home` blokları sıralı döner; gizleme backend'de yapılır. Bilinmeyen blok tipi atlanır.
- **İçerik detayı:** `contents/{id}` galeri, video, ek dosya, breadcrumb, `hreflang`, SEO ve `jsonLd` verir; frontend yalnız basar (`serializeJsonLd`).
- **Etkinlik:** `registrationState` (NotOpen, Open, Full, WaitlistOpen, Closed, Cancelled) ve `remainingSpots` backend'den; frontend hesaplamaz, tarayıcıda canlı çeker. **Online bağlantı public yanıtlarda yoktur.** Yinelenen kayıt yanıtı bilgi sızdırmaz, mesaj aynı kalır. İptal bağlantısı idempotent.
- **Formlar:** `forms/{key}` alan tanımı → dinamik çizim. Sıra: `submission-tokens` → honeypot + minimum süre + Turnstile → `submissions`. Başarıda referans numarası.
- **Pop-up:** `site.popups`; frontend yola uyan en yüksek öncelikli Modal ve Banner'ı seçer. Karar: yalnız ana sayfada.
- **Menü:** `site` içinde `Header`/`Utility`/`Footer` konumları; ayrı mobil menü yok (Header'dan türetilir).
- **İlan:** `jobs` (sayfalama/filtre; `q` desteği doğrulanacak), `jobs/{slug}`; şirket adı ve logosu görünür, public şirket profili sayfası yok; cinsiyet tercihi yok, askerlik tercihi var; `PublicJobListingsEnabled` kapalıysa gizle.
- **Bayraklar:** `GlobalSearchEnabled` (kullanılmaz), `NewsletterEnabled`, `PublicJobListingsEnabled`, `DonationPageEnabled`, `BotProtectionEnabled`, `AllowSearchEngineIndexing`.

## 4. Aday portalı (`/candidates/...`)
Kayıt (`POST /candidates/register`), liste/özet (`GET /candidates`, `GET /candidates/{candidateCvId}`), kişisel bilgi (`personal-info`), iletişim (`contact-info`), fotoğraf (`photo`), CV içeriği (`content` ve alt kaynaklar: `educations`, `experiences`, `languages`, `certificates`, `references`, `summary`, `cv-file`), CV dışa aktarım (`cv/export`), görüşme talepleri (`meeting-requests`, `.../confirm`), `candidates/interviews`, `employments/candidates/{id}`, `career-development/candidates/{id}`, `bulk-notifications` (yönetim).
Kural: `candidateCvId` oturumdaki adayın kendisininki olarak backend tarafından doğrulanır; frontend başka id deneyerek gezmez. Kişisel/özel nitelikli veri alanları SECURITY.md §11-12'ye göre sunulur.

## 5. İşveren portalı (`/employer/...`)
`register`, `my-company`, `companies/{id}/logo`, `companies/me/logo-visibility`, `jobs` (liste/oluştur/düzenle), `jobs/{id}/submit`, `personnel-needs` (+ `submit`), `interviews`. Genel ilan listesi `GET /jobs`.

## 6. Danışman ve eşleştirme (`/career-advisor/...`)
`candidate-suggestions` (+ `accept/reject`), `my-suggestions`, `candidates/{id}/{advisor-recommendations|career-goals|development-plans|notes|skill-gaps|training-recommendations}`, `development-plans/{id}/{cancel|complete}`, `employments` (+ `end`, `notes`), `general-pool`, `personnel-needs/{id}/{pool|suggestions}`, `personnel-needs/own-pool`, `interviews/to-organize`, `interviews/{id}/{schedule|cancel|result}`, `meeting-requests/{id}/{propose-time|reject}`, `jobs/pending-review`, `jobs/{id}/{approve|reject|request-revision}`.

## 7. Yönetim (`/admin/...`, `/auth/admin/...`)
- **Operasyon:** `admin/candidates/{id}/reassign`, `admin/career-advisors` (+ `deactivate`), `admin/companies/{id}` (+ `approve|reject|deactivate|reassign`), `admin/jobs/{id}/{suspend|reinstate}`, `admin/jobs/backfill-slugs`.
- **Website CMS (`/admin/website/...`):** `content-types` (+ kategoriler, çeviriler), `contents` (+ yayın/zamanlama/arşiv/kopyalama/önizleme bağlantısı/revizyon/çeviri/galeri/video/ek/ilişkili/form/üst içerik/etkinlik ve kayıtları), `media` (+ `folders`), `menus` (`{location}`), `sliders` (+ `slides`), `popups`, `impact-metrics` (rakamlar şeridi), `partners`, `languages`, `settings/*` (genel, tema, iletişim, çerez, özellikler, bakım, bot koruması, kimlik, banka, etkinlik, bülten, gönderim), `layouts/home` (taslak/yayın/önizleme) ve `layouts/content/{id}`, `block-types`, `forms` (+ `fields`), `form-submissions` (liste, atama, durum, not, dosya, arşiv), `legal-documents` (sürümler), `newsletter-subscribers` (+ dışa aktarım), `redirects`, `not-found-paths` (+ yönlendirmeye çevir), `tags` (+ birleştir), `third-party-scripts`, `cookie-consents/summary`, `personal-data-access-log`, `search/sources` (+ yeniden indeksle).
- **Destek:** `support/tickets` (liste, mesaj, kapat, öncelik, devret).
- **Referans verisi:** `reference-data`, `.../districts`, `.../tax-offices`.

## 8. Backend'de olmayan veya doğrulanacak
`FRONTEND-ACIK-MADDELER.md` §B. Bir ekranın ucu OpenAPI'de yoksa o ekran yazılmaz; kayıt altına alınır.

## 9. İstemci katmanı kuralları (`packages/api-client`)
- Tek `createApiClient({ baseUrl, getAccessToken, onUnauthorized })`. Sunucu (Next) ve tarayıcı varyantı; sunucu varyantı `cache` ve `revalidate` seçeneklerini yalnız anonim uçlarda alır.
- Her uç için tipli sarmalayıcı (`getPublicHome()`, `listJobs(params)`...) özellik klasöründe; ham `fetch` sayfalarda yasak.
- `ApiError { status, title, detail?, errors?, correlationId }`. UI yalnız bunu görür.
- İptal (`AbortSignal`) desteklenir; sayfa değişiminde bekleyen istekler iptal.
- Test: sözleşme testleri OpenAPI'ye karşı (üretilen tipler derlenir), 401/refresh/429 senaryoları.
