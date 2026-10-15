# Frontend Tasarım Sistemi

Tek kaynak: `frontend/packages/design-tokens/tokens.css`. Bu belge o dosyanın tasarım gerekçesidir. Hiçbir uygulama kendi renk/radius/gölge değerini yazmaz; yalnız değişken kullanır.

## 1. İlke
- Yön: **"Modern güven"**. Koyu lacivert/mürekkep zemin, tek canlı vurgu, editoryal başlıklar, bol boşluk, **yuvarlak hatlar**.
- Demonun yapısı (header, menü, layout, ızgara, bileşenler) korunur; demonun renkleri ve keskin kenarları değişir.
- "AI sitesi" görünümünden kaçın: mor-mavi gradyan yok, emoji yok, her yerde aynı kart kalıbı yok, sahte rakam yok.
- Public site vitrin gibi (geniş, editoryal, görsel ağırlıklı); paneller sakin ve yoğun bilgi için (aynı token'lar, daha sıkı boşluk).

## 2. Renk

### Ham palet (bizim paletimiz)
| Ad | Hex | Rol |
|---|---|---|
| Navy | #071f32 | koyu tema zemini, açık temada ana metin, header |
| Ink | #041725 | koyu tema derin zemin, utility bar |
| Ember | #c44325 | ana vurgu (buton, CTA) |
| Ember-strong | #92301b | vurgu hover/aktif, açık zeminde vurgu metni |
| Ember-bright | #d94a2b | yalnız büyük yazı ve süsleme |
| Mist | #e6efed | açık tema ikincil zemin |
| White | #ffffff | açık tema zemini, koyu zeminde metin |
| Moss | #102a2e | koyu tema yüzey (kart) |
| Teal | #0a686b | ikon, rozet, bağlantı (açık tema), odak halkası (açık tema) |

### Doğrulanmış türevler
| Token | Hex | Neden |
|---|---|---|
| teal-strong | #084f52 | teal hover, açık zeminde rozet metni |
| teal-on-dark | #7ed3c8 | koyu zeminde ikon/bağlantı (teal koyuda okunmaz: 2.56) |
| ember-on-dark | #f0735a | koyu zeminde vurgu **metni/bağlantısı** (ember koyuda 3.34, yetmez) |
| surface-raised (koyu) | #123140 | koyu temada yükseltilmiş yüzey |
| surface-alt (açık) | #f2f7f6 | açık temada ikincil yüzey |
| text-secondary (açık) | #425a6b | ikincil metin |
| text-secondary (koyu) | #9fb3bd | ikincil metin |
| border (açık / koyu) | #d7e1df / rgba(255,255,255,.12) | ayırıcı (dekoratif) |
| control-border (açık) | #6c8987 | form kontrolü sınırı (≥3:1 şart) |
| control-border (koyu) | #6a8493 | form kontrolü sınırı (≥3:1 şart) |
| success / warning / danger / info (açık) | #176a42 / #8a4f05 / #b42318 / #125d78 | durum renkleri (demodan) |
| success / warning / danger / info (koyu) | #5ccf98 / #f0b15a / #ff8a80 / #6cc4e0 | koyu temada durum |

### Kontrast tablosu (hesaplandı; `scripts/check-contrast` bunu CI'da tekrar doğrular)
| Çift | Oran | Kullanım |
|---|---|---|
| White / Ember #c44325 | 5.03 | buton metni ✔ |
| White / Ember-strong #92301b | 7.89 | hover ✔ |
| White / Ember-bright #d94a2b | 4.23 | **küçük metin yasak** (büyük metin ≥24 px/19 px kalın tamam) |
| Navy / White | 16.79 | gövde metni ✔ |
| Navy / Mist | 14.33 | ✔ |
| Text-secondary #425a6b / White | 7.23 | ✔ (Mist üzerinde 6.17 ✔) |
| Teal / White | 6.55 | bağlantı, odak ✔ (Mist üzerinde 5.60 ✔) |
| White / Teal | 6.55 | teal dolgu üstü metin ✔ |
| White / Navy | 16.79 | koyu tema metni ✔ |
| White / Moss | 15.08 | ✔ |
| #9fb3bd / Navy | 7.72 | koyu ikincil metin ✔ (Moss üzerinde 6.93, #123140 üzerinde 6.27 ✔) |
| Ember #c44325 / Navy | **3.34** | koyu zeminde **yalnız dolgu/büyük süs**; metin için ember-on-dark |
| Ember #c44325 / Ink | **3.62** | aynı kural |
| Ember-bright #d94a2b / Navy | **3.97** | yalnız büyük yazı |
| Teal #0a686b / Navy | **2.56** | koyu zeminde **kullanma**; teal-on-dark |
| teal-on-dark #7ed3c8 / Navy | 9.62 | ✔ (Moss üzerinde 8.64) |
| ember-on-dark #f0735a / Navy | 5.84 | ✔ (Ink üzerinde 6.33, Moss üzerinde 5.25) |
| control-border #6c8987 / White | 3.77 | form sınırı ≥3:1 ✔ |
| control-border koyu #6a8493 / Navy | 4.26 | ✔ |
| Durum (açık) / White | 6.56-7.34 | ✔ |
| Durum (koyu) / Navy | 7.35-8.91 | ✔ |

Kural: koyu zeminde vurgu rengi **metin** olarak kullanılacaksa `ember-on-dark`/`teal-on-dark`, **dolgu** olarak kullanılacaksa ember/teal. Dolgu üstü metin her zaman beyaz.

## 3. Tipografi
- **Başlık: Poppins** (600-700), **metin: Manrope** (400-800). Self-host (`@fontsource/*`), `latin` ve `latin-ext` alt kümeleri. Türkçe karakterleri (İ ı Ş ş Ğ ğ Ü ü Ö ö Ç ç) görsel testte doğrula. `font-display: swap`, kritik ağırlıklar `preload`.
- Ölçek (`clamp` ile akışkan): display-1 64/1.05, display-2 44/1.1, h1 36/1.18, h2 28/1.22, h3 22/1.3, body-lg 18/1.6, body 16/1.6, small 14/1.5, caption 12/1.4 (semibold, büyük harf aralıklı eyebrow için 12 px, .14em).
- Panelde gövde 14-15 px, tablo 14 px.
- Yazı tipi yığını: `--font-body: Manrope, system-ui, ...; --font-display: Poppins, Manrope, system-ui, ...`. Demodaki `--font-serif` kaldırılır.

## 4. Şekil ve yükseklik
- Radius: `--radius-sm 10px`, `md 16px`, `lg 24px`, `xl 32px`, `pill 999px`. Demonun .5/.875/1.375/2 rem değerleri bunlarla değişir.
- Kullanım: kart `lg`, input/select `md`, rozet `pill`, modal `xl`, görsel çerçevesi `lg`, tablo konteyneri `lg`, tablo hücresi köşesiz.
- **Public** birincil buton `pill`; **panel** buton `md` (yoğun arayüzde daha derli toplu). Aynı token, farklı varsayılan.
- Gölge: `sm 0 1px 2px rgba(7,23,37,.08)`, `md 0 8px 24px rgba(7,23,37,.12)`, `lg 0 24px 56px rgba(7,23,37,.22)`. Koyu temada gölge yerine 1px kenarlık + yüzey tonu farkı.
- Boşluk: demonun `--space-*` ölçeği korunur. Bölüm aralığı `clamp(4.5rem, 7vw, 7rem)`.
- Konteyner: 82 rem (demo), okuma genişliği 47 rem.

## 5. Tema
`.theme-light` / `.theme-dark` sınıfları `<html>` üzerinde. Varsayılan `prefers-color-scheme`; kullanıcı anahtarı tercihi `localStorage` içinde `theme` anahtarıyla (**yalnız tema**; token/oturum asla). İlk boyamada yanıp sönmeyi önlemek için `web` ve paneller `<head>` içinde küçük satır içi betikle sınıfı erkenden atar (CSP için nonce/hash ile).

## 6. Bileşen envanteri (`packages/ui`)
Demodan taşınanlar: `Button`/`ButtonLink`, `FormField`, `AccessibleDialog`, `StatusBadge`, `PageHeader`, `PageState`, `ProgressBar`. Yeni yazılacaklar (panel için gerekli): `DataTable` (sıralama, sayfalama, seçim, boş/yükleniyor/hata), `Pagination`, `FilterBar`, `Tabs`, `Drawer`, `Toast`, `ConfirmDialog`, `EmptyState`, `Stepper`, `FileDropzone` (yalnız UX; doğrulama backend'de), `DescriptionList`, `Card`, `Badge`, `Tooltip`, `SafeHtml`.
Public (`apps/web` içinde): `HeroSlider`, `StatsBand`, `JobCard`, `EventCard`, `NewsCard`, `VideoPoster`, `SuccessStoryCard`, `NewsletterForm`, `CookieBanner`, menü/footer.

Her bileşen: açık+koyu tema, tüm durumlar (hover, focus-visible, disabled, error), klavye, `aria-*`, test.

## 7. Hareket
Ölçülü: giriş animasyonları ≤ 500 ms, `prefers-reduced-motion` altında kapalı. Slider otomatik oynatmada duraklat düğmesi zorunlu.

## 8. Görseller
Stok fotoğraf, doğal ve sıcak; içeriğe bağlı oran etiketleri (`hero 16:9`, `kart 4:3`, `kapak 3:2`). Backend'in sabit boyut varyantları `srcset`. Alt metin zorunlu (dekoratifse boş `alt=""`). Yüz içeren görsellerde izin ve lisans takibi `FRONTEND-ACIK-MADDELER.md`'dedir.
