const LINE_SEPARATOR = String.fromCharCode(0x2028);
const PARAGRAPH_SEPARATOR = String.fromCharCode(0x2029);

/**
 * Backend'in hazır döndürdüğü schema.org nesnesini `<script type="application/ld+json">` içine
 * basmak için TEK yardımcı (FRONTEND-GUVENLIK.md §3, SECURITY.md §23.5). `JSON.stringify` çıktısında
 * `<`, `>`, `&`, satır ayırıcıları kaçışlanır - aksi halde `</script>` dizisi script'i kırabilir.
 */
export function serializeJsonLd(value: unknown): string {
  return JSON.stringify(value)
    .split('<').join('\\u003c')
    .split('>').join('\\u003e')
    .split('&').join('\\u0026')
    .split(LINE_SEPARATOR).join('\\u2028')
    .split(PARAGRAPH_SEPARATOR).join('\\u2029');
}
