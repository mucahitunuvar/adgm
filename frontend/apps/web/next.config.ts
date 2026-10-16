import type { NextConfig } from 'next';

const backendOrigin = process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';

const nextConfig: NextConfig = {
  output: 'standalone',
  images: {
    unoptimized: true,
  },
  transpilePackages: ['@gm/design-tokens', '@gm/ui', '@gm/api-client', '@gm/auth'],
  productionBrowserSourceMaps: false,
  async rewrites() {
    return [
      { source: '/api/:path*', destination: `${backendOrigin}/api/:path*` },
      { source: '/sitemap.xml', destination: `${backendOrigin}/sitemap.xml` },
      { source: '/sitemap-:segment.xml', destination: `${backendOrigin}/sitemap-:segment.xml` },
      { source: '/robots.txt', destination: `${backendOrigin}/robots.txt` },
      // ADR-024 §6 / FRONTEND-ACIK-MADDELER.md madde 16: backend'in gerçek public medya yolu
      // `/webuploads` (FileStorageSettings.PublicRequestPath) - Faz 0'ın varsayımsal `/uploads`
      // rewrite'ı da (backend'de ayrıca rezerve bir segment olduğu için, başka bir modülün kendi
      // dosya kökü olabilir) güvenle bırakılıyor, ikisi birbirini ezmiyor.
      { source: '/webuploads/:path*', destination: `${backendOrigin}/webuploads/:path*` },
      { source: '/uploads/:path*', destination: `${backendOrigin}/uploads/:path*` },
    ];
  },
};

export default nextConfig;
