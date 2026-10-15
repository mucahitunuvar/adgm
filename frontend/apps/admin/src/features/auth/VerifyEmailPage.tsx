import { verifyEmail } from '@gm/api-client';
import { getAuthErrorMessage, useAuth } from '@gm/auth';
import { LoadingState, PageHeader } from '@gm/ui';
import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { ROUTES } from '../../routes';

/**
 * E-postadaki bağlantı backend'in `GET /auth/verify-email`'ine gider; bu SPA sayfası aynı
 * token'ı **POST** ile backend'e onaylar (GET yerine) — backend'in kendi kaynak koduna göre bu,
 * token'ı backend'in sunucu erişim loglarından uzak tutar
 * (src/Modules/Identity/Features/VerifyEmail/VerifyEmailEndpoint.cs). Token URL'den bir kez okunup
 * hemen adres çubuğundan silinir.
 */
export function VerifyEmailPage() {
  const { client } = useAuth();
  const [searchParams] = useSearchParams();
  const [token] = useState(() => searchParams.get('token') ?? '');
  const [status, setStatus] = useState<'pending' | 'success' | 'error'>(() => (token ? 'pending' : 'error'));
  const [error, setError] = useState<string | null>(() => (token ? null : 'Doğrulama bağlantısı geçersiz.'));

  useEffect(() => {
    if (window.location.search) {
      window.history.replaceState(null, '', window.location.pathname);
    }
  }, []);

  useEffect(() => {
    if (!token) return;
    let active = true;
    verifyEmail(client, token)
      .then(() => {
        if (active) setStatus('success');
      })
      .catch((verifyError: unknown) => {
        if (!active) return;
        setStatus('error');
        setError(getAuthErrorMessage(verifyError));
      });
    return () => {
      active = false;
    };
  }, [client, token]);

  return (
    <main className="container section auth-page">
      <PageHeader title="E-posta doğrulama" />
      {status === 'pending' && <LoadingState label="E-posta adresiniz doğrulanıyor…" />}
      {status === 'success' && (
        <>
          <p className="status status--success" role="status">
            E-posta adresiniz doğrulandı.
          </p>
          <p className="auth-page__link">
            <Link to={ROUTES.login}>Girişe dön</Link>
          </p>
        </>
      )}
      {status === 'error' && (
        <>
          <p className="field-error" role="alert">
            {error}
          </p>
          <p className="auth-page__link">
            <Link to={ROUTES.login}>Girişe dön</Link>
          </p>
        </>
      )}
    </main>
  );
}
