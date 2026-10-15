# OpenAPI Yanıt Şemaları Master Prompt (backend işi)

> Bu dosya depoda `docs/frontend/FRONTEND-OPENAPI-PRODUCES-PROMPT.md` olarak durur. **Backend çalışmasıdır**; kök `AGENTS.md`, `ARCHITECTURE.md`, `SECURITY.md` ve ADR'ler geçerlidir (frontend kuralları değil). Görev görev ilerle; her görevden sonra dur ve raporla. Commit/push yapma.

## Bağlam
Frontend Faz 0'ı bir bulgu verdi: `GET /openapi/v1.json` içinde 425 yanıtın **378'i `content` şeması tanımlamıyor** (yalnız `"200": {"description": "OK"}`). Etkilenen modüller: **Identity (auth)** ve **Website** (hem `/api/v1/public/*` hem `/api/v1/admin/website/*`, sitemap/robots dahil). Candidate, Employer, Job, CareerAdvisor, ReferenceData uçları `.Produces<TResponse>(StatusCodes.Status200OK)` kullandığı için tipli.

Sonuç: `openapi-typescript` bu uçlar için `unknown` üretiyor; frontend tipleri backend kaynak kodundan elle yazmak zorunda kalıyor (örneğin `LoginResponse`, `GetCurrentUserResponse`). Bu hem AGENTS.md §23 ("OpenAPI sözleşme keşfinin kaynağıdır") hem frontend'in "tipler yalnız OpenAPI'den" kuralıyla çelişir ve sessiz sözleşme kaymasına yol açar.

## Amaç
Her HTTP uç noktası, gerçek yanıt tiplerini ve standart hata yanıtlarını OpenAPI'ye bildirsin. **Davranış değişmeyecek**; yalnız endpoint metadata'sı eklenecek.

## Kurallar
- AGENTS.md §5 (dur-önce-planla), §11 (bir tip bir dosya), §20-21 (ince endpoint, açık sözleşme), §23 (OpenAPI), §52 (kısayol yok).
- Endpoint'in iş mantığına, yetkilendirmesine, rate limit politikasına, rotasına, HTTP yöntemine, durum koduna dokunma.
- Yanıt tipi, handler'ın gerçekten döndürdüğü `TResponse` olmalı (`result.ToOkOrProblem()` / benzeri yardımcıların çıktısı). Tipini tahmin etme; yardımcıyı ve handler'ı oku. Domain varlığı sızdırma; zaten Response/DTO kullanılıyor, kullanılmıyorsa dur ve raporla.
- Mevcut örnek desen: `GetCandidateCvEndpoint.cs` (`.Produces<GetCandidateCvResponse>(StatusCodes.Status200OK)`).

## Görev 0: Ölçüm ve plan
1. Çalışan API'den `GET /openapi/v1.json` indir; yanıt başına `content` olup olmadığını sayan küçük bir betik (kalıcı değil, sonuçları rapora yaz): toplam yanıt, şemasız yanıt, modül/etiket kırılımı.
2. Rota + HTTP yöntemi listesinin **anlık görüntüsünü** al (before-snapshot). Bittiğinde aynı liste birebir aynı olmalı.
3. `ToOkOrProblem`, `ToCreatedOrProblem` vb. yardımcıları ve dosya indiren uçları (`.ics`, CSV, CV PDF, logo, fotoğraf, form ek dosyası, sitemap/robots) listele.
4. Planı yaz (en fazla 25 satır) ve uygula.

## Görev 1: Identity (frontend Faz 1'in önkoşulu)
Kapsam: `/api/v1/auth/*` (login, refresh-token, logout, me, register, verify-email (GET/POST), resend-verification-email, forgot-password, reset-password, change-password) ve `/api/v1/auth/admin/*` (users liste/detay, deactivate, reactivate, role, unlock, audit-log).
- Başarı yanıtlarına `.Produces<T>(...)` (201/200/204 doğru kodla; gövdesiz yanıtlarda `.Produces(StatusCodes.Status204NoContent)`).
- Standart hata yanıtları için **tek bir yardımcı**: `BuildingBlocks/Infrastructure/Http` altında bir extension (örn. `ProducesProblemExtensions`, bir tip bir dosya) ve uçlara göre 400/401/403/404/409/422/429 bildirimi (yalnız ucun gerçekten dönebildiği kodlar).
- Kabul: Identity uçlarında şemasız yanıt 0; `LoginResponse` ve `GetCurrentUserResponse` OpenAPI'de görünüyor.

## Görev 2: Website public (frontend Faz 3'ün önkoşulu)
Kapsam: `/api/v1/public/*` (site, home, routes/resolve, contents, contents/{id}, preview, events + registrations + verifications + cancellations + `.ics`, jobs, jobs/{slug}, companies logo, forms, submission-tokens, submissions, newsletter, legal-documents, cookie-consents, videos, partners, search) ve kökteki `/sitemap.xml`, `/robots.txt`, `/sitemap-{n}.xml`.
- Karmaşık/polimorfik yanıtlar (routes/resolve sonucu, blok listesi `blocks` / `data`) için OpenAPI'de ifade edilebilen en doğru tipi kullan; ifade edilemeyen kısım (blok `data` nesnesi) için bunu raporla, **tahmini tip uydurma**. Blok tipleri için `GET /admin/website/block-types` zaten alan tanımı veriyor; public yanıtta `data` serbest şema ise `JsonElement`/açık bir "free-form" olarak belgelenir ve frontend tarafına not düşülür.
- Dosya/metin yanıtları: doğru content type (`text/calendar`, `application/xml`, `text/plain`, `text/csv`, `application/pdf`, görsel).
- Kabul: public uçlarda şemasız JSON yanıt 0; `getPublicSite()` ve `getPublicHome()` tipli.

## Görev 3: Website admin (frontend Faz 2'nin önkoşulu)
Kapsam: `/api/v1/admin/website/*` (yaklaşık 145 yol): content-types, contents (+ alt kaynaklar), media, menus, sliders, popups, impact-metrics, partners, languages, settings/*, layouts/*, block-types, forms, form-submissions, legal-documents, newsletter-subscribers (+ export), redirects, not-found-paths, tags, third-party-scripts, cookie-consents/summary, personal-data-access-log, search/sources.
- Aynı kurallar. Büyük iş: alt kapsamlara böl (örn. içerik; medya+menü+slider; ayarlar; formlar+başvurular; etkinlik kayıtları; diğerleri) ve her dilim sonunda derle + test.
- Dışa aktarım ve dosya indirme uçları doğru content type ile.

## Güncelleme (14 Ekim, Görev 1 sonrası): kapsam ve sıra
Görev 0 ölçümü bu dosyadaki "diğer modüller tipli" varsayımının **yanlış** olduğunu gösterdi: şemasız yanıt oranları CareerAdvisor 7/7, ReferenceData 63/88, Candidate 24/29, Employer 23/31 (Job ve Notification'ın kendi HTTP ucu yok). Bu yüzden iş **fazlara bağlı dilimlere** bölünür; her dilim ilgili frontend fazından hemen önce yapılır:

| Dilim | Kapsam | Frontend önkoşulu |
|---|---|---|
| Görev 1 | Identity (**tamamlandı**, 2817/2817 test) | Faz 1 |
| Görev 3 | Website admin | Faz 2 |
| Görev 2 | Website public + sitemap/robots | Faz 3 |
| Görev 4a | Candidate + ReferenceData | Faz 4 |
| Görev 4b | Employer | Faz 5 |
| Görev 4c | CareerAdvisor + admin operasyon uçları (`/admin/companies|jobs|candidates|career-advisors`) + Support | Faz 6 |
| Görev 5 | Kalıcı test (aşağıda) | Görev 3'ten itibaren, modül bazlı istisna listesiyle |

Kalıcı test baştan **modül/etiket bazlı istisna listesiyle** eklenir (henüz yapılmamış modüller listede); her dilim ilgili modülü listeden çıkarır. Böylece yeni endpoint'ler hemen denetlenir, eski modüller dilim sırası gelince kapanır.
Ek kurallar (Görev 1'den öğrenilenler): 401/403/429 yanıtları middleware'den **gövdesizdir** (ProblemDetails değildir) ve bunlar helper'da ayrı bildirilir; 422 kullanılmıyorsa eklenmez.

## Görev 4 (genel tarif; yukarıdaki dilimlere göre uygulanır): Geri kalan eksikler ve kalıcı güvence
1. Görev 0'daki betikle kalan şemasız yanıtları bul (Support, Notification, ReferenceData dışı vb.), düzelt veya gerekçeli istisna listesine yaz.
2. **Mimari/fonksiyonel test:** uygulamanın `EndpointDataSource`'undaki her `/api/` ucunun en az bir başarı (2xx) yanıt metadata'sı bildirdiğini, JSON gövde döneniyse yanıt tipinin tanımlı olduğunu doğrulayan test (`tests/` altında uygun projede; bir tip bir dosya). Gerekçeli istisna listesi testin içinde açık ve kısa olsun (ör. dosya indirme uçları content type ile ayrıca doğrulanır).
3. İkinci test: rota+yöntem listesi (Görev 0 öncesi anlık görüntü) ile sonrası arasında fark yok (bu bir kerelik doğrulama olarak çalıştırılır; kalıcı test olması gerekmez).
4. `ARCHITECTURE.md` ve `DEVELOPMENT.md`'ye kural ekle: "Yeni endpoint `.Produces<T>()` ve standart hata metadata'sı olmadan eklenemez; test bunu denetler." (Frontend tip üretimi buna bağlıdır.)

## Doğrulama
- `dotnet build` ve ilgili tüm testler (önceki bilinen durum 2817/2817); sonuçları aynen raporla.
- Önce/sonra: şemasız yanıt sayısı (hedef: istisna listesi dışında 0), rota+yöntem listesinde fark yok.
- `GET /openapi/v1.json` ile `pnpm api:gen` (frontend) çalıştırılabilir olmalı; **frontend tarafını sen değiştirme**, yalnız OpenAPI'nin geçerli JSON/şema olduğunu doğrula.

## Rapor (AGENTS.md §56)
Özet / Mimari (etkilenen modüller, eklenen yardımcı) / Doğrulama (komutlar ve gerçek sonuçlar) / Kalan sorunlar (ifade edilemeyen polimorfik alanlar, istisnalar, mevcut ihlaller, varsayımlar).
