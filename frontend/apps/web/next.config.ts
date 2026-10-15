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
      { source: '/robots.txt', destination: `${backendOrigin}/robots.txt` },
      { source: '/uploads/:path*', destination: `${backendOrigin}/uploads/:path*` },
    ];
  },
};

export default nextConfig;
