/**
 * Tema (açık/koyu) yardımcıları. K-07: tarayıcıda saklanan TEK şey burasıdır (tema tercihi).
 * Token, oturum, kullanıcı verisi burada asla saklanmaz.
 */

export type Theme = 'theme-light' | 'theme-dark';

export const THEME_STORAGE_KEY = 'theme';

/**
 * <head> içine satır içi basılacak betik metni. Tema sınıfını ilk boyamadan önce
 * atar (flash önleme). CSP nonce/hash ile kullanılmalıdır.
 */
export const themeInitScript = `(function () {
  try {
    var stored = localStorage.getItem('${THEME_STORAGE_KEY}');
    var theme = stored === 'theme-light' || stored === 'theme-dark'
      ? stored
      : (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'theme-dark' : 'theme-light');
    document.documentElement.classList.add(theme);
  } catch (e) {}
})();`;

export function getStoredTheme(): Theme | null {
  try {
    const value = localStorage.getItem(THEME_STORAGE_KEY);
    return value === 'theme-light' || value === 'theme-dark' ? value : null;
  } catch {
    return null;
  }
}

export function setStoredTheme(theme: Theme): void {
  try {
    localStorage.setItem(THEME_STORAGE_KEY, theme);
  } catch {
    /* localStorage kapalı/engelli olabilir; tema yalnız bu oturumda uygulanır. */
  }
}

export function applyTheme(theme: Theme): void {
  const root = document.documentElement;
  root.classList.remove('theme-light', 'theme-dark');
  root.classList.add(theme);
  setStoredTheme(theme);
}

export function getSystemTheme(): Theme {
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'theme-dark' : 'theme-light';
}

export function getActiveTheme(): Theme {
  return document.documentElement.classList.contains('theme-dark') ? 'theme-dark' : 'theme-light';
}

export function toggleTheme(): Theme {
  const next: Theme = getActiveTheme() === 'theme-dark' ? 'theme-light' : 'theme-dark';
  applyTheme(next);
  return next;
}
