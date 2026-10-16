'use client';

import { useSyncExternalStore } from 'react';
import { Moon, Sun } from 'lucide-react';
import { getActiveTheme, toggleTheme } from '@gm/design-tokens';

/**
 * `themeInitScript` tema sınıfını hydration'dan önce <html>'e uygular (flash önleme); bu bileşen
 * yalnız doğru ikonu göstermek için o sınıfı okur. `useSyncExternalStore` ile: SSR'da sabit bir
 * varsayılan (`getServerSnapshot`) döner, hydration sonrası gerçek sınıfa anında geçer - effect
 * içinde senkron `setState` çağırmadan (eslint-plugin-react-hooks `set-state-in-effect`, bkz.
 * FRONTEND-ACIK-MADDELER.md madde 31'deki aynı kuralla ilgili not).
 */
function subscribe(onStoreChange: () => void): () => void {
  const observer = new MutationObserver(onStoreChange);
  observer.observe(document.documentElement, { attributes: true, attributeFilter: ['class'] });
  return () => observer.disconnect();
}

function getServerSnapshot() {
  return 'theme-light' as const;
}

export function ThemeToggle() {
  const theme = useSyncExternalStore(subscribe, getActiveTheme, getServerSnapshot);

  return (
    <button
      type="button"
      className="icon-button"
      onClick={() => toggleTheme()}
      aria-label={theme === 'theme-dark' ? 'Açık temaya geç' : 'Koyu temaya geç'}
    >
      {theme === 'theme-dark' ? <Sun aria-hidden="true" /> : <Moon aria-hidden="true" />}
    </button>
  );
}
