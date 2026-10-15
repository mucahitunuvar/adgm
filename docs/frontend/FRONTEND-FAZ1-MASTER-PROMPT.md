# Faz 1 Master Prompt: Kimlik + Yönetim Kabuğu

> Claude Code için görev. Önce `docs/frontend/README.md` okuma sırasını ve `FRONTEND-AGENTS.md`'yi yeniden uygula. Bu faz bittiğinde **dur** ve rapor ver. Commit/push yapma.

## Önkoşullar (kontrol et, sağlanmıyorsa dur)
1. **Identity OpenAPI şemaları:** `docs/frontend/FRONTEND-OPENAPI-PRODUCES-PROMPT.md` Görev 1 tamamlanmış olmalı. Kontrol: `pnpm api:gen` sonrası `schema.d.ts` içinde `login`, `refresh-token`, `me`, `auth/admin/users`, `auth/admin/audit-log` yanıtları `unknown` değil, tipli. Değilse **dur** ve bana söyle; elle tip uydurma (Faz 0'daki geçici elle yazılmış `Session`/`SessionUser` tipleri bu fazın başında OpenAPI tiplerine **geçirilir**).
2. **Geliştirme admin'i (ACIK-MADDELER A9):** Geliştirmede Admin kullanıcısının nasıl oluştuğunu bul (Identity seed'i, `docs/DEVELOPMENT.md`, ortam ayarı). Yoksa **dur** ve seçenekleri sun (seed, ortam değişkeniyle bootstrap, kayıt + kontrollü rol yükseltme). **Veritabanını elle değiştirme**, migration geçmişi tablosuna dokunma; böyle bir ihtiyaç doğarsa dur ve bana sor.
3. Backend ayakta (`http://localhost:5016`). Ayakta değilse nasıl çalıştıracağını söyle ve dur. Not: backend testleri koşulacaksa dev API kapalı olmalı (LocalDB paylaşımı flaky yapıyor); Faz 1'de backend testi gerekmez.
4. Personel (staff) hesabı oluşturma ucu backend'de yok (ACIK-MADDELER #19): kullanıcı yönetimi ekranında "yeni kullanıcı" düğmesi yapma.

## Kapsam
`apps/admin` içinde giriş, çıkış, parola sıfırlama, e-posta doğrulama; korumalı kabuk; `GET /auth/me` ile menü; kullanıcı yönetimi; denetim kaydı. Rol: `Admin` tam; `CareerAdvisor` giriş yapabilir ama Faz 1'de yalnız "Panel" ve hesap sayfasını görür (K-08). `Candidate`/`Employer` rolü admin uygulamasına girerse backend'in verdiği yanıt neyse onu göster, "bu panele yetkiniz yok" ekranı + çıkış.

## Görevler

### Görev 1: Tipleri OpenAPI'ye geçir
`packages/api-client` ve `packages/auth`: elle yazılmış `Session`, `SessionUser`, login/me tiplerini sil; üretilen tiplerden türet. `roles.ts` rol adlarını OpenAPI'den/`UserRole` ile uyumlu sabitler. Testler güncellenir.

### Görev 2: api-client auth uçları
Önce `pnpm api:gen` çalıştır (backend Görev 1 sonrası şema); değişen `openapi.json` ve `schema.d.ts` fark özetini rapora yaz. **Hata yanıtları:** 400/404/409 ProblemDetails; 401/403/429 **boş gövdeli** gelir. `ApiError` gövdesiz yanıtı çökmeden ele alır (durum koduna göre güvenli varsayılan başlık), `Retry-After` 429'da okunur. Birim test ekle.

Tipli sarmalayıcılar: `login`, `refreshToken`, `logout`, `me`, `forgotPassword`, `resetPassword`, `verifyEmail` (POST ve GET varyantı; hangisi kullanılacak OpenAPI'den), `resendVerificationEmail`, `changePassword`, ve `auth/admin`: `listUsers`, `getUser`, `changeUserRole`, `deactivateUser`, `reactivateUser`, `unlockUser`, `listAuditLog`. Özellik klasöründe, sayfalarda ham `fetch` yok.

### Görev 3: `packages/auth` olgunlaştırma
- Login → bellekte token; `emailConfirmed=false` ise doğrulama uyarısı ve "yeniden gönder" akışı (backend izin veriyorsa uygulama içinde kullanım sınırlı olabilir; davranışı backend yanıtından öğren ve raporla).
- Refresh: access token süresi dolmadan önce zamanlı yenileme **ya da** 401'de tek seferlik yenileme (mevcut mekanizmayı koru); eşzamanlı isteklerde tek refresh; başarısızlıkta oturum temizlenir ve `/admin/giris`'e yönlendirilir.
- `useCan()`: rol tabanlı (K-08). Sayfa yenilenince oturum biter (K-07); bunu kullanıcıya sürpriz yaşatmayacak şekilde giriş sayfasında kısa bir bilgi yerine yönlendirme sonrası "oturum süresi doldu" mesajı ile ele al.
- Token'lar hiçbir depoya yazılmaz (test mevcut; yeni akışları da kapsa).

### Görev 4: `packages/ui` eklemeleri
`DataTable` (sıralama, boş/yükleniyor/hata durumu, satır eylemleri, klavye ile erişilebilir), `Pagination`, `FilterBar`, `ConfirmDialog`, `Toast`, `EmptyState` (varsa), `Tabs`. Açık/koyu tema, erişilebilirlik, test.

### Görev 5: `apps/admin` giriş akışı (herkese açık sayfalar)
Rotalar (`base: /admin/`): `/giris`, `/sifremi-unuttum`, `/sifre-sifirla`, `/eposta-dogrulama`, `/yetkisiz`, `*` (404).
- Giriş: e-posta + parola, `autocomplete` doğru, hata mesajı backend'e uygun ama **hesap var/yok bilgisini ayırt ettirmeyen** genel mesaj; 423/429 durumları (kilit, rate limit, `Retry-After`) kullanıcı diline çevrilir.
- Parola sıfırlama: "e-posta kayıtlıysa gönderildi" sabit mesaj (anti-enumeration). Sıfırlama ve doğrulama token'ı URL'den **okunup hemen adres çubuğundan silinir** (`history.replaceState`), log'a/analitiğe/hata ekranına yazılmaz, `Referrer-Policy` korur.
- Parola alanı: göster/gizle (erişilebilir), politika ipuçları backend ProblemDetails hatalarından.

### Görev 6: Korumalı kabuk ve menü
`AdminLayout` (demodan taşınan): header (kullanıcı menüsü, tema anahtarı, çıkış), kenar menü (yalnız **var olan** rotalar; sahte link yok), sayfa başlığı/breadcrumb, 403 ve 404 ekranları. Menü rol tabanlı: Admin → Panel, Kullanıcılar, Denetim kaydı, Hesabım; CareerAdvisor → Panel, Hesabım.
`ProtectedRoute` yalnız UX; yetkisiz erişimde `/yetkisiz`.

### Görev 7: Sayfalar
- **Panel (dashboard):** karşılama + oturum bilgisi; backend'den gerçekten alınabilen özet yoksa boş bırak/EmptyState (sahte sayı yok).
- **Hesabım:** `me` bilgisi, parola değiştirme (`changePassword`; başarıda backend oturumları ne yapıyorsa o davranışı izle ve raporla).
- **Kullanıcılar (yalnız Admin):** liste (sayfalama/filtre/arama OpenAPI'nin sunduğu kadar), detay, rol değiştirme, pasife alma, yeniden etkinleştirme, kilit açma. Her yıkıcı/kritik eylem `ConfirmDialog` ile; sonuç `Toast`; 403/404/409/422 anlamlı mesajla. Kendi hesabını pasife alma gibi durumlarda backend ne diyorsa göster, frontend ayrıca engel uydurmaz (yalnız UX uyarısı verebilir).
- **Denetim kaydı (yalnız Admin):** sayfalı liste, filtre, salt okunur. Kişisel veri sızdırmamaya dikkat (e-posta maskeleme yalnız ek katman).

### Görev 8: Test ve güvenlik güvencesi
- Birim: auth akışları (login, refresh kilidi, logout), `useCan`, ProblemDetails eşleme, URL token temizleme.
- Bileşen: `DataTable`, `ConfirmDialog`, giriş formu hata durumları.
- E2E (gerçek backend + gerçek geliştirme Admin'i): giriş → kullanıcı listesi → rol değiştirme (test kullanıcısı üzerinde, geri alınır) → çıkış; hatalı parola; 401 sonrası yönlendirme; yetkisiz rota; sayfa yenileyince oturumun bittiği; **depolarda token yok**; CSP/başlıklar; axe (açık/koyu, 390/1440).
- `pnpm audit`, kontrast betiği.
- Kabul: `pnpm typecheck && pnpm lint && pnpm test && pnpm build` yeşil, e2e yeşil.

### Görev 9: Rapor
`FRONTEND-AGENTS.md` §11 biçiminde, ek olarak: (a) A9 sonucu (geliştirme admin'i nasıl alındı), (b) OpenAPI'ye geçiş sonrası kalan elle tipler (olmamalı), (c) backend davranış bulguları (emailConfirmed, parola değişince oturumlar, 423/429), (d) yeni bağımlılık (olmamalı), (e) **oturum modeli için öneri güncellemesi**: bellek modeliyle panelde yenileme deneyimi nasıl, A2 kararı için ne öneriyorsun.

## Yapma
Backend kodunu/veritabanını değiştirme; mock/sahte oturum; `localStorage/sessionStorage`'a token; onaylı liste dışı paket; Faz 2 ekranlarına (CMS) başlama.
