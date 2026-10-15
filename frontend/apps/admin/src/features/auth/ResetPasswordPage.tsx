import { resetPassword } from '@gm/api-client';
import { getAuthErrorMessage, useAuth } from '@gm/auth';
import { Button, PageHeader, PasswordField } from '@gm/ui';
import { useEffect, useId, useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { ROUTES } from '../../routes';

/**
 * Token URL'den bir kez okunup hemen adres çubuğundan silinir (`history.replaceState`);
 * log'a/analitiğe/hata ekranına yazılmaz (FRONTEND-GUVENLIK.md §6).
 */
export function ResetPasswordPage() {
  const { client } = useAuth();
  const [searchParams] = useSearchParams();
  const [token] = useState(() => searchParams.get('token') ?? '');
  const formId = useId();

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDone, setIsDone] = useState(false);

  useEffect(() => {
    if (window.location.search) {
      window.history.replaceState(null, '', window.location.pathname);
    }
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (!token) {
      setError('Sıfırlama bağlantısı geçersiz. Lütfen e-postanızdaki bağlantıyı yeniden kullanın.');
      return;
    }
    if (newPassword !== confirmPassword) {
      setError('Parolalar eşleşmiyor.');
      return;
    }

    setIsSubmitting(true);
    try {
      await resetPassword(client, token, newPassword);
      setIsDone(true);
    } catch (submitError) {
      setError(getAuthErrorMessage(submitError));
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isDone) {
    return (
      <main className="container section auth-page">
        <PageHeader title="Parola sıfırlandı" />
        <p className="status status--success" role="status">
          Parolanız güncellendi. Yeni parolanızla giriş yapabilirsiniz.
        </p>
        <p className="auth-page__link">
          <Link to={ROUTES.login}>Girişe dön</Link>
        </p>
      </main>
    );
  }

  return (
    <main className="container section auth-page">
      <PageHeader title="Parolanı sıfırla" description="Yeni parolanızı belirleyin." />
      {error && (
        <p className="field-error" role="alert">
          {error}
        </p>
      )}
      <form onSubmit={handleSubmit} aria-busy={isSubmitting}>
        <PasswordField
          id={`${formId}-new-password`}
          label="Yeni parola"
          autoComplete="new-password"
          required
          minLength={8}
          hint="En az 8 karakter, en az bir harf ve bir rakam içermeli."
          value={newPassword}
          onChange={(event) => setNewPassword(event.target.value)}
        />
        <PasswordField
          id={`${formId}-confirm-password`}
          label="Yeni parola (tekrar)"
          autoComplete="new-password"
          required
          value={confirmPassword}
          onChange={(event) => setConfirmPassword(event.target.value)}
        />
        <Button type="submit" block disabled={isSubmitting}>
          {isSubmitting ? 'Kaydediliyor…' : 'Parolayı sıfırla'}
        </Button>
      </form>
    </main>
  );
}
