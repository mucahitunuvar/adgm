import { Suspense } from 'react';
import type { Metadata } from 'next';
import { ConfirmSubscriptionClient } from '../../../components/newsletter/ConfirmSubscriptionClient';

export const metadata: Metadata = { robots: { index: false, follow: false } };

export default function NewsletterConfirmPage() {
  return (
    <Suspense>
      <ConfirmSubscriptionClient />
    </Suspense>
  );
}
