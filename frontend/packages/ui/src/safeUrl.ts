const ALLOWED_SCHEMES = ['http:', 'https:', 'mailto:', 'tel:'];

/**
 * İçerik/kullanıcı kaynaklı `href`/`src` değerlerini doğrular (FRONTEND-GUVENLIK.md §3).
 * `javascript:`, `data:` ve protokolden bağımsız (`//host`) adresler reddedilir.
 * Köke-göreli yollar (`/haberler/x`) her zaman güvenli kabul edilir.
 * Geçersizse `undefined` döner; çağıran `href` özniteliğini hiç basmaz.
 */
export function safeUrl(value: string | null | undefined): string | undefined {
  if (!value) return undefined;
  const trimmed = value.trim();
  if (trimmed === '') return undefined;
  if (trimmed.startsWith('/') && !trimmed.startsWith('//')) return trimmed;

  try {
    const parsed = new URL(trimmed);
    return ALLOWED_SCHEMES.includes(parsed.protocol) ? trimmed : undefined;
  } catch {
    return undefined;
  }
}
