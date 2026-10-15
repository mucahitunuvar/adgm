# AGENTS.md (frontend)

> **Kapsam:** `frontend/` altındaki tüm çalışma. Kök `AGENTS.md` (backend) ve `docs/SECURITY.md` frontend için de bağlayıcıdır; bu dosya frontend'e özgü kuralları ekler.
> **Zorunlu:** Kod yazmadan, taşımadan, silmeden önce bu dosyayı ve `docs/frontend/README.md` içindeki okuma sırasını oku.
> **Çekirdek kural:** Mimariyi veya güvenliği hızlı bitirmek için delme. Çatışma varsa dur ve açıkla.

## 1. Önce durup düşün (AGENTS.md §5 karşılığı)
Anlamlı bir özellikten önce şunları yaz: hangi uygulama (`web/candidate/employer/admin`), hangi aktör/rol, hangi kullanım senaryosu, hangi backend uçları (OpenAPI'den), yetki gereksinimi, hassas veri var mı (KVKK), hangi paylaşılan paket etkilenir, oluşturulacak/değiştirilecek dosyalar. Sonra uygula.

## 2. Çatışma kuralı
Bir gereksinim şunlarla çelişirse **kodlamadan dur**: backend sözleşmesi (OpenAPI), `SECURITY.md`, `FRONTEND-KARARLAR.md`, uygulamalar arası sınırlar. Biçim: Çatışma / Neden / Etkilenen kural / Alternatifler / Önerilen çözüm. Sessizce çözüm uydurma.

## 3. Backend kaynak gerçektir
- İş kuralı, yetki, doğrulama ve veri sahipliği backend'dedir. Frontend yalnız sunar ve kullanım kolaylığı sağlar.
- Backend sözleşmesini frontend kolaylığı için değiştirme önerme; eksik varsa `FRONTEND-ACIK-MADDELER.md` listesine yaz.
- İstemci doğrulaması güvenlik değildir. Rota koruması (`ProtectedRoute`) güvenlik değildir; backend her isteği doğrular.
- İstemci tarafından gönderilen kullanıcı/firma/aday kimliği yetki kanıtı değildir; kimlik her zaman oturum token'ından.

## 4. Mock yok
`MockApiClient`, sahte veri, sabit "demo" metin/istatistik üretme. Veri yoksa boş durum göster ve raporla. Geliştirme verisi `frontend/scripts/seed-dev` ile **gerçek admin uçlarından** girilir.

## 5. Tip ve sözleşme
- API tipleri yalnız OpenAPI'den üretilir (`pnpm api:gen`); elle API tipi yazma, `any` kullanma.
- TypeScript `strict`. `// @ts-ignore`, `eslint-disable` gerekçesiz yasak.
- ProblemDetails tek yardımcıyla ele alınır (`packages/api-client`); ham hata metni ekrana basılmaz.
- Para/tarih: tarihler UTC gelir, arayüzde Europe/Istanbul'a çevrilir (tek yardımcı).

## 6. Yapı kuralları
- **Bir bileşen bir dosya.** Dosya adı bileşen adıyla aynı (`StatusBadge.tsx`). `utils.ts`, `helpers.ts`, `common.ts`, `types.ts` gibi torba dosyalar yasak; ilgili özelliğin içinde anlamlı adla.
- Özellik odaklı klasörleme (`features/jobs/...`), yatay `components/` yığını değil. Yalnız gerçekten ortak olanlar `packages/ui` içine.
- Uygulamalar birbirinin kodunu içe aktarmaz; ortak kod yalnız `packages/*` üzerinden.
- `packages/ui` iş mantığı ve API çağrısı içermez; sunumsal ve erişilebilir olur.
- `packages/api-client` UI bilmez. `packages/auth` UI bilmez.
- Sayfalar ince tutulur: veri çekme özellik kancasında, sunum bileşende.

## 7. Güvenlik (özet, ayrıntı `FRONTEND-GUVENLIK.md`)
Token'lar yalnız bellekte (K-07), `localStorage/sessionStorage` yasak. `dangerouslySetInnerHTML` yalnız `SafeHtml` bileşeninde ve yalnız backend'in temizlediği alanlar için. JSON-LD basarken `<` → `<`. Gizli değer (anahtar, parola) pakete girmez; `NEXT_PUBLIC_`/`VITE_` yalnız açık bilgi (Turnstile site anahtarı gibi). Token, parola, kişisel veri loglanmaz. Üretimde source map kapalı.

## 8. Erişilebilirlik ve kalite
Gerçek `button/a/input/label`; div'e `onClick` yasak. 44 px dokunma hedefi, 4.5:1 metin kontrastı (`scripts/check-contrast` ile), görünür odak, klavye ile tam kullanım, `prefers-reduced-motion`. Her yeni sayfa için axe testi.

## 9. Bağımlılık
`FRONTEND-MIMARI.md` §7 onaylı liste. Dışına çıkma. `pnpm` kilit dosyası commit'lenir; `postinstall` betikleri açıkça izin verilenlerle sınırlıdır.

## 10. Bitti tanımı
- [ ] Doğru uygulama/paket
- [ ] Gerçek backend ucu kullanıldı (OpenAPI'de var)
- [ ] Yetki/rol senaryoları ve 401/403/404/409/422/429 durumları ele alındı
- [ ] Yükleniyor/boş/hata durumu var
- [ ] Hassas veri sızdırılmıyor (URL, log, önbellek, analitik)
- [ ] Erişilebilirlik ve kontrast kontrolü yapıldı
- [ ] Test eklendi/güncellendi (bileşen + kritik akış)
- [ ] `pnpm typecheck && pnpm lint && pnpm test && pnpm build` çalıştırıldı, sonuçlar raporda
- [ ] Mimari kısayol yok

## 11. Tamamlama raporu
Özet (ne, neden) / Mimari (uygulama, paket, uçlar, yetki) / Doğrulama (komutlar ve sonuçlar, çalıştırılmayanı "çalıştı" deme) / Kalan sorunlar (açık backend maddeleri, ertelenenler, riskler, varsayımlar). Commit/push yapma; değişiklikleri göster.
