'use client';

import { useEffect, useState, useSyncExternalStore } from 'react';
import { createApiClient, createCookieConsentRecord, CookieConsentAction, type PublicSiteResponse } from '@gm/api-client';
import { AccessibleDialog } from '@gm/ui';
import { createAnonymousConsentId, readConsent, writeConsent, type ConsentState } from '../lib/consentStore';
import { OPEN_COOKIE_PREFERENCES_EVENT } from './CookiePreferencesButton';

const NECESSARY = 'Necessary';

function noopSubscribe(): () => void {
  return () => {};
}

function getServerConsentSnapshot(): ConsentState | null {
  return null;
}

/**
 * FRONTEND-GUVENLIK.md §9 + FRONTEND-FAZ3-MASTER-PROMPT.md "Depolama kuralı". Tercih yalnız
 * istemcide, hydration sonrası okunur (SSR'da okunursa sayfalar önbelleğe giremez). `policyVersion`
 * yoksa (çerez politikası henüz yayınlanmamış) banner hiç gösterilmez - kaydedilecek bir sürüm yok.
 * Çerez okuma `useSyncExternalStore` ile (ThemeToggle'daki gerekçeyle aynı: effect içinde senkron
 * `setState` yok - `persist()`/`setDismissed` her zaman bir olay işleyicisi içinde çağrılır).
 */
export function CookieConsentBanner({ cookieConsent }: { cookieConsent: PublicSiteResponse['cookieConsent'] }) {
  const consent = useSyncExternalStore(noopSubscribe, readConsent, getServerConsentSnapshot);
  const [dismissed, setDismissed] = useState(false);
  const [preferencesOpen, setPreferencesOpen] = useState(false);
  const [selected, setSelected] = useState<Set<string>>(new Set([NECESSARY]));

  useEffect(() => {
    const onOpenPreferences = () => {
      setSelected(new Set(readConsent()?.categories ?? [NECESSARY]));
      setPreferencesOpen(true);
    };
    window.addEventListener(OPEN_COOKIE_PREFERENCES_EVENT, onOpenPreferences);
    return () => window.removeEventListener(OPEN_COOKIE_PREFERENCES_EVENT, onOpenPreferences);
  }, []);

  if (cookieConsent.policyVersion == null) {
    return null;
  }

  const showBanner = !dismissed && consent === null && !preferencesOpen;

  function persist(categories: string[], action: CookieConsentAction) {
    if (cookieConsent.policyVersion == null) return;
    const policyVersion = Number(cookieConsent.policyVersion);
    const state: ConsentState = { consentId: createAnonymousConsentId(), policyVersion, categories, action };
    writeConsent(state);
    // Tarayıcıdan aynı origin'e (Next'in /api rewrite'ı üzerinden) - FRONTEND-ACIK-MADDELER.md
    // madde 22'deki CORS/baseUrl hatasını tekrarlamamak için `window.location.origin` kullanılır,
    // BACKEND_ORIGIN değil (o yalnız sunucu tarafı fetch'ler içindir).
    const client = createApiClient({ baseUrl: window.location.origin });
    createCookieConsentRecord(client, state).catch(() => {
      // Kayıt backend'e ulaşmasa da tarayıcıdaki tercih geçerli kalır; ziyaretçi engellenmez.
    });
    setDismissed(true);
    setPreferencesOpen(false);
  }

  const allCategories = cookieConsent.categories.map((category) => category.category);

  return (
    <>
      {showBanner && (
        <div className="cookie-banner" role="region" aria-label={cookieConsent.bannerTitle || 'Çerez tercihleri'}>
          <div className="container cookie-banner__inner">
            <div>
              <p className="cookie-banner__title">{cookieConsent.bannerTitle}</p>
              <p>{cookieConsent.bannerText}</p>
            </div>
            <div className="cookie-banner__actions">
              <button type="button" className="button button--quiet" onClick={() => setPreferencesOpen(true)}>
                Özelleştir
              </button>
              <button type="button" className="button button--secondary" onClick={() => persist([NECESSARY], CookieConsentAction.RejectAll)}>
                Tümünü reddet
              </button>
              <button
                type="button"
                className="button button--primary"
                onClick={() => persist([NECESSARY, ...allCategories], CookieConsentAction.AcceptAll)}
              >
                Tümünü kabul et
              </button>
            </div>
          </div>
        </div>
      )}

      <AccessibleDialog open={preferencesOpen} onClose={() => setPreferencesOpen(false)} title="Çerez tercihleri">
        <h2>Çerez tercihleri</h2>
        <ul className="cookie-preferences__list">
          <li>
            <label>
              <input type="checkbox" checked readOnly /> Zorunlu
            </label>
            <p className="field-hint">Sitenin çalışması için gereklidir, kapatılamaz.</p>
          </li>
          {cookieConsent.categories
            .filter((category) => category.category !== NECESSARY)
            .map((category) => (
              <li key={category.category}>
                <label>
                  <input
                    type="checkbox"
                    checked={selected.has(category.category)}
                    onChange={(event) => {
                      const next = new Set(selected);
                      if (event.target.checked) next.add(category.category);
                      else next.delete(category.category);
                      setSelected(next);
                    }}
                  />{' '}
                  {category.category}
                </label>
                {category.description && <p className="field-hint">{category.description}</p>}
              </li>
            ))}
        </ul>
        <div className="confirm-dialog__actions">
          <button
            type="button"
            className="button button--primary"
            onClick={() => persist([NECESSARY, ...selected].filter((v, i, arr) => arr.indexOf(v) === i), CookieConsentAction.Custom)}
          >
            Kaydet
          </button>
        </div>
      </AccessibleDialog>
    </>
  );
}
