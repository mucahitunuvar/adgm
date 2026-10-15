# Faz 2 Master Prompt: Website CMS Yönetim Ekranları (`apps/admin`)

> Claude Code için görev. Önce `docs/frontend/README.md` okuma sırasını ve `FRONTEND-AGENTS.md`'yi yeniden uygula. Bu faz **üç dilime** bölünmüştür; her dilim sonunda **dur** ve rapor ver. Commit/push yapma. Veritabanını ve backend kodunu elle değiştirme.

## Önkoşullar (kontrol et, sağlanmıyorsa dur)
1. **Website admin OpenAPI şemaları:** `docs/frontend/FRONTEND-OPENAPI-PRODUCES-PROMPT.md` Görev 3 tamamlanmış olmalı. Kontrol: `pnpm api:gen` sonrası `/admin/website/*` yanıtları `unknown` değil, tipli. Eksikse **dur**, listesini ver; elle tip uydurma.
   **Enum modeli (karar, 14 Ekim):** Backend enum'ları kablo üzerinde çoğunlukla **string üye adı** olarak gönderir/alır (`"Published"`); OpenAPI'de bu alanlar düz `string` kalır (bilinçli, kablo biçimi değiştirilmedi). Enum **kod listesinin tek kaynağı** `docs/api/enums.v1.json` manifestidir (`{Modül: {EnumAdı: {ÜyeAdı: sayısalDeğer}}}`; backend `EnumContractTests` ile sapmaya karşı korunuyor). İstisna: enum tipinde olup **sayı** giden alanlar vardır (şimdilik `EventRegistration*Response.Status`); bunlar şemada `integer` + `x-enum-varnames` olarak gelir. Manifest yoksa veya testi düşüyorsa **dur**. Kodları/ adları elle yazma.
2. Backend ayakta, geliştirme Admin'i (`docs/DEVELOPMENT.md` §44.1) çalışıyor.
3. Okuma: `ADR-024-Website-Module-Design.md` (içerik modeli §4, medya §6, menü/bağlantı hedefi §7, blok kataloğu §8, pop-up §9, ayarlar §12), `SECURITY.md` §16.1 (HTML sanitizer) ve §22.1.

## Görev 0: Faz 1'den kalan paylaşılan hata (küçük)
`apps/candidate` ve `apps/employer` `App.tsx` aynı hatayı taşıyor: `AuthProvider`'a backend origin'i veriliyor (CORS). `window.location.origin` kullanacak şekilde düzelt (admin'deki gibi). Her iki uygulamaya, API çağrılarının aynı origin'e gittiğini doğrulayan basit bir smoke test ekle. Bu görevden başka bir şeye dokunma.

## Görev 0b: Enum altyapısı (Dilim 2a-1'den önce, küçük)
Amaç: sistem enum'ları için **üretilmiş kod listesi + dile göre etiket sözlüğü**; elle yazılmış ayna yok.
1. `pnpm api:gen` adımını genişlet: `docs/api/enums.v1.json`'dan `packages/api-client/src/enums.generated.ts` üret (her enum için `as const` nesne + üye-adı birleşim tipi; dosya başında "ÜRETİLDİ, elle düzenleme" uyarısı). Üretilen dosya commit edilir; manifest değişip dosya güncellenmezse CI/test düşer (`api:gen` sonrası `git diff --exit-code` benzeri kontrol).
2. `parseEnum(enumObj, value)` yardımcısı (api-client): bilinen üye adını döndürür; **bilinmeyen** değerde `null` döner ve arayüz "Bilinmeyen (ham değer)" gösterir; çökme yok. Sayı gelen enum alanları için `fromNumeric(enumName, n)` (manifestteki sayısal değerden üye adına).
3. Etiket sözlüğü: `apps/admin/src/i18n/enum-labels.tr.ts`, her enum için `Record<EnumTipi, string>` (tam kapsama zorunlu: manifeste üye eklenince `api:gen` sonrası **derleme hatası** vermeli; `satisfies`/`Record` ile). Yeni i18n kütüphanesi **ekleme**; yapı sonradan `en` eklemeye hazır (`enum-labels.<dil>.ts`, tek `getEnumLabel(enumName, member, lang)` girişi).
4. `enumOptions(enumObj, { only?, exclude? })` → `[{ value: <üye adı>, label: <çeviri> }]`; `Select`, rozet ve filtreler bunu kullanır. **Backend'e her zaman `value` (üye adı) gider, çevrilmiş etiket asla; etiketten koda geri dönüş yapılmaz.**
5. Faz 1'de elle yazılmış `UserRole`, `UserStatus`, `AdminActionType` sabitlerini üretilmişlere geçir, etiketleri sözlüğe taşı. Test: sözlük tam kapsıyor, `parseEnum` bilinmeyeni güvenle ele alıyor, seçenek `value`'ları üye adı.
6. Gerçek çok dilli veri (içerik tipi adı, kategori, menü başlığı, form alanı etiketi, slide metni) bu sistemin **dışındadır**; bunlar CMS çevirileriyle gelir.
Bu görev dışında bir şeye dokunma. Bitince kısaca rapor ver ve Dilim 2a-1'e geç (ayrı durak yok).

## Ortak ilkeler (üç dilim için)
- **Enum kuralı:** her enum alanı için `enums.generated.ts` + `parseEnum` + `getEnumLabel` kullanılır; ekranda ham enum adı gösterilmez; seçenek listeleri `enumOptions()`'tan gelir.
- Yalnız `Admin` rolü CMS'i görür; `CareerAdvisor` bu menüleri görmez (rota koruması UX; gerçek yetki backend).
- Menüde yalnız **gerçekten var olan** sayfalar. Bir ekran yarım ise menüde gösterme.
- Her ekran: yükleniyor / boş / hata / yetkisiz durumları, klavye erişimi, açık+koyu tema, axe temiz.
- **Eşzamanlılık:** backend `rowversion`/ETag benzeri bir mekanizma kullanıyorsa (409) kullanıcıya "başkası değiştirdi, yeniden yükle" akışı göster; sessizce üzerine yazma.
- **Kaydedilmemiş değişiklik** uyarısı (rota değişimi ve sekme kapama).
- **Çok dilli yapı:** diller `GET /admin/website/languages`'ten; arayüz dil sekmeleri destekler ama tek aktif dil varsa sekme gizlenir. Çeviri alanları (`/translations/{languageCode}`) bu sekmeler üzerinden.
- **HTML güvenliği:** yönetim ekranlarında bile zengin metin önizlemesi yalnız `SafeHtml` ile ve backend'in sanitize ettiği çıktıyla. İstemci tarafında kendi sanitizer'ını yazma.
- Tarihler: formda `datetime-local`, gönderirken Europe/Istanbul → UTC çeviri (tek yardımcı), gösterirken tersi. Zamanlama (yayın/kaldırma) alanları açık ve doğrulanmış.
- Sıralama (menü, slide, galeri, video listesi): sürükle-bırak kütüphanesi **ekleme** (onaylı liste dışı); erişilebilir "yukarı/aşağı taşı" düğmeleriyle yap. İhtiyaç netleşirse dur ve öner.
- Yeni paylaşılan bileşenler `packages/ui`'ya, bir bileşen bir dosya: `TranslationTabs`, `FormSection`, `FieldRow`, `DateTimeField`, `TagInput`, `TreeList` (iç içe liste), `ReorderList`, `MediaPicker`'ın sunumsal kısmı (veri bağımlı kısmı özellik klasöründe), `PublishStatusBadge`, `UnsavedChangesGuard`.
- Veri: `@tanstack/react-query`; her kaynak için `queryKey` fabrikası; mutasyon sonrası ilgili anahtarlar geçersiz kılınır. Ham `fetch` yok; her uç `packages/api-client` altında tipli sarmalayıcı (`features`ın değil, ağ katmanının işi).
- Test verisi: e2e testlerinde oluşturulan her kayıt `e2e-` önekiyle adlandırılır ve test sonunda temizlenir (arşivle/sil); geliştirme veritabanı kirlenmemeli.

## DİLİM 2a-1: Altyapı, ayarlar, diller, içerik tipleri
**Kapsam:** CMS menü grubu ve bölüm kabuğu; **Genel ayarlar** (`settings` ve alt uçları: tema, iletişim, çerez, özellikler/bayraklar, bakım, bot koruması, kimlik, banka hesapları, etkinlik, bülten, gönderim); **Diller** (liste, etkinleştir/pasifleştir, varsayılan yap); **İçerik tipleri** (liste, oluştur/düzenle, etkinleştir/pasifleştir, çeviriler, tip bayrakları: `HasListingPage`, `HasDetailPage`, `SupportsEvent`, `SupportsBlockLayout`, `SchemaKind` vb. — ADR-024'e göre); **Kategoriler** (tip altında, iç içe) ve **Etiketler** (liste, düzenle, birleştir).
**Kabul:** ayarlar kaydedilir ve `GET /public/site` yanıtında değişim görünür; bayrak (ör. `NewsletterEnabled`) kapat/aç; içerik tipi oluşturulup etkinleştirilir; kategori ağacı oluşturulur; e2e gerçek backend ile geçer; testler ve `typecheck/lint/test/build` yeşil.
**Dur ve raporla.**

## DİLİM 2a-2: İçerik ve medya
**İlk adım: zengin metin editörü kararı (A11).** Koda başlamadan önce backend `IHtmlContentSanitizer` kurallarını (izinli etiket/öznitelik/URL şemaları) oku; **MIT/Apache** lisanslı en fazla **2 editör adayı** karşılaştır (ör. Tiptap, Lexical gibi); her biri için: lisans, bakım durumu, ürettiği HTML'in backend sanitizer ile uyumu (hangi etiketler çıkarılır/korunur), erişilebilirlik, paket boyutu, bağımlılık sayısı. CKEditor/TinyMCE hariç. **Önerini yaz ve DUR; onayımı bekle.** Onaydan sonra devam et.
**Kapsam (onaydan sonra):**
- **Medya kütüphanesi:** klasörler, yükleme (çoklu, ilerleme, hata), liste/ızgara, detay (alt metin, boyut varyantları, kullanım bilgisi), silme (kullanımda silinemez: backend hatasını açıkla). Yükleme istemcide tür/boyut kontrolü (yalnız UX); gerçek doğrulama backend'de; izinli belge türleri PDF/Word/Excel.
- **MediaPicker** (içerik formlarında medya seçme).
- **İçerik:** tip bazlı liste (filtre: durum, kategori, etiket, dil, tarih, arama; sayfalama; sıralama), oluştur/düzenle (başlık, kısa açıklama, zengin gövde, slug ve tam yol önizleme, SEO alanları `meta title/keywords/description`, `NoIndex`, kapak ve detay görseli, galeri, video listesi (sıralı), ek dosyalar, kategori/etiketler, ilişkili içerik, üst içerik/hiyerarşi, öne çıkan, sıra), çeviri sekmeleri, **yaşam döngüsü** (taslak/yayında/yayından kaldırılmış/arşiv, zamanlanmış yayın/kaldırma, kopyalama, çöp kutusu ve geri alma varsa), **revizyon geçmişi** (liste, karşılaştırma, geri yükleme), **önizleme bağlantısı** üretme, **etkinlik alanları** (`SupportsEvent` tiplerde `/event`: tarih/yer/format/kontenjan/başvuru penceresi/`AutoConfirm`/`WaitlistEnabled`/iptal; kayıt yönetimi Dilim 2b'dedir, burada yok).
**Kabul:** CMS'ten bir içerik oluşturulup yayınlanır; `GET /public/contents` ve `routes/resolve` yanıtında görünür; yayından kaldırılınca kaybolur; revizyon geri yükleme çalışır; eşzamanlı düzenleme 409 senaryosu e2e ile; zengin metin çıktısı `SafeHtml` ile önizlenir; axe temiz.
**Dur ve raporla.**

## DİLİM 2a-3: Sunum (menü, slider, rakamlar, pop-up, ana sayfa düzeni)
- **Menüler** (`Header`/`Utility`/`Footer`, `menus/{location}`): iç içe öğe ağacı, her öğe için **bağlantı hedefi** (içerik, içerik tipi liste sayfası, sistem rotası, dış URL, bağlantısız grup başlığı — `LinkTarget`), açma/kapama, sıralama, çeviri. Öğe hedefi çözümlenemiyorsa backend'in public'te gizlemesini yönetim ekranında bir uyarıyla göster.
- **Slider** ve **slide'lar** (masaüstü görsel zorunlu, mobil görsel opsiyonel, metin, buton/bağlantı hedefi, zamanlama, sıralama).
- **Rakamlar şeridi** (`impact-metrics`: etiket, değer, ikon, sıra, çeviri, etkin/pasif).
- **Pop-up/Banner** (`popups`: tür Modal/Banner, hedefleme, sıklık, öncelik, zamanlama, görsel, metin, buton, çeviri).
- **Ana sayfa düzeni:** `layouts/home` taslak/yayın/önizleme akışı. Blok tipleri `GET /admin/website/block-types`'tan; **blok düzenleme formu bu alan tanımından dinamik üretilir** (genel bir `BlockFieldRenderer`: metin, zengin metin, medya, link hedefi, içerik/tip seçici, sayı, seçim, dizi vb.). Blok ekleme, sıralama, etkin/pasif, taslağı at, yayınla. Bilinmeyen alan tipinde formu bozma, "desteklenmeyen alan" göster ve raporla. İçerik detay sayfaları için `layouts/content/{id}` aynı bileşenle.
- **Zincir doğrulama (kabul):** CMS'te menü + slider + rakamlar + ana sayfa düzeni girilip yayınlanınca `GET /public/site` ve `GET /public/home` yanıtlarında görünür; yayından kaldırılan öğe public'te yoktur. (Bu, Faz 3 public siteyi gerçek veriyle besleyecek durumdur.)
**Dur ve raporla.**

## Her dilimin sonunda
- `pnpm typecheck && pnpm lint && pnpm test && pnpm build`, e2e (gerçek backend), axe (açık/koyu, 390 ve 1440), `pnpm audit`, kontrast betiği. Hepsini çalıştır, sonuçları aynen yaz.
- `e2e/` klasörünü `pnpm lint` kapsamına al (Faz 1'den kalan boşluk).
- Rapor biçimi `FRONTEND-AGENTS.md` §11 + şunlar: (a) OpenAPI'de eksik/yanlış bulduğun her şey (uç, enum, alan; backend'e iletilecek liste), (b) backend davranış bulguları, (c) eklenen bağımlılık (editör kararı hariç olmamalı), (d) bir sonraki dilim için riskler, (e) geliştirme veritabanında bıraktığın veri (olmamalı).

## Yapma
Faz 2b ekranlarına (formlar/başvurular, etkinlik kayıt yönetimi, yasal metinler, bülten, yönlendirmeler/404, üçüncü taraf script, arama kaynakları, çerez özeti, kişisel veri erişim kaydı) başlama. Zengin metin editörünü onaysız seçme. Sürükle-bırak/tarih/form kütüphanesi ekleme. Backend/veritabanını değiştirme. Mock veri.
