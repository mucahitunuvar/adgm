import { getAuthErrorMessage, useAuth } from '@gm/auth';
import { Button, PageHeader, PasswordField, TextField } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { ROUTES } from '../../routes';

interface LocationState {
  from?: string;
}

export function LoginPage() {
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const formId = useId();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const state = location.state as LocationState | null;
  const redirectTo = state?.from ?? ROUTES.dashboard;

  // Zaten oturum açıksa giriş sayfası gösterilmez.
  if (user) {
    return <Navigate to={redirectTo} replace />;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await login(email, password);
      navigate(redirectTo, { replace: true });
    } catch (loginError) {
      setError(getAuthErrorMessage(loginError));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="container section auth-page">
      <PageHeader title="Giriş yap" description="Yönetim paneline erişmek için e-posta ve parolanızla giriş yapın." />
      {state?.from && !error && (
        <p className="status status--info" role="status">
          Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.
        </p>
      )}
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
        <PasswordField
          id={`${formId}-password`}
          label="Parola"
          autoComplete="current-password"
          required
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />
        <Button type="submit" block disabled={isSubmitting}>
          {isSubmitting ? 'Giriş yapılıyor…' : 'Giriş yap'}
        </Button>
      </form>
      <p className="auth-page__link">
        <Link to={ROUTES.forgotPassword}>Parolamı unuttum</Link>
      </p>
    </main>
  );
}
