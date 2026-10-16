import { CookieConsentAction } from '@gm/api-client';

// FRONTEND-FAZ3-MASTER-PROMPT.md "Depolama kuralı": çerez tercihi için TEK yardımcı. Birinci
// taraf, JS ile yazılan, kimliksiz (consentId rastgele - hiçbir kullanıcı/oturumla eşlenmez),
// `SameSite=Lax`, üretimde `Secure`, ~180 gün ömürlü zorunlu bir çerez - localStorage/sessionStorage
// KULLANILMAZ (K-07 bu ikisini tema dışında tamamen yasaklıyor). Yalnız istemcide, hydration
// sonrası okunur/yazılır - SSR'da okunursa sayfalar önbelleğe giremez (ortak ilkeler).
const COOKIE_NAME = 'gm_consent';
const COOKIE_MAX_AGE_SECONDS = 180 * 24 * 60 * 60;

export interface ConsentState {
  consentId: string;
  policyVersion: number;
  /** Her zaman "Necessary" içerir; backend'in `site.cookieConsent.categories[].category` ile aynı kümedir. */
  categories: readonly string[];
  action: CookieConsentAction;
}

function parseCookieHeader(): Record<string, string> {
  if (typeof document === 'undefined') return {};
  return document.cookie
    .split(';')
    .map((part) => part.trim())
    .filter(Boolean)
    .reduce<Record<string, string>>((acc, part) => {
      const separatorIndex = part.indexOf('=');
      if (separatorIndex === -1) return acc;
      acc[part.slice(0, separatorIndex)] = decodeURIComponent(part.slice(separatorIndex + 1));
      return acc;
    }, {});
}

export function readConsent(): ConsentState | null {
  const raw = parseCookieHeader()[COOKIE_NAME];
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as Partial<ConsentState>;
    if (
      typeof parsed.consentId !== 'string' ||
      typeof parsed.policyVersion !== 'number' ||
      !Array.isArray(parsed.categories) ||
      typeof parsed.action !== 'string'
    ) {
      return null;
    }
    return {
      consentId: parsed.consentId,
      policyVersion: parsed.policyVersion,
      categories: parsed.categories,
      action: parsed.action as CookieConsentAction,
    };
  } catch {
    return null;
  }
}

export function writeConsent(state: ConsentState): void {
  if (typeof document === 'undefined') return;
  const isProd = process.env.NODE_ENV === 'production';
  const value = encodeURIComponent(JSON.stringify(state));
  const attributes = [`path=/`, `max-age=${COOKIE_MAX_AGE_SECONDS}`, 'SameSite=Lax', ...(isProd ? ['Secure'] : [])];
  document.cookie = `${COOKIE_NAME}=${value}; ${attributes.join('; ')}`;
}

export function createAnonymousConsentId(): string {
  return crypto.randomUUID();
}

export function hasCategoryConsent(state: ConsentState | null, category: string): boolean {
  return category === 'Necessary' || (state?.categories.includes(category) ?? false);
}
