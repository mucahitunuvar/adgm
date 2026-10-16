import { getPublicHome } from '@gm/api-client';
import { createServerApiClient } from '../lib/apiClient';
import { getSite } from '../lib/getSite';
import { renderHomeBlocks } from '../components/blocks/BlockRegistry';
import { NewsletterSection } from '../components/NewsletterSection';
import { PopupHost } from '../components/PopupHost';

/** Ana sayfa - blok registry'si `GET /public/home`'un yayındaki bloklarını render eder (Koşu 2C). */
export async function HomePage() {
  const client = createServerApiClient();
  const [home, site] = await Promise.all([getPublicHome(client), getSite()]);

  if (home.blocks.length === 0) {
    return (
      <main className="container section">
        <p className="eyebrow">Ana sayfa</p>
        <p className="page-lead">Ana sayfa düzeni henüz yayınlanmamış.</p>
      </main>
    );
  }

  return (
    <main>
      {renderHomeBlocks(home.blocks, { publicJobListingsEnabled: site.publicJobListingsEnabled })}
      {site.newsletterEnabled && (
        <div className="container section">
          <NewsletterSection enabled={site.newsletterEnabled} />
        </div>
      )}
      <PopupHost popups={site.popups} />
    </main>
  );
}
