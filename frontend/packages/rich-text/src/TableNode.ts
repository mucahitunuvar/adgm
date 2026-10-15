import { mergeAttributes } from '@tiptap/core';
import { Table as TiptapTable } from '@tiptap/extension-table';

/**
 * Backend whitelist'indeki `table` düğümü (`table, thead, tbody, tr, th, td` -
 * `HtmlWhitelistPolicy.AllowedTags`, Website). `@tiptap/extension-table`'ın varsayılan `renderHTML`'i
 * kolon genişliği izlemek için `resizable: false` olsa da her zaman bir `<colgroup>`/`<col>` çifti
 * VE hesaplanmış bir `style="width: ...` özniteliği üretir - üçü de whitelist dışı
 * (`schema.contract.test.ts` bunu yakaladı). `renderHTML` burada ezilir: backend'in beklediği sade
 * `<table><tbody>...</tbody></table>` biçimi üretilir; sütun genişliği izleme yalnız düzenleme
 * görünümünde (`TableView` nodeView, `getHTML()` serileştirmesini etkilemez) kalır.
 */
export const Table = TiptapTable.extend({
  renderHTML({ HTMLAttributes }) {
    return ['table', mergeAttributes(HTMLAttributes), ['tbody', 0]];
  },
});
