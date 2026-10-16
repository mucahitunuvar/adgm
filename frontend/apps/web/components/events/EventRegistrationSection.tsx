'use client';

import { useEffect, useState, type FormEvent } from 'react';
import { createApiClient, ApiError, getApiErrorMessage } from '@gm/api-client';
import {
  getPublicContentById,
  createEventRegistration,
  resendEventRegistrationVerification,
  createSubmissionToken,
  publicEventCalendarUrl,
  EventRegistrationState,
} from '@gm/api-client';
import type { PublicContentDetailEventResponse } from '@gm/api-client';
import { TextField } from '@gm/ui';
import { HoneypotField } from '../forms/HoneypotField';
import { TurnstileWidget } from '../forms/TurnstileWidget';
import { registrationStateLabel, canRegister } from './eventLabels';

type Phase = 'idle' | 'submitting' | 'pending-verification' | 'success';

/**
 * Kayıt durumu tarayıcıda canlı çekilir (K-07, ADR-024 §11.3): SSR anlık görüntüsü (`initialEvent`,
 * içerik detayı önbelleğinden ~60 sn eski olabilir) yalnız ilk boyama için kullanılır; bağlanır
 * bağlanmaz aynı içerik ucuna tarayıcı kaynaklı (Next sunucu önbelleğini atlayan) bir istekle
 * güncel durum çekilir. Tek bir etkinliğin canlı durumunu döndüren ayrı bir uç yok; bu yüzden
 * aynı `contents/{id}` ucu istemciden yeniden çağrılır.
 */
export function EventRegistrationSection({
  contentItemId,
  initialEvent,
  turnstileSiteKey,
}: {
  contentItemId: string;
  initialEvent: PublicContentDetailEventResponse;
  turnstileSiteKey: string;
}) {
  const [event, setEvent] = useState(initialEvent);
  const [phase, setPhase] = useState<Phase>('idle');
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null);
  const [submittedEmail, setSubmittedEmail] = useState('');

  useEffect(() => {
    let cancelled = false;
    const client = createApiClient({ baseUrl: window.location.origin });
    getPublicContentById(client, contentItemId)
      .then((detail) => {
        if (!cancelled && detail.event) setEvent(detail.event);
      })
      .catch(() => {
        // Canlı durum alınamazsa SSR anlık görüntüsü gösterilmeye devam eder; sessizce yok sayılır.
      });
    return () => {
      cancelled = true;
    };
  }, [contentItemId]);

  async function handleSubmit(formEvent: FormEvent<HTMLFormElement>) {
    formEvent.preventDefault();
    setError(null);
    setFieldErrors({});
    setPhase('submitting');

    const form = new FormData(formEvent.currentTarget);
    const email = String(form.get('email') ?? '');

    try {
      const client = createApiClient({ baseUrl: window.location.origin });
      const { token: submissionToken } = await createSubmissionToken(client);
      await createEventRegistration(client, contentItemId, {
        submissionToken,
        turnstileToken,
        website: String(form.get('website') ?? ''),
        firstName: String(form.get('firstName') ?? ''),
        lastName: String(form.get('lastName') ?? ''),
        email,
        phone: String(form.get('phone') ?? ''),
        lang: 'tr',
        // Bilinen backend boşluğu: `PublicSiteResponse`/`PublicContentDetailEventResponse` hangi
        // `LegalDocument` anahtarının/sürümünün etkinlik kaydına bağlı olduğunu döndürmüyor (A6/A36
        // ile aynı sınıf eksiklik, bkz. FRONTEND-ACIK-MADDELER.md). `NewsletterSection.tsx` ile aynı
        // geçici varsayım izlenir; backend sürümü değiştirirse 409 (`LegalVersionChanged`) döner.
        acceptedPrivacyNoticeVersion: 1,
      });
      setSubmittedEmail(email);
      setPhase('pending-verification');
    } catch (err) {
      if (err instanceof ApiError && err.status === 400 && err.errors) {
        const mapped: Record<string, string> = {};
        for (const [key, messages] of Object.entries(err.errors)) {
          mapped[key.charAt(0).toLowerCase() + key.slice(1)] = messages[0] ?? '';
        }
        setFieldErrors(mapped);
      }
      setError(getApiErrorMessage(err));
      setPhase('idle');
    }
  }

  async function handleResend() {
    setError(null);
    try {
      const client = createApiClient({ baseUrl: window.location.origin });
      const { token: submissionToken } = await createSubmissionToken(client);
      await resendEventRegistrationVerification(client, {
        contentItemId,
        submissionToken,
        turnstileToken,
        website: '',
        email: submittedEmail,
      });
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  }

  const calendarHref = publicEventCalendarUrl(contentItemId, 'tr');

  return (
    <section className="event-registration" aria-labelledby="event-registration-heading">
      <h2 id="event-registration-heading">Kayıt</h2>
      <p className="event-registration__state">
        {registrationStateLabel(event.registrationState)}
        {event.remainingSpots != null && event.registrationState !== EventRegistrationState.Full && (
          <> · Kalan yer: {event.remainingSpots}</>
        )}
      </p>
      <a className="button button--quiet" href={calendarHref} download>
        Takvime ekle (.ics)
      </a>

      {phase === 'pending-verification' ? (
        <div className="event-registration__pending">
          <p>Kaydınızı tamamlamak için <strong>{submittedEmail}</strong> adresine gönderilen bağlantıya tıklayın.</p>
          <button type="button" className="button button--quiet" onClick={handleResend}>
            Doğrulama e-postasını yeniden gönder
          </button>
        </div>
      ) : canRegister(event.registrationState) && !event.isCancelled ? (
        <form onSubmit={handleSubmit} className="event-registration__form" noValidate>
          <HoneypotField />
          <TextField id="event-firstName" name="firstName" label="Ad" required error={fieldErrors.firstName} />
          <TextField id="event-lastName" name="lastName" label="Soyad" required error={fieldErrors.lastName} />
          <TextField id="event-email" name="email" type="email" label="E-posta" required error={fieldErrors.email} />
          <TextField id="event-phone" name="phone" type="tel" label="Telefon" error={fieldErrors.phone} />
          {turnstileSiteKey && <TurnstileWidget siteKey={turnstileSiteKey} onToken={setTurnstileToken} />}
          {error && (
            <p className="form-error" role="alert">
              {error}
            </p>
          )}
          <button type="submit" className="button button--primary" disabled={phase === 'submitting'}>
            {phase === 'submitting' ? 'Gönderiliyor…' : 'Kayıt ol'}
          </button>
        </form>
      ) : (
        !event.isCancelled && <p className="page-lead">Bu etkinlik için şu anda kayıt alınmıyor.</p>
      )}
    </section>
  );
}
