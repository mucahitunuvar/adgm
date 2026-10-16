'use client';

import { useEffect, useState } from 'react';
import { safeUrl, SafeHtml } from '@gm/ui';
import type { PublicSiteResponse } from '@gm/api-client';

type Popup = PublicSiteResponse['popups'][number];

function matchesPath(popup: Popup, path: string): boolean {
  switch (popup.targeting.kind) {
    case 'AllPages':
      return true;
    case 'HomeOnly':
      return path === '/';
    case 'Contents':
    case 'Paths':
      return popup.targeting.paths.includes(path);
    default:
      return false;
  }
}

/**
 * ADR-024 §9: "Hangi pop-up'ın gösterileceğine frontend karar verir" - yola uyan en yüksek
 * öncelikli bir `Modal` ve bir `Banner`. Karar özeti: pop-up yalnız ana sayfada (bu bileşen bu
 * yüzden yalnız `HomePage`'den çağrılır, layout'tan değil).
 *
 * AÇIK KARAR (master prompt'un kendi önerdiği varsayılan uygulanıyor, kullanıcıdan yeni bir onay
 * gelmedi): `Frequency` (OncePerSession/EveryNDays) K-07'nin yasakladığı localStorage/sessionStorage
 * gerektirir - bu yüzden her ana sayfa görüntülemesinde gösterilir, yalnız bu sayfa ömrü boyunca
 * kapatılabilir (React state, kalıcı değil). Kalıcı hatırlama istenirse `consentStore` benzeri
 * onaylı bir işlevsel çerez ve ayrı bir karar gerekir.
 */
export function PopupHost({ popups }: { popups: PublicSiteResponse['popups'] }) {
  const [dismissedIds, setDismissedIds] = useState<Set<string>>(new Set());
  const [delayElapsedIds, setDelayElapsedIds] = useState<Set<string>>(new Set());

  const matching = popups.filter((popup) => matchesPath(popup, '/'));
  const modal = matching.filter((p) => p.displayMode === 'Modal').sort((a, b) => Number(b.priority) - Number(a.priority))[0];
  const banner = matching.filter((p) => p.displayMode === 'Banner').sort((a, b) => Number(b.priority) - Number(a.priority))[0];

  useEffect(() => {
    const timers = [modal, banner].filter(Boolean).map((popup) => {
      const delayMs = Number(popup!.delaySeconds ?? 0) * 1000;
      return setTimeout(() => setDelayElapsedIds((prev) => new Set(prev).add(popup!.id)), delayMs);
    });
    return () => timers.forEach(clearTimeout);
    // eslint-disable-next-line react-hooks/exhaustive-deps -- yalnız mount'ta bir kez zamanlayıcı kurulur
  }, []);

  function dismiss(id: string) {
    setDismissedIds((prev) => new Set(prev).add(id));
  }

  return (
    <>
      {banner && !dismissedIds.has(banner.id) && delayElapsedIds.has(banner.id) && <PopupBanner popup={banner} onDismiss={() => dismiss(banner.id)} />}
      {modal && !dismissedIds.has(modal.id) && delayElapsedIds.has(modal.id) && <PopupModal popup={modal} onDismiss={() => dismiss(modal.id)} />}
    </>
  );
}

function PopupBanner({ popup, onDismiss }: { popup: Popup; onDismiss: () => void }) {
  const href = safeUrl(popup.href);
  return (
    <div className="popup-banner" role="region" aria-label={popup.title ?? 'Duyuru'}>
      <div className="container popup-banner__inner">
        <p>{popup.body}</p>
        <div className="popup-banner__actions">
          {href && popup.buttonLabel && (
            <a className="button button--primary" href={href}>
              {popup.buttonLabel}
            </a>
          )}
          {popup.dismissible && (
            <button type="button" className="icon-button" onClick={onDismiss} aria-label="Bandı kapat">
              ×
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

function PopupModal({ popup, onDismiss }: { popup: Popup; onDismiss: () => void }) {
  const href = safeUrl(popup.href);
  const imageUrl = safeUrl(popup.image?.original);
  return (
    <div className="accessible-dialog popup-modal" role="dialog" aria-modal="true" aria-label={popup.title ?? 'Duyuru'}>
      <div className="accessible-dialog__surface">
        <button type="button" className="accessible-dialog__close" onClick={onDismiss} aria-label="Pencereyi kapat">
          ×
        </button>
        {imageUrl && <img src={imageUrl} alt="" loading="lazy" />}
        {popup.title && <h2>{popup.title}</h2>}
        <SafeHtml html={popup.body} />
        {href && popup.buttonLabel && (
          <a className="button button--primary" href={href}>
            {popup.buttonLabel}
          </a>
        )}
      </div>
    </div>
  );
}
