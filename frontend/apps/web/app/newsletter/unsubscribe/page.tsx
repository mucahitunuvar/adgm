import { Suspense } from 'react';
import type { Metadata } from 'next';
import { UnsubscribeClient } from '../../../components/newsletter/UnsubscribeClient';

export const metadata: Metadata = { robots: { index: false, follow: false } };

export default function NewsletterUnsubscribePage() {
  return (
    <Suspense>
      <UnsubscribeClient />
    </Suspense>
  );
}
