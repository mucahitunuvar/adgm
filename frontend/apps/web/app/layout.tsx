import { headers } from 'next/headers';
import type { ReactNode } from 'react';
import type { Metadata } from 'next';
import { themeInitScript } from '@gm/design-tokens';
import { JsonLd } from '@gm/ui';
import '@fontsource/manrope/latin-ext-400.css';
import '@fontsource/manrope/latin-ext-600.css';
import '@fontsource/manrope/latin-ext-700.css';
// KOŞU 3F (LCP): latin-ext-800 kaldırıldı - hiçbir kaynak CSS kuralı `font-weight: 800` kullanmıyor
// (grep ile doğrulandı); gereksiz bir @font-face bildirimi render-blocking CSS'e boşuna bayt katıyordu.
import '@fontsource/poppins/latin-ext-600.css';
import '@fontsource/poppins/latin-ext-700.css';
import '@gm/design-tokens/tokens.css';
import '@gm/design-tokens/base.css';
import './globals.css';
import { Header } from '../components/Header';
import { Footer } from '../components/Footer';
import { CookieConsentBanner } from '../components/CookieConsentBanner';
import { getSite } from '../lib/getSite';

// KOŞU 3F: `metadataBase` olmadan Next, `alternates.canonical`/`openGraph.url` için verilen
// kök-göreli yolu (`seo.ts`'teki `normalizeCanonical`'ın ürettiği `/haberler` gibi) OLDUĞU GİBİ basar
// - üretim derlemesine karşı doğrulandı, `<link rel="canonical">` mutlak bir URL değil. `metadataBase`
// verilince Next tüm sayfalarda bunu otomatik mutlak hale getirir (tek yerde çözülür, her
// `metadataFrom*` fonksiyonu tek tek değişmez).
const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? 'http://localhost:3000';

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: 'Ahmet Demir Gençlik Merkezi',
  description: 'Gençlik merkezi derneği kurumsal sitesi ve istihdam destek portalı.',
};

export default async function RootLayout({ children }: { children: ReactNode }) {
  const nonce = (await headers()).get('x-nonce') ?? undefined;
  const site = await getSite();

  return (
    <html lang="tr">
      <head>
        <script nonce={nonce}>{themeInitScript}</script>
        {/* ADR-024 §15: backend'in hazır döndürdüğü Organization şeması. */}
        {Object.keys(site.organization).length > 0 && <JsonLd data={site.organization} nonce={nonce} />}
      </head>
      <body>
        <nav aria-label="Atla bağlantısı">
          <a href="#main-content" className="skip-link">
            İçeriğe atla
          </a>
        </nav>
        <Header site={site} />
        <main id="main-content">{children}</main>
        <Footer site={site} />
        <CookieConsentBanner cookieConsent={site.cookieConsent} />
      </body>
    </html>
  );
}
