'use client';

import { useState } from 'react';
import { createApiClient, ApiError } from '@gm/api-client';

/**
 * Karar (BLOK-ESLEMESI.md tablo #12, madde 1): bloğun yerini `PageLayout` değil bu sabit bileşen
 * alır; `site.newsletterEnabled` kapalıyken hiç render edilmez. Hem footer'da hem (istenirse) ana
 * sayfa sonunda aynı bileşen kullanılır.
 *
 * BİLİNEN AÇIK NOKTA: `POST /public/newsletter/subscriptions`, kabul edilen aydınlatma metni
 * sürümünü (`acceptedPrivacyNoticeVersion`) ister (ADR-024 §14/§12.1) ama `GET /public/site` hangi
 * `LegalDocument` anahtarının bültene bağlı olduğunu HİÇ döndürmüyor - frontend bu anahtarı/sürümü
 * keşfedemiyor. Bu form, sürüm uyuşmazlığında backend'in zaten güvenli döndüğü 409'u genel bir hata
 * olarak gösterir; gerçek çözüm backend'in `site`/ayrı bir uca bu anahtarı eklemesidir (rapora
 * taşındı, burada tahmini bir anahtar UYDURULMADI - yalnız sürüm 1 varsayımıyla denenir).
 */
export function NewsletterSection({ enabled }: { enabled: boolean }) {
  const [email, setEmail] = useState('');
  const [status, setStatus] = useState<'idle' | 'pending' | 'success' | 'error'>('idle');

  if (!enabled) return null;

  async function onSubmit(event: React.FormEvent) {
    event.preventDefault();
    setStatus('pending');
    try {
      const client = createApiClient({ baseUrl: window.location.origin });
      const tokenResponse = await client.request<{ token: string }>('/api/v1/public/submission-tokens');
      await client.request('/api/v1/public/newsletter/subscriptions', {
        method: 'POST',
        body: {
          submissionToken: tokenResponse.token,
          turnstileToken: '',
          website: '', // honeypot - gizli alan, her zaman boş gönderilir
          email,
          lang: 'tr',
          acceptedPrivacyNoticeVersion: 1,
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
      {status === 'error' && <p className="field-error">Kaydınız şu anda tamamlanamadı, lütfen daha sonra tekrar deneyin.</p>}
    </div>
  );
}
