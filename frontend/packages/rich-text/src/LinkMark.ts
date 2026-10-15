import { mergeAttributes } from '@tiptap/core';
import TiptapLink from '@tiptap/extension-link';
import { ALLOWED_LINK_SCHEMES, isAllowedLinkHref } from './isAllowedLinkHref';

/**
 * Backend whitelist'i `href`, `title`, `target` özniteliklerine izin verir, `rel`/`class` vermez
 * (HtmlSanitizerContentSanitizer.cs - `rel` zaten yalnız backend'in kendisi, `target="_blank"`
 * olduğunda `noopener noreferrer` ekleyerek üretir). `@tiptap/extension-link`'in varsayılan
 * `rel: 'noopener noreferrer nofollow'` ve `class` öznitelikleri bu yüzden tanımdan tamamen
 * çıkarılır - editör hiçbir zaman whitelist dışı bir öznitelik üretmesin diye.
 *
 * `addAttributes()`'i ezmek TEK BAŞINA yetmez: `@tiptap/extension-link`'in varsayılan `renderHTML`'i
 * her zaman `this.options.HTMLAttributes`'i (varsayılanı `{ target: '_blank', rel: '...', class: null }`)
 * düğüm özniteliklerinin ÜSTÜNE `mergeAttributes` ile birleştirir - `rel` düğüm özniteliği olarak
 * tanımlı olmadığı için bu varsayılan sızdırılırdı (`schema.contract.test.ts` bunu yakaladı;
 * `.configure({ HTMLAttributes: {} })` denemesi yetmedi çünkü Tiptap `configure()`'ı `mergeDeep` ile
 * uygular - boş nesne var olan `rel`/`class`/`target` anahtarlarını SİLMEZ). Bu yüzden `renderHTML`
 * burada da ezilir: yalnız düğümün KENDİ özniteliklerini (`href`/`target`) basar, seçenek düzeyindeki
 * varsayılanları hiç karıştırmaz. Href geçerlilik tekrar-kontrolü (parse/komut zamanında zaten
 * `isAllowedLinkHref`/`protocols` ile yapılıyor) bilerek burada tekrarlanmaz - render zamanı yalnız
 * zaten doğrulanmış bir özniteliği HTML'e yazar.
 *
 * `protocols` + `validate`: ikisi de aynı kuralı uygular (bkz. isAllowedLinkHref.ts - backend'in
 * ProcessAnchor/AllowedSchemes mantığının birebir yansıması). `validate` asıl güvenlik kontrolü;
 * `protocols` Tiptap'in kendi ayrıştırma/otomatik-bağlantı davranışını da aynı listeye hizalar.
 */
export const Link = TiptapLink.extend({
  addAttributes() {
    return {
      href: { default: null },
      target: { default: null },
    };
  },
  renderHTML({ HTMLAttributes }) {
    return ['a', mergeAttributes(HTMLAttributes), 0];
  },
}).configure({
  openOnClick: false,
  autolink: false,
  protocols: [...ALLOWED_LINK_SCHEMES],
  validate: isAllowedLinkHref,
});
