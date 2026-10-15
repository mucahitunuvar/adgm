import TiptapLink from '@tiptap/extension-link';
import { isAllowedLinkHref } from './isAllowedLinkHref';

/**
 * Backend whitelist'i `href`, `title`, `target` özniteliklerine izin verir, `rel`/`class` vermez
 * (HtmlSanitizerContentSanitizer.cs - `rel` zaten yalnız backend'in kendisi, `target="_blank"`
 * olduğunda `noopener noreferrer` ekleyerek üretir). `@tiptap/extension-link`'in varsayılan
 * `rel: 'noopener noreferrer nofollow'` ve `class` öznitelikleri bu yüzden tanımdan tamamen
 * çıkarılır - editör hiçbir zaman whitelist dışı bir öznitelik üretmesin diye.
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
}).configure({
  openOnClick: false,
  autolink: false,
  protocols: ['http', 'https', 'mailto', 'tel'],
  validate: isAllowedLinkHref,
});
