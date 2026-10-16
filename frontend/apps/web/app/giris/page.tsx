import type { Metadata } from 'next';
import { PortalChoiceCard } from '../../components/PortalChoiceCard';
import { CANDIDATE_PORTAL_URL, EMPLOYER_PORTAL_URL } from '../../lib/portalUrls';

export const metadata: Metadata = {
  title: 'Giriş Yap',
  robots: { index: false, follow: true },
};

export default function LoginPage() {
  return (
    <main className="container section">
      <p className="eyebrow">Giriş Yap</p>
      <h1>Hangi portala giriş yapmak istersiniz?</h1>
      <div className="portal-choice-grid">
        <PortalChoiceCard
          title="Aday Girişi"
          description="Profilinizi yönetin, ilanlara göz atın, görüşme davetlerinizi takip edin."
          href={CANDIDATE_PORTAL_URL}
          ctaLabel="Aday portalına git"
        />
        <PortalChoiceCard
          title="Firma Girişi"
          description="İlan yayınlayın, başvuruları ve görüşmeleri yönetin."
          href={EMPLOYER_PORTAL_URL}
          ctaLabel="Firma portalına git"
        />
      </div>
    </main>
  );
}
