import { forgotPassword } from '@gm/api-client';
import { getAuthErrorMessage, useAuth } from '@gm/auth';
import { Button, PageHeader, TextField } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../routes';

/** "E-posta kayıtlıysa gönderildi" sabit mesajı: hesap var/yok bilgisini sızdırmaz (anti-enumeration). */
export function ForgotPasswordPage() {
  const { client } = useAuth();
  const formId = useId();

  const [email, setEmail] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDone, setIsDone] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await forgotPassword(client, email);
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
        <PageHeader title="Parolamı unuttum" />
        <p className="status status--info" role="status">
          E-posta adresiniz kayıtlıysa parola sıfırlama bağlantısı gönderildi.
        </p>
        <p className="auth-page__link">
          <Link to={ROUTES.login}>Girişe dön</Link>
        </p>
      </main>
    );
  }

  return (
    <main className="container section auth-page">
      <PageHeader title="Parolamı unuttum" description="E-posta adresinizi girin; kayıtlıysa bir sıfırlama bağlantısı göndereceğiz." />
      {error && (
        <p className="field-error" role="alert">
          {error}
        </p>
      )}
      <form onSubmit={handleSubmit} aria-busy={isSubmitting}>
        <TextField
          id={`${formId}-email`}
          label="E-posta"
          type="email"
          autoComplete="username"
          required
          value={email}
          onChange={(event) => setEmail(event.target.value)}
        />
        <Button type="submit" block disabled={isSubmitting}>
          {isSubmitting ? 'Gönderiliyor…' : 'Sıfırlama bağlantısı gönder'}
        </Button>
      </form>
      <p className="auth-page__link">
        <Link to={ROUTES.login}>Girişe dön</Link>
      </p>
    </main>
  );
}
