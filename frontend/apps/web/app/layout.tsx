import { headers } from 'next/headers';
import type { ReactNode } from 'react';
import type { Metadata } from 'next';
import { themeInitScript } from '@gm/design-tokens';
import { JsonLd } from '@gm/ui';
import '@fontsource/manrope/latin-ext-400.css';
import '@fontsource/manrope/latin-ext-600.css';
import '@fontsource/manrope/latin-ext-700.css';
import '@fontsource/manrope/latin-ext-800.css';
import '@fontsource/poppins/latin-ext-600.css';
import '@fontsource/poppins/latin-ext-700.css';
import '@gm/design-tokens/tokens.css';
import '@gm/design-tokens/base.css';
import './globals.css';
import { Header } from '../components/Header';
import { Footer } from '../components/Footer';
import { CookieConsentBanner } from '../components/CookieConsentBanner';
import { getSite } from '../lib/getSite';

export const metadata: Metadata = {
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
        <a href="#main-content" className="skip-link">
          İçeriğe atla
        </a>
        <Header site={site} />
        <main id="main-content">{children}</main>
        <Footer site={site} />
        <CookieConsentBanner cookieConsent={site.cookieConsent} />
      </body>
    </html>
  );
}
