'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { createApiClient, confirmNewsletterSubscription, getApiErrorMessage } from '@gm/api-client';

type Status = 'loading' | 'success' | 'error' | 'missing-token';

/** Backend'in ürettiği gerçek bağlantı: `{PublicSiteBaseUrl}/newsletter/confirm?token=` (BLOK-ESLEMESI.md önkoşul 3-g). */
export function ConfirmSubscriptionClient() {
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<Status>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const token = searchParams.get('token');
    window.history.replaceState(null, '', window.location.pathname);
    const client = createApiClient({ baseUrl: window.location.origin });
    const run = token
      ? confirmNewsletterSubscription(client, token).then(() => setStatus('success'))
      : Promise.resolve().then(() => setStatus('missing-token'));
    run.catch((error: unknown) => {
      setMessage(getApiErrorMessage(error));
      setStatus('error');
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps -- yalnız mount'ta bir kez token işlenir
  }, []);

  return (
    <div className="container section token-page">
      <h1>E-bülten aboneliği</h1>
      {status === 'loading' && <p>Doğrulanıyor…</p>}
      {status === 'success' && <p>Aboneliğiniz onaylandı. Teşekkür ederiz.</p>}
      {status === 'missing-token' && <p>Onay bağlantısı geçersiz.</p>}
      {status === 'error' && <p role="alert">{message}</p>}
    </div>
  );
}
