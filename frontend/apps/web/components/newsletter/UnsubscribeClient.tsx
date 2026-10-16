'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { createApiClient, unsubscribeFromNewsletter, getApiErrorMessage } from '@gm/api-client';

type Status = 'loading' | 'success' | 'error' | 'missing-token';

/**
 * Backend henüz çıkış (unsubscribe) bağlantısını e-postayla göndermiyor (BLOK-ESLEMESI.md
 * önkoşul 3-g, kapanmamış kısım) - uç zaten var, bu sayfa onu tüketmeye hazır; e-posta linki
 * eklendiğinde yol adı burada sabitlenir.
 */
export function UnsubscribeClient() {
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<Status>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const token = searchParams.get('token');
    window.history.replaceState(null, '', window.location.pathname);
    const client = createApiClient({ baseUrl: window.location.origin });
    const run = token
      ? unsubscribeFromNewsletter(client, token).then(() => setStatus('success'))
      : Promise.resolve().then(() => setStatus('missing-token'));
    run.catch((error: unknown) => {
      setMessage(getApiErrorMessage(error));
      setStatus('error');
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps -- yalnız mount'ta bir kez token işlenir
  }, []);

  return (
    <div className="container section token-page">
      <h1>E-bülten aboneliğinden çıkış</h1>
      {status === 'loading' && <p>İşleniyor…</p>}
      {status === 'success' && <p>Abonelikten çıktınız.</p>}
      {status === 'missing-token' && <p>Bağlantı geçersiz.</p>}
      {status === 'error' && <p role="alert">{message}</p>}
    </div>
  );
}
