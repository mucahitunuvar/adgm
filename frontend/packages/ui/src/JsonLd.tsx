import { serializeJsonLd } from './serializeJsonLd';

/**
 * `<script type="application/ld+json">` basmak için TEK yer (`SafeHtml`'in JSON-LD karşılığı,
 * FRONTEND-GUVENLIK.md §3, SECURITY.md §23.5). İçerik her zaman `serializeJsonLd` ile kaçışlanır;
 * başka hiçbir yerde JSON-LD için `dangerouslySetInnerHTML` kullanılmaz.
 */
export function JsonLd({ data, nonce }: { data: unknown; nonce?: string }) {
  return <script type="application/ld+json" nonce={nonce} dangerouslySetInnerHTML={{ __html: serializeJsonLd(data) }} />;
}
