# Frontend Güvenlik Kuralları

Bu belge `docs/SECURITY.md` ile uyumludur ve onu **genişletmez, frontend'e uygular**. Çelişkide SECURITY.md kazanır. İlgili bölümler: §5 Token, §14-16 API/Doğrulama/Enjeksiyon, §16.1 HTML Sanitizer, §21 Loglama, §23 Rate limiting (§23.5 JSON-LD), §24 CORS, §26 CSRF, §32 Bağımlılık, §33 Güvenlik başlıkları.

## 1. Temel ilke
Frontend bir güvenlik sınırı değildir. Yetki, doğrulama, kaynak sahipliği ve KVKK kararları backend'dedir. Frontend (a) güvenli olmayan şeyi **sızdırmaz**, (b) kullanıcıya yetkisiz şeyi göstermemeye çalışır (UX), (c) saldırı yüzeyi açmaz.

## 2. Oturum ve token
- Kalıcı model ertelendi (K-07). **Geçici model:** access ve refresh token yalnız JS belleğinde; `localStorage`, `sessionStorage`, IndexedDB, JS ile yazılan çerez **yasak**. Sayfa yenilenince yeniden giriş.
- Tüm erişim `packages/auth` `SessionStore` arayüzünden. Başka yerde token okunmaz/yazılmaz.
- Token'lar URL'ye, log'a, hata mesajına, analitiğe, `console`'a, hata izleme aracına girmez.
- `Authorization: Bearer` yalnız `packages/api-client` içinde eklenir.
- 401 → tek seferlik refresh denemesi (eşzamanlı isteklerde tek refresh, kuyruklu); olmazsa bellek temizlenir ve girişe yönlendirilir. Refresh token yeniden kullanım tespiti backend'dedir; frontend aynı refresh token'ı iki kez göndermemeli (kilit).
- Logout: `POST /api/v1/auth/logout` çağrılır, sonra bellek temizlenir. Yalnız istemcide silmek yetmez (SECURITY.md §5).
- Çerez tabanlı modele geçilirse (HttpOnly refresh) CSRF değerlendirmesi yeniden yapılır (SECURITY.md §26); bu belge güncellenir.

## 3. XSS
- Metin her zaman React'in kaçışlamasıyla basılır.
- `dangerouslySetInnerHTML` yalnız `packages/ui/SafeHtml` içinde, yalnız backend'in sanitize ettiği alanlar için (zengin metin). Başka her kullanım derleme hatasıdır (ESLint `no-restricted-syntax`).
- **JSON-LD:** `JSON.stringify` sonrası `<` → `<` (ayrıca `>` `&` ` ` ` `). Tek yardımcı `serializeJsonLd`. SECURITY.md §23.5.
- `href`/`src` değerleri kullanıcı/içerik kaynaklıysa şema doğrulaması (`https`, `http`, `mailto`, `tel`); `javascript:` ve `data:` reddedilir. Tek yardımcı `safeUrl`.
- Dış bağlantılar `rel="noopener noreferrer"`; `target="_blank"` yalnız gerektiğinde.
- `eval`, `new Function`, `document.write`, `innerHTML` yasak.
- Üçüncü taraf script yalnız backend'in tipli `ThirdPartyScript` sağlayıcılarından ve onay sonrası; ham script ekleme yok.

## 4. CSP ve başlıklar
`web` (Next) ve paneller aşağıdaki başlıkları üretir (IIS yapılandırması da aynısını sağlar):
- `Content-Security-Policy`: `default-src 'self'`; `script-src 'self'` + (Next için nonce/hash; Turnstile `https://challenges.cloudflare.com`; GA4 yalnız onaydan sonra eklenecekse bu domainler dinamik izinli değildir, ayrı karar); `frame-src https://challenges.cloudflare.com https://www.youtube-nocookie.com`; `img-src 'self' data: https:` yerine mümkün olan en dar liste (backend medya kaynağı, `i.ytimg.com` kararı SECURITY.md §23.3); `connect-src 'self'`; `frame-ancestors 'none'`; `base-uri 'self'`; `form-action 'self'`; `object-src 'none'`.
- `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy` (kamera/mikrofon/konum kapalı), `Strict-Transport-Security` (üretimde), `Cross-Origin-Opener-Policy: same-origin`.
- Yönetim ve portallar için `X-Robots-Tag: noindex`.
- Üretimde source map yayınlanmaz.

## 5. Veri sızıntısı
- URL'de kişisel veri, token, e-posta yok (sorgu dizesi dahil). Kimlik olarak GUID/slug kullanılır.
- Yetkili yanıtlar `Cache-Control: no-store` semantiğiyle; Next sunucu `fetch` önbelleği **yalnız anonim public uçlarda**. Yetkili isteğe `cache: 'no-store'` zorunlu.
- Panel verisi SSR edilmez (paneller zaten SPA). `web` oturum bilgisi okumaz.
- Hata ekranları ham sunucu mesajı, stack, SQL, kimlik göstermez; `ProblemDetails.title` güvenli ise ve `traceId`/`X-Correlation-ID` "destek kodu" olarak gösterilir.
- Analitik (GA4): yalnız çerez onayı sonrası, kişisel veri/URL parametresi/e-posta göndermez, `/portal` ve `/admin` yollarında hiç yüklenmez.
- Kişisel veri gösteren ekranlarda ekran görüntüsü/yazdırma/dışa aktarma backend yetkisine bağlıdır; frontend maskelemeyi **ek** katman olarak yapar, tek koruma yapmaz. Özel nitelikli veri (SECURITY.md §12.1) listelerde varsayılan gizli.

## 6. Formlar ve yükleme
- Backend ProblemDetails alan hatları gösterilir; istemci doğrulaması yalnız kolaylık.
- Public formlar: gönderim jetonu → honeypot (gizli alan, doldurulursa gönderme) → minimum doldurma süresi → Cloudflare Turnstile belirteci → gönderim. Bot koruması sağlayıcısı bayrağı `GET /public/site`'ten.
- Dosya yükleme: istemci türü/boyutu denetler (UX); gerçek doğrulama (imza, tür, boyut) backend'dedir. Dosya adı ekrana basılırken kaçışlanır. İndirme yalnız yetkili uçtan; imzalı/kısa ömürlü bağlantılar loglanmaz.
- Çifte gönderim: gönder düğmesi pending iken kilitlenir; mutasyonlarda backend gerektiriyorsa `Idempotency-Key`.
- Parola alanları: `autocomplete` doğru (`current-password`, `new-password`), parola göster/gizle erişilebilir, parola politikası backend'den.

## 7. Yetkilendirme (UX katmanı)
- `ProtectedRoute` ve menü gizleme yalnız UX'tir; yetkisiz kullanıcı URL'yi yazsa da backend 403 verir ve ekran "yetkisiz" gösterir.
- Rol/yetki kontrolleri tek yerde (`packages/auth`: `useCan()`); bileşenlerde `role === 'Admin'` gibi dağınık karşılaştırma yasak.
- Kaynak sahipliği (kendi firması/adayı) backend tarafından; frontend kimlik parametresi vermez (`/employer/my-company`, `/candidates/{id}` gibi uçlarda id'yi oturumdan değil, backend'in verdiği listeden alır).

## 8. Üçüncü taraf ve bağımlılık
- Onaylı bağımlılık listesi dışına çıkılmaz (`FRONTEND-MIMARI.md` §7). Lisans MIT/Apache/BSD/ISC.
- `pnpm-lock.yaml` commit; `pnpm audit` Faz sonlarında çalıştırılır ve sonuç raporlanır; `postinstall` betikleri yalnız izin listesiyle (`onlyBuiltDependencies`).
- CDN'den script/stil yüklenmez; yazı tipleri self-host. Harici medya yalnız `youtube-nocookie` (tıklayınca yükle) ve Turnstile.
- Gizli değerler pakete girmez; `NEXT_PUBLIC_*`/`VITE_*` yalnız açık bilgi (Turnstile **site** anahtarı, base URL). Gizli anahtar/parola/bağlantı dizesi frontend deposunda bulunmaz.

## 9. Çerezler ve izleme
Çerez bandı: zorunlu / işlevsel / analitik kategorileri; analitik varsayılan kapalı (opt-in). Tercih tarayıcıda saklanır ve kimliksiz olarak `POST /public/cookie-consents`'e kaydedilir. "Çerez tercihleri" bağlantısı her sayfada. KVKK aydınlatmasında Turnstile ve GA4'ün yurt dışı aktarımı belirtilir (SECURITY.md §12.2).

## 10. Test ve denetim
- Güvenlik odaklı testler: `SafeHtml` (script/olay işleyici temizlenmiş içerikle bile çıplak `innerHTML` yok), `serializeJsonLd` (kaçış), `safeUrl`, token'ın hiçbir depoda bulunmadığı (testte `localStorage/sessionStorage` boş), 401 refresh kilidi, yetkisiz rota davranışı, CSP başlıkları (e2e).
- ESLint kuralları: `no-restricted-syntax` ile `dangerouslySetInnerHTML`, `innerHTML`, `eval`, `localStorage`/`sessionStorage` (yalnız `theme` yardımcısı hariç, tek dosyada izinli).
- Her faz sonunda: `pnpm audit`, axe taraması, başlık kontrolü, sızıntı gözden geçirmesi (URL/log/önbellek).
