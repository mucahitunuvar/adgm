import type { AnyExtension } from '@tiptap/core';
import Document from '@tiptap/extension-document';
import Paragraph from '@tiptap/extension-paragraph';
import Text from '@tiptap/extension-text';
import Bold from '@tiptap/extension-bold';
import Italic from '@tiptap/extension-italic';
import Underline from '@tiptap/extension-underline';
import Strike from '@tiptap/extension-strike';
import Heading from '@tiptap/extension-heading';
import BulletList from '@tiptap/extension-bullet-list';
import OrderedList from '@tiptap/extension-ordered-list';
import ListItem from '@tiptap/extension-list-item';
import ListKeymap from '@tiptap/extension-list-keymap';
import Blockquote from '@tiptap/extension-blockquote';
import { Table } from '@tiptap/extension-table';
import TableRow from '@tiptap/extension-table-row';
import TableHeader from '@tiptap/extension-table-header';
import TableCell from '@tiptap/extension-table-cell';
import HorizontalRule from '@tiptap/extension-horizontal-rule';
import HardBreak from '@tiptap/extension-hard-break';
import History from '@tiptap/extension-history';
import Gapcursor from '@tiptap/extension-gapcursor';
import Dropcursor from '@tiptap/extension-dropcursor';
import { Figure } from './FigureNode';
import { Figcaption } from './FigcaptionNode';
import { Image } from './ImageNode';
import { Link } from './LinkMark';

/**
 * TEK kaynak: bu listedeki düğüm/işaretlerin ürettiği etiket kümesi, backend'in
 * `HtmlSanitizerContentSanitizer.cs` (ADR-024 §16) `AllowedTags` kümesiyle BİREBİR eşleşir -
 * `p, br, strong, b, em, i, u, s, h2, h3, h4, ul, ol, li, blockquote, a, img, figure, figcaption,
 * table, thead, tbody, tr, th, td, hr`. `iframe` kasıtlı olarak yok (A11 §3: YouTube/iframe gömme bu
 * dilimde kapalı, Faz 3'te CSP frame-src kararıyla birlikte ele alınacak - ACIK-MADDELER'e not
 * düşüldü). `code`/`pre` de kasıtlı olarak yok (whitelist'te değiller).
 *
 * `RichTextEditorSchema.test.ts`, her uzantının `renderHTML`'inin ürettiği etiket adını bu listeyle
 * karşılaştırarak yeni bir uzantı eklendiğinde whitelist'ten sapmayı derleme zamanında değil ama
 * test zamanında yakalar.
 */
export const richTextExtensions: AnyExtension[] = [
  Document,
  Paragraph,
  Text,
  Bold,
  Italic,
  Underline,
  Strike,
  Heading.configure({ levels: [2, 3, 4] }),
  BulletList,
  OrderedList,
  ListItem,
  ListKeymap,
  Blockquote,
  Table.configure({ resizable: false }),
  TableRow,
  TableHeader,
  TableCell,
  HorizontalRule,
  HardBreak,
  History,
  Gapcursor,
  Dropcursor,
  Figure,
  Figcaption,
  Image,
  Link,
];
