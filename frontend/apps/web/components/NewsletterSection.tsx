'use client';

import { useState } from 'react';
import { createApiClient, ApiError } from '@gm/api-client';
import type { PublicSiteResponse } from '@gm/api-client';
import { useSubmissionToken } from '../lib/useSubmissionToken';

/**
 * Karar (BLOK-ESLEMESI.md tablo #12, madde 1): bloğun yerini `PageLayout` değil bu sabit bileşen
 * alır; `site.newsletterEnabled` kapalıyken hiç render edilmez. Hem footer'da hem (istenirse) ana
 * sayfa sonunda aynı bileşen kullanılır.
 *
 * `POST /public/newsletter/subscriptions`, kabul edilen aydınlatma metni sürümünü
 * (`acceptedPrivacyNoticeVersion`) ister (ADR-024 §14/§12.1); gerçek anahtar/sürüm artık
 * `site.newsletterPrivacyNotice`'tan gelir (madde 36). `privacyNotice` `null`sa (anahtar
 * `SiteSettings`'te hiç ayarlanmamış ya da belgenin yürürlükte sürümü yok) form UYDURMA bir sürüm
 * göndermez - tamamen kapalı, anlaşılır bir mesajla gösterilir.
 */
export function NewsletterSection({
  enabled,
  privacyNotice,
}: {
  enabled: boolean;
  privacyNotice: PublicSiteResponse['newsletterPrivacyNotice'];
}) {
  const [email, setEmail] = useState('');
  const [status, setStatus] = useState<'idle' | 'pending' | 'success' | 'error'>('idle');
  const submissionToken = useSubmissionToken(enabled && privacyNotice != null);

  if (!enabled) return null;

  if (!privacyNotice) {
    return (
      <div className="newsletter-section">
        <h2>E-Bülten</h2>
        <p className="page-lead">Bülten kaydı şu anda kullanılamıyor.</p>
      </div>
    );
  }

  async function onSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!privacyNotice) return;
    setStatus('pending');
    try {
      const client = createApiClient({ baseUrl: window.location.origin });
      await client.request('/api/v1/public/newsletter/subscriptions', {
        method: 'POST',
        body: {
          submissionToken: submissionToken ?? '',
          turnstileToken: '',
          website: '', // honeypot - gizli alan, her zaman boş gönderilir
          email,
          lang: 'tr',
          acceptedPrivacyNoticeVersion: privacyNotice.version,
        },
      });
      setStatus('success');
    } catch (error) {
      void (error instanceof ApiError ? error.title : error);
      setStatus('error');
    }
  }

  if (status === 'success') {
    return (
      <div className="newsletter-section">
        <p>Aboneliğinizi onaylamak için e-postanızı kontrol edin.</p>
      </div>
    );
  }

  return (
    <div className="newsletter-section">
      <h2>E-Bülten</h2>
      <p>Gelişmelerden haberdar olmak için e-posta bültenimize katılın.</p>
      <form onSubmit={onSubmit} className="newsletter-section__form">
        <label htmlFor="newsletter-email" className="sr-only">
          E-posta adresi
        </label>
        <input
          id="newsletter-email"
          type="email"
          required
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="ornek@eposta.com"
        />
        <button type="submit" className="button button--primary" disabled={status === 'pending'}>
          Abone Ol
        </button>
      </form>
      <p className="newsletter-section__notice">
        Kaydolarak <a href={`/legal/${privacyNotice.key}`}>aydınlatma metnini</a> kabul etmiş olursunuz.
      </p>
      {status === 'error' && <p className="field-error">Kaydınız şu anda tamamlanamadı, lütfen daha sonra tekrar deneyin.</p>}
    </div>
  );
}
