import { Suspense } from 'react';
import type { Metadata } from 'next';
import { CancelRegistrationClient } from '../../../../components/events/CancelRegistrationClient';

export const metadata: Metadata = { robots: { index: false, follow: false } };

export default function CancelEventRegistrationPage() {
  return (
    <Suspense>
      <CancelRegistrationClient />
    </Suspense>
  );
}
