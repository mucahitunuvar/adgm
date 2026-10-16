'use client';

import { useEffect, useId, useRef } from 'react';

declare global {
  interface Window {
    turnstile?: {
      render: (container: string | HTMLElement, options: { sitekey: string; callback: (token: string) => void; 'error-callback'?: () => void }) => string;
      remove: (widgetId: string) => void;
    };
  }
}

const SCRIPT_SRC = 'https://challenges.cloudflare.com/turnstile/v0/api.js';
let scriptLoadPromise: Promise<void> | null = null;

function loadTurnstileScript(): Promise<void> {
  if (window.turnstile) return Promise.resolve();
  scriptLoadPromise ??= new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.src = SCRIPT_SRC;
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error('Turnstile script could not be loaded.'));
    document.head.appendChild(script);
  });
  return scriptLoadPromise;
}

/**
 * ADR-024 §12.3: Turnstile npm paketi değil, yalnız `challenges.cloudflare.com` script'i
 * (CSP `script-src`/`frame-src` zaten izinli, bkz. `proxy.ts`). Yalnız `site.turnstileSiteKey`
 * boş değilse render edilir (bot koruması o zaman etkin kabul edilir).
 */
export function TurnstileWidget({ siteKey, onToken }: { siteKey: string; onToken: (token: string | null) => void }) {
  const containerId = useId().replace(/:/g, '-');
  const containerRef = useRef<HTMLDivElement>(null);
  const widgetIdRef = useRef<string | null>(null);
  // Widget'ı `onToken` her render'da yeniden kurmak yerine her zaman en güncel geri çağrımı
  // çağıran bir ref kullanılır - `effect` yalnız `siteKey` değişince yeniden kurulur. Ref'in
  // kendisi render sırasında değil, ayrı bir effect içinde güncellenir (react-hooks/refs).
  const onTokenRef = useRef(onToken);
  useEffect(() => {
    onTokenRef.current = onToken;
  });

  useEffect(() => {
    let cancelled = false;
    loadTurnstileScript()
      .then(() => {
        if (cancelled || !containerRef.current || !window.turnstile) return;
        widgetIdRef.current = window.turnstile.render(containerRef.current, {
          sitekey: siteKey,
          callback: (token) => onTokenRef.current(token),
          'error-callback': () => onTokenRef.current(null),
        });
      })
      .catch(() => onTokenRef.current(null));

    return () => {
      cancelled = true;
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.remove(widgetIdRef.current);
      }
    };
  }, [siteKey]);

  return <div id={containerId} ref={containerRef} className="turnstile-widget" />;
}
