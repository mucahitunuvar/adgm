import { getPublicHome } from '@gm/api-client';
import { createServerApiClient } from '../lib/apiClient';
import { getSite } from '../lib/getSite';
import { renderLayoutBlocks } from '../components/blocks/BlockRegistry';
import { NewsletterSection } from '../components/NewsletterSection';
import { PopupHost } from '../components/PopupHost';

/** Ana sayfa - blok registry'si `GET /public/home`'un yayındaki bloklarını render eder (Koşu 2C). */
export async function HomePage() {
  const client = createServerApiClient();
  const [home, site] = await Promise.all([getPublicHome(client), getSite()]);

  // Blok bileşimi neyin render edileceğini belirler (hero-slider varsa kendi h2'si görsel olarak
  // belirgin olur); sayfa düzeyinde TAM BİR `h1` garantisi (axe `page-has-heading-one`) blok
  // bileşiminden bağımsız olmalı - bu yüzden her durumda, görünmez, tek bir `h1` basılır.
  const pageHeading = <h1 className="sr-only">{site.siteName || 'Ahmet Demir Gençlik Merkezi'}</h1>;

  if (home.blocks.length === 0) {
    return (
      <div className="container section">
        <p className="eyebrow">Ana sayfa</p>
        {pageHeading}
        <p className="page-lead">Ana sayfa düzeni henüz yayınlanmamış.</p>
      </div>
    );
  }

  return (
    <>
      {pageHeading}
      {renderLayoutBlocks(home.blocks, { publicJobListingsEnabled: site.publicJobListingsEnabled })}
      {site.newsletterEnabled && (
        <div className="container section">
          <NewsletterSection enabled={site.newsletterEnabled} />
        </div>
      )}
      <PopupHost popups={site.popups} />
    </>
  );
}
