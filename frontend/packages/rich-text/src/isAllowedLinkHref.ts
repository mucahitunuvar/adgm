const ALLOWED_SCHEMES = new Set(['http', 'https', 'mailto', 'tel']);

/**
 * Backend'in `HtmlSanitizerContentSanitizer.ProcessAnchor` ve `AllowedSchemes` kuralının birebir
 * yansıması: şemasız (site içi göreli) bağlantılar kısıtlamasız geçer; `//host/yol` biçimindeki
 * protokolden-bağımsız bağlantılar backend tarafından reddedildiği için burada da reddedilir;
 * mutlak bağlantılar yalnız http/https/mailto/tel şemalarıyla kabul edilir - `javascript:`,
 * `data:` ve boş şema (`:`) bu kümenin dışında kaldığı için otomatik reddedilir.
 */
export function isAllowedLinkHref(href: string): boolean {
  const trimmed = href.trim();
  if (trimmed.length === 0) {
    return false;
  }

  if (trimmed.startsWith('//')) {
    return false;
  }

  const schemeMatch = /^([a-zA-Z][a-zA-Z0-9+.-]*):/.exec(trimmed);
  if (!schemeMatch) {
    return true;
  }

  return ALLOWED_SCHEMES.has(schemeMatch[1]!.toLowerCase());
}
