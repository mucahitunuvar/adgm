import { Suspense } from 'react';
import type { Metadata } from 'next';
import { VerifyRegistrationClient } from '../../../../components/events/VerifyRegistrationClient';

export const metadata: Metadata = { robots: { index: false, follow: false } };

/**
 * ADR-024 §4.1: backend'in ürettiği `{PublicSiteBaseUrl}/events/registrations/verify?token=`
 * bağlantısı bu sabit (statik) yolla birebir eşleşir - yakalayıcı `[[...slug]]`'dan önce gelir.
 */
export default function VerifyEventRegistrationPage() {
  return (
    <Suspense>
      <VerifyRegistrationClient />
    </Suspense>
  );
}
