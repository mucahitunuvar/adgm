/**
 * `dangerouslySetInnerHTML` kullanımına izin verilen TEK yer burasıdır
 * (FRONTEND-GUVENLIK.md §3, ESLint no-restricted-syntax istisnası).
 * Yalnız backend'in sanitize ettiği zengin metin alanları için kullanılır;
 * kullanıcı girdisini doğrudan burada basmayın.
 */
export function SafeHtml({ html, className = '' }: { html: string; className?: string }) {
  return <div className={className} dangerouslySetInnerHTML={{ __html: html }} />;
}
