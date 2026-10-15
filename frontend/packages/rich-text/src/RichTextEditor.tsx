import { EditorContent, useEditor } from '@tiptap/react';
import { useEffect } from 'react';
import { richTextExtensions } from './schema';
import { RichTextToolbar } from './RichTextToolbar';
import { transformPastedHtml } from './transformPastedHtml';

/**
 * Backend whitelist'ine (ADR-024 §16) kısıtlanmış zengin metin düzenleyici (A11). Güvenlik sınırı
 * BACKEND'dir - bu bileşenin ürettiği HTML hiçbir zaman güvenli SAYILMAZ; kaydedildikten sonraki
 * önizleme her zaman backend'in döndürdüğü, zaten sanitize edilmiş HTML'i `@gm/ui`'nin `SafeHtml`
 * bileşeniyle gösterir - bu bileşenin kendi `editor.getHTML()` çıktısı asla
 * `dangerouslySetInnerHTML`'e girmez (tüketici, `onChange` ile aldığı HTML'i yalnız `value` olarak
 * saklar/gönderir, kendi önizlemesinde basmaz).
 */
export function RichTextEditor({
  value,
  onChange,
  onRequestImage,
  ariaLabel,
  disabled = false,
}: {
  value: string;
  onChange: (html: string) => void;
  onRequestImage: () => Promise<{ src: string; alt: string } | null>;
  ariaLabel: string;
  disabled?: boolean;
}) {
  const editor = useEditor({
    extensions: richTextExtensions,
    content: value,
    editable: !disabled,
    editorProps: {
      attributes: { 'aria-label': ariaLabel, role: 'textbox', 'aria-multiline': 'true' },
      transformPastedHTML: transformPastedHtml,
    },
    onUpdate: ({ editor: current }) => onChange(current.getHTML()),
  });

  useEffect(() => {
    editor?.setEditable(!disabled);
  }, [disabled, editor]);

  useEffect(() => {
    // Harici bir `value` değişikliğini (ör. form sıfırlama) yansıtmak için - yazarken her tuş
    // vuruşunda `value` prop'u da değiştiği için `editor.getHTML() === value` kontrolü döngüyü önler.
    if (!editor || editor.getHTML() === value) return;
    editor.commands.setContent(value, { emitUpdate: false });
  }, [value, editor]);

  if (!editor) return null;

  return (
    <div className="rich-text-editor">
      <RichTextToolbar editor={editor} onRequestImage={onRequestImage} />
      <EditorContent editor={editor} />
    </div>
  );
}
