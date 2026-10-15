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
import { Table } from './TableNode';

/**
 * Backend'in iki ayrı modül-özel `HtmlWhitelistPolicy`'si var (Website, Employer -
 * `docs/api/html-whitelist.v1.json`, `tests/ArchitectureTests/HtmlWhitelistContractTests.cs`
 * tarafından denetlenir). Editör bu iki kuralı tek bir `WhitelistProfile` parametresiyle yansıtır;
 * `schema.contract.test.ts` her iki profilin de ürettiği etiket/öznitelik kümesinin kendi manifest
 * bölümünün ALT KÜMESİ olduğunu doğrular.
 *
 * Employer'ın whitelist'i kasıtlı olarak daha küçük (AboutHtml kısa bir "hakkımızda" metni - medya
 * gömme/tablo/üstü-çizili gerektirmiyor): img/figure/figcaption/table/hr/s yok. `createRichTextExtensions`
 * bu farkı `WEBSITE_ONLY_EXTENSIONS` ile yansıtır.
 */
export type WhitelistProfile = 'website' | 'employer';

const configuredHeading = Heading.configure({ levels: [2, 3, 4] });
const configuredTable = Table.configure({ resizable: false });

/**
 * Her iki profilde de ortak (Employer `HtmlWhitelistPolicy.AllowedTags`'in TAMAMI, Website'in bir
 * alt kümesi): `p, br, strong, b, em, i, u, h2, h3, h4, ul, ol, li, blockquote, a`.
 */
const BASE_EXTENSIONS: AnyExtension[] = [
  Document,
  Paragraph,
  Text,
  Bold,
  Italic,
  Underline,
  configuredHeading,
  BulletList,
  OrderedList,
  ListItem,
  ListKeymap,
  Blockquote,
  HardBreak,
  History,
  Gapcursor,
  Dropcursor,
  Link,
];

/**
 * Yalnız Website profilinde var: `s, table, thead, tbody, tr, th, td, hr, figure, figcaption, img`.
 * `iframe` kasıtlı olarak yok (A11 §3: YouTube/iframe gömme bu dilimde kapalı, Faz 3'te CSP
 * frame-src kararıyla birlikte ele alınacak - ACIK-MADDELER'e not düşüldü). `code`/`pre` de kasıtlı
 * olarak yok (whitelist'te değiller - ne Website ne Employer).
 */
const WEBSITE_ONLY_EXTENSIONS: AnyExtension[] = [
  Strike,
  configuredTable,
  TableRow,
  TableHeader,
  TableCell,
  HorizontalRule,
  Figure,
  Figcaption,
  Image,
];

/**
 * `schema.test.ts`, her uzantının `renderHTML`'inin ürettiği etiket adını manifestle karşılaştırarak
 * yeni bir uzantı eklendiğinde whitelist'ten sapmayı derleme zamanında değil ama test zamanında yakalar.
 */
export function createRichTextExtensions(profile: WhitelistProfile): AnyExtension[] {
  return profile === 'website' ? [...BASE_EXTENSIONS, ...WEBSITE_ONLY_EXTENSIONS] : [...BASE_EXTENSIONS];
}

/**
 * Faz 5'e kadar (İşveren portalı, ilan açıklaması) tek kullanılan profil budur - `apps/admin`
 * bunu kullanır. `'employer'` profili bugün hiçbir arayüzde kullanılmıyor; yalnız
 * `schema.contract.test.ts`'te manifestle doğrulanıyor (bkz. o dosyadaki yorum).
 */
export const richTextExtensions: AnyExtension[] = createRichTextExtensions('website');
