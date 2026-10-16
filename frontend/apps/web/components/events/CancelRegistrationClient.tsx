'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { createApiClient, cancelEventRegistration, getApiErrorMessage } from '@gm/api-client';

type Status = 'loading' | 'success' | 'error' | 'missing-token';

/** İptal idempotenttir (ADR-024 §11.2): bu sayfa birden açılsa da aynı sonucu gösterir. */
export function CancelRegistrationClient() {
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<Status>('loading');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const token = searchParams.get('token');
    window.history.replaceState(null, '', window.location.pathname);
    const client = createApiClient({ baseUrl: window.location.origin });
    const run = token ? cancelEventRegistration(client, token).then(() => setStatus('success')) : Promise.resolve().then(() => setStatus('missing-token'));
    run.catch((error: unknown) => {
      setMessage(getApiErrorMessage(error));
      setStatus('error');
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps -- yalnız mount'ta bir kez token işlenir
  }, []);

  return (
    <div className="container section token-page">
      <h1>Etkinlik kaydı iptali</h1>
      {status === 'loading' && <p>İşleniyor…</p>}
      {status === 'success' && <p>Kaydınız iptal edildi.</p>}
      {status === 'missing-token' && <p>İptal bağlantısı geçersiz.</p>}
      {status === 'error' && <p role="alert">{message}</p>}
    </div>
  );
}
