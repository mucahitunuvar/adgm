import { changePassword } from '@gm/api-client';
import { getAuthErrorMessage, useAuth } from '@gm/auth';
import { Button, PasswordField, useToast } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';

/**
 * Backend parola değiştirince mevcut erişim/refresh token'ları iptal etmiyor
 * (src/Modules/Identity/Features/ChangePassword/ChangePasswordCommandHandler.cs) — bu oturum
 * geçerliliğini sürdürür, bu yüzden başarıda yeniden girişe zorlamaya gerek yoktur.
 */
export function ChangePasswordForm() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const formId = useId();

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (newPassword !== confirmPassword) {
      setError('Parolalar eşleşmiyor.');
      return;
    }

    setIsSubmitting(true);
    try {
      await changePassword(client, currentPassword, newPassword);
      showToast('Parolanız güncellendi.', 'success');
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (submitError) {
      setError(getAuthErrorMessage(submitError));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} aria-busy={isSubmitting}>
      {error && (
        <p className="field-error" role="alert">
          {error}
        </p>
      )}
      <PasswordField
        id={`${formId}-current`}
        label="Mevcut parola"
        autoComplete="current-password"
        required
        value={currentPassword}
        onChange={(event) => setCurrentPassword(event.target.value)}
      />
      <PasswordField
        id={`${formId}-new`}
        label="Yeni parola"
        autoComplete="new-password"
        required
        minLength={8}
        hint="En az 8 karakter, en az bir harf ve bir rakam içermeli."
        value={newPassword}
        onChange={(event) => setNewPassword(event.target.value)}
      />
      <PasswordField
        id={`${formId}-confirm`}
        label="Yeni parola (tekrar)"
        autoComplete="new-password"
        required
        value={confirmPassword}
        onChange={(event) => setConfirmPassword(event.target.value)}
      />
      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Kaydediliyor…' : 'Parolayı güncelle'}
      </Button>
    </form>
  );
}
