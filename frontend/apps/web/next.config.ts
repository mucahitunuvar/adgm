import type { NextConfig } from 'next';

const backendOrigin = process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';

const nextConfig: NextConfig = {
  output: 'standalone',
  images: {
    unoptimized: true,
  },
  // KOŞU 3G Görev 1 (LCP): önceki oturumların ölçtüğü darboğaz (~7 KB render-engelleyici `<link>` CSS,
  // Lighthouse'un mobil throttling modelinde tahmini 290-580 ms LCP maliyeti) için `critters` paketi
  // gerektiren `experimental.optimizeCss` yerine denendi - bu YENİ BİR BAĞIMLILIK GEREKTİRMİYOR
  // (Next'in kendi derleyicisi, yalnız üretim derlemesinde etkin), K-11'in "önce dur ve sor" kuralını
  // tetiklemeden denenebilir. Üretim derlemesine/Lighthouse'a (mobil) karşı ölçüldü - TUTULDU:
  // LCP ana sayfa ~1.75s→~1.32s, liste ~1.88s→~1.05s, detay ~1.81s→~1.10s (üçü de 2.5s bütçesinin
  // epey altında); toplam transfer ~12% arttı (CSS artık SSR HTML'de VE RSC yükünde iki kez taşınıyor
  // - Next'in kendi belgelediği ödünleşim), makul karşılandı. Tam karşılaştırma tablosu ve üretim
  // axe/e2e doğrulaması: FRONTEND-ACIK-MADDELER.md.
  experimental: {
    inlineCss: true,
  },
  // `next dev`'in kayan geliştirici araçları rozeti axe'ın renk kontrastı hesaplamasını bulanık bir
  // arka planla kirletiyordu (canlı doğrulandı: aynı metin/renk kombinasyonu her koşuda farklı,
  // tekrarlanamayan bir `bgColor` ile "yetersiz kontrast" raporlanıyordu - üretimde bu rozet hiç yok).
  devIndicators: false,
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
