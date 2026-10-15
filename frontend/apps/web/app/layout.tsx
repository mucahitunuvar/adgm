import { headers } from 'next/headers';
import type { ReactNode } from 'react';
import type { Metadata } from 'next';
import { themeInitScript } from '@gm/design-tokens';
import '@fontsource/manrope/latin-ext-400.css';
import '@fontsource/manrope/latin-ext-600.css';
import '@fontsource/manrope/latin-ext-700.css';
import '@fontsource/manrope/latin-ext-800.css';
import '@fontsource/poppins/latin-ext-600.css';
import '@fontsource/poppins/latin-ext-700.css';
import '@gm/design-tokens/tokens.css';
import '@gm/design-tokens/base.css';
import './globals.css';

export const metadata: Metadata = {
  title: 'Ahmet Demir Gençlik Merkezi',
  description: 'Gençlik merkezi derneği kurumsal sitesi ve istihdam destek portalı.',
};

export default async function RootLayout({ children }: { children: ReactNode }) {
  const nonce = (await headers()).get('x-nonce') ?? undefined;
  return (
    <html lang="tr">
      <head>
        <script nonce={nonce}>{themeInitScript}</script>
      </head>
      <body>{children}</body>
    </html>
  );
}
