import type { Metadata } from 'next';
import { PortalChoiceCard } from '../../components/PortalChoiceCard';
import { CANDIDATE_PORTAL_URL, EMPLOYER_PORTAL_URL } from '../../lib/portalUrls';

export const metadata: Metadata = {
  title: 'Üye Ol',
  robots: { index: false, follow: true },
};

export default function RegisterPage() {
  return (
    <div className="container section">
      <p className="eyebrow">Üye Ol</p>
      <h1>Hangi hesap türüyle üye olmak istersiniz?</h1>
      <div className="portal-choice-grid">
        <PortalChoiceCard
          title="Aday Olarak Üye Ol"
          description="Ücretsiz profil oluşturun, kariyer danışmanlığından faydalanın."
          href={CANDIDATE_PORTAL_URL}
          ctaLabel="Aday portalına git"
        />
        <PortalChoiceCard
          title="Firma Olarak Üye Ol"
          description="Şirketinizi tanıtın, ilan yayınlayın."
          href={EMPLOYER_PORTAL_URL}
          ctaLabel="Firma portalına git"
        />
      </div>
    </div>
  );
}
