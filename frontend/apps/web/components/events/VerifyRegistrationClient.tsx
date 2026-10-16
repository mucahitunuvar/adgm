'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { createApiClient, verifyEventRegistration, getApiErrorMessage } from '@gm/api-client';

type Status = 'loading' | 'success' | 'error' | 'missing-token';

/**
 * Token'lı sayfa (SECURITY.md/FRONTEND-FAZ3 ortak ilkeler): token okunur, backend'e gönderilir,
 * sonra `history.replaceState` ile adres çubuğundan silinir - log/analitiğe hiç yazılmaz.
 */
export function VerifyRegistrationClient() {
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<Status>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const token = searchParams.get('token');
    window.history.replaceState(null, '', window.location.pathname);
    const client = createApiClient({ baseUrl: window.location.origin });
    const run = token ? verifyEventRegistration(client, token).then(() => setStatus('success')) : Promise.resolve().then(() => setStatus('missing-token'));
    run.catch((error: unknown) => {
      setMessage(getApiErrorMessage(error));
      setStatus('error');
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps -- yalnız mount'ta bir kez token işlenir
  }, []);

  return (
    <div className="container section token-page">
      <h1>Etkinlik kaydı doğrulama</h1>
      {status === 'loading' && <p>Doğrulanıyor…</p>}
      {status === 'success' && <p>Kaydınız doğrulandı. Teşekkür ederiz.</p>}
      {status === 'missing-token' && <p>Doğrulama bağlantısı geçersiz.</p>}
      {status === 'error' && <p role="alert">{message}</p>}
    </div>
  );
}
