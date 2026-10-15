import TiptapImage from '@tiptap/extension-image';

/**
 * `@tiptap/extension-image`'ın `group: 'block'` varsayılanı kaldırılır: `image` düğümü yalnız
 * `FigureNode.ts`'in `content: 'image figcaption?'` ifadesinde ADIYLA referans edildiği için var
 * olabilir, üst düzey belge içeriğine (`block+`) tek başına eklenemez - editör UI'ı zaten görseli
 * yalnız `insertFigureImage` komutuyla (RichTextEditor.tsx) figure içinde ekler, ama şema seviyesinde
 * de "çıplak img" olasılığını kapatır (yapıştırma gibi başka bir yoldan sızmasın).
 *
 * `width`/`height` backend whitelist'inde (HtmlSanitizerContentSanitizer.cs) izinli öznitelikler
 * olduğu için eklenir; temel uzantı bunları tanımlamaz. `allowBase64: false` (varsayılan) - data:
 * URI olarak resim yapıştırma/ekleme hiçbir zaman desteklenmez.
 */
export const Image = TiptapImage.extend({
  group: '',
  addAttributes() {
    return {
      ...this.parent?.(),
      width: { default: null },
      height: { default: null },
    };
  },
});
