import { Node, mergeAttributes } from '@tiptap/core';

/**
 * Backend whitelist'indeki `figure` düğümü (HtmlSanitizerContentSanitizer.cs). Yalnız bir `image`
 * ve isteğe bağlı bir `figcaption` içerebilir - görsel yalnız bu sarmalayıcı içinde var olabilir
 * (bkz. ImageNode.ts: `image`'ın kendi `group`'u boş bırakılmıştır, üst düzeyde tek başına
 * eklenemez), böylece editörün ürettiği her görsel backend'in beklediği `<figure><img>...</figure>`
 * biçimine zaten uyar.
 */
export const Figure = Node.create({
  name: 'figure',
  group: 'block',
  content: 'image figcaption?',
  parseHTML() {
    return [{ tag: 'figure' }];
  },
  renderHTML({ HTMLAttributes }) {
    return ['figure', mergeAttributes(HTMLAttributes), 0];
  },
});
