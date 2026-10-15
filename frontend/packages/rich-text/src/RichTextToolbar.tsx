import type { Editor } from '@tiptap/core';
import { useEditorState } from '@tiptap/react';
import {
  Bold,
  Italic,
  Underline,
  Strikethrough,
  Heading2,
  Heading3,
  Heading4,
  List,
  ListOrdered,
  Quote,
  Link as LinkIcon,
  Image as ImageIcon,
  Table as TableIcon,
  Minus,
  Undo2,
  Redo2,
} from 'lucide-react';
import { useState } from 'react';
import { InsertLinkDialog } from './InsertLinkDialog';

interface ToolbarButtonSpec {
  label: string;
  icon: typeof Bold;
  isActive?: boolean;
  disabled?: boolean;
  onClick: () => void;
}

function ToolbarButton({ label, icon: Icon, isActive, disabled, onClick }: ToolbarButtonSpec) {
  return (
    <button
      type="button"
      className={`rich-text-toolbar__button${isActive ? ' rich-text-toolbar__button--active' : ''}`}
      aria-label={label}
      aria-pressed={isActive}
      disabled={disabled}
      onClick={onClick}
    >
      <Icon aria-hidden="true" size={18} />
    </button>
  );
}

export function RichTextToolbar({
  editor,
  onRequestImage,
}: {
  editor: Editor;
  onRequestImage: () => Promise<{ src: string; alt: string } | null>;
}) {
  const [linkDialogOpen, setLinkDialogOpen] = useState(false);

  const state = useEditorState({
    editor,
    selector: ({ editor: current }) => ({
      bold: current.isActive('bold'),
      italic: current.isActive('italic'),
      underline: current.isActive('underline'),
      strike: current.isActive('strike'),
      heading2: current.isActive('heading', { level: 2 }),
      heading3: current.isActive('heading', { level: 3 }),
      heading4: current.isActive('heading', { level: 4 }),
      bulletList: current.isActive('bulletList'),
      orderedList: current.isActive('orderedList'),
      blockquote: current.isActive('blockquote'),
      link: current.isActive('link'),
      canUndo: current.can().undo(),
      canRedo: current.can().redo(),
    }),
  });

  async function handleInsertImage() {
    const picked = await onRequestImage();
    if (!picked) return;
    editor
      .chain()
      .focus()
      .insertContent({
        type: 'figure',
        content: [{ type: 'image', attrs: { src: picked.src, alt: picked.alt } }],
      })
      .run();
  }

  function handleSubmitLink(href: string, openInNewTab: boolean) {
    editor
      .chain()
      .focus()
      .extendMarkRange('link')
      .setLink({ href, target: openInNewTab ? '_blank' : null })
      .run();
    setLinkDialogOpen(false);
  }

  return (
    <div className="rich-text-toolbar" role="toolbar" aria-label="Biçimlendirme araç çubuğu">
      <ToolbarButton label="Kalın" icon={Bold} isActive={state.bold} onClick={() => editor.chain().focus().toggleBold().run()} />
      <ToolbarButton label="İtalik" icon={Italic} isActive={state.italic} onClick={() => editor.chain().focus().toggleItalic().run()} />
      <ToolbarButton
        label="Alt çizili"
        icon={Underline}
        isActive={state.underline}
        onClick={() => editor.chain().focus().toggleUnderline().run()}
      />
      <ToolbarButton
        label="Üstü çizili"
        icon={Strikethrough}
        isActive={state.strike}
        onClick={() => editor.chain().focus().toggleStrike().run()}
      />
      <ToolbarButton
        label="Başlık 2"
        icon={Heading2}
        isActive={state.heading2}
        onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()}
      />
      <ToolbarButton
        label="Başlık 3"
        icon={Heading3}
        isActive={state.heading3}
        onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()}
      />
      <ToolbarButton
        label="Başlık 4"
        icon={Heading4}
        isActive={state.heading4}
        onClick={() => editor.chain().focus().toggleHeading({ level: 4 }).run()}
      />
      <ToolbarButton
        label="Madde işaretli liste"
        icon={List}
        isActive={state.bulletList}
        onClick={() => editor.chain().focus().toggleBulletList().run()}
      />
      <ToolbarButton
        label="Numaralı liste"
        icon={ListOrdered}
        isActive={state.orderedList}
        onClick={() => editor.chain().focus().toggleOrderedList().run()}
      />
      <ToolbarButton
        label="Alıntı"
        icon={Quote}
        isActive={state.blockquote}
        onClick={() => editor.chain().focus().toggleBlockquote().run()}
      />
      <ToolbarButton label="Bağlantı ekle" icon={LinkIcon} isActive={state.link} onClick={() => setLinkDialogOpen(true)} />
      <ToolbarButton label="Resim ekle" icon={ImageIcon} onClick={() => void handleInsertImage()} />
      <ToolbarButton
        label="Tablo ekle"
        icon={TableIcon}
        onClick={() => editor.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run()}
      />
      <ToolbarButton label="Yatay çizgi ekle" icon={Minus} onClick={() => editor.chain().focus().setHorizontalRule().run()} />
      <ToolbarButton label="Geri al" icon={Undo2} disabled={!state.canUndo} onClick={() => editor.chain().focus().undo().run()} />
      <ToolbarButton label="Yinele" icon={Redo2} disabled={!state.canRedo} onClick={() => editor.chain().focus().redo().run()} />
      <InsertLinkDialog
        open={linkDialogOpen}
        initialHref={editor.getAttributes('link').href ?? ''}
        onCancel={() => setLinkDialogOpen(false)}
        onSubmit={handleSubmitLink}
      />
    </div>
  );
}
