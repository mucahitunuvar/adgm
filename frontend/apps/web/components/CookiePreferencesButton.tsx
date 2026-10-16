'use client';

export const OPEN_COOKIE_PREFERENCES_EVENT = 'gm:open-cookie-preferences';

/** Her sayfada bulunması gereken "Çerez tercihleri" bağlantısı (FRONTEND-GUVENLIK.md §9). */
export function CookiePreferencesButton() {
  return (
    <button
      type="button"
      className="site-footer__cookie-link"
      onClick={() => window.dispatchEvent(new CustomEvent(OPEN_COOKIE_PREFERENCES_EVENT))}
    >
      Çerez tercihleri
    </button>
  );
}
