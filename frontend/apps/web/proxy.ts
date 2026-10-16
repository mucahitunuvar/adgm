import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { getMaintenanceStatus } from './lib/maintenanceFlag';
import { renderMaintenancePageHtml } from './lib/maintenancePage';
import { mediaOrigin } from './lib/mediaOrigin';

const backendOrigin = process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';
const MAINTENANCE_RETRY_AFTER_SECONDS = 300;

// Bu önekler Next'in `rewrites()`'i (next.config.ts) aracılığıyla doğrudan backend'e gider; backend
// kendi bakım modu zorlamasını (503 + Retry-After) zaten uyguluyor (Faz 3 Koşu 1, Görev 3) - burada
// AYNI yolları ikinci kez engellemek, backend'in ProblemDetails/boş-gövde sözleşmesini bir düz HTML
// sayfasıyla ezer ve `packages/api-client`'ın hata ayrıştırmasını bozar. Yalnız gerçek SAYFA
// gezinmeleri (aşağıdaki hiçbirine uymayan yollar) bu middleware'in kendi bakım sayfasını görür.
const BACKEND_PROXIED_PREFIXES = ['/api/', '/sitemap', '/robots.txt', '/webuploads/', '/uploads/'];

function isBackendProxiedPath(pathname: string): boolean {
  return BACKEND_PROXIED_PREFIXES.some((prefix) => pathname === prefix.replace(/\/$/, '') || pathname.startsWith(prefix));
}

/**
 * FRONTEND-GUVENLIK.md §4: CSP (nonce ile), nosniff, referrer, permissions,
 * frame-ancestors, COOP. Üretimde HSTS eklenir. style-src 'unsafe-inline' bilinçli bir
 * ödünleşimdir: ProgressBar gibi bileşenler dinamik genişlik için satır içi `style` kullanır;
 * script-src'de nonce zorunludur, 'unsafe-inline' YOKTUR.
 */
export async function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  if (!isBackendProxiedPath(pathname)) {
    const maintenance = await getMaintenanceStatus(backendOrigin);
    if (maintenance.enabled) {
      return new NextResponse(
        renderMaintenancePageHtml(maintenance.message || 'Siteyi yakında tekrar burada bulacaksınız.'),
        {
          status: 503,
          headers: {
            'Content-Type': 'text/html; charset=utf-8',
            'Cache-Control': 'no-store',
            'Retry-After': String(MAINTENANCE_RETRY_AFTER_SECONDS),
          },
        },
      );
    }
  }

  const nonce = Buffer.from(crypto.randomUUID()).toString('base64');
  const isProd = process.env.NODE_ENV === 'production';

  const csp = [
    "default-src 'self'",
    `script-src 'self' 'nonce-${nonce}'${isProd ? '' : " 'unsafe-eval'"} https://challenges.cloudflare.com`,
    "style-src 'self' 'unsafe-inline'",
    // FRONTEND-ACIK-MADDELER.md madde 10 / SECURITY.md §23.3: video-feature bloğunun
    // `thumbnailUrl`'u backend tarafından i.ytimg.com'a hotlink ediliyor (canlı doğrulandı,
    // Koşu 2C) - CSP bunu açıkça izin vermeli, aksi halde küçük resim sessizce engellenir.
    `img-src 'self' data: ${mediaOrigin} https://i.ytimg.com`,
    "font-src 'self'",
    "connect-src 'self'",
    "frame-src https://challenges.cloudflare.com https://www.youtube-nocookie.com",
    "frame-ancestors 'none'",
    "base-uri 'self'",
    "form-action 'self'",
    "object-src 'none'",
  ].join('; ');

  const requestHeaders = new Headers(request.headers);
  requestHeaders.set('x-nonce', nonce);

  const response = NextResponse.next({ request: { headers: requestHeaders } });
  response.headers.set('Content-Security-Policy', csp);
  response.headers.set('X-Content-Type-Options', 'nosniff');
  response.headers.set('Referrer-Policy', 'strict-origin-when-cross-origin');
  response.headers.set('Permissions-Policy', 'camera=(), microphone=(), geolocation=()');
  response.headers.set('Cross-Origin-Opener-Policy', 'same-origin');
  if (isProd) {
    response.headers.set('Strict-Transport-Security', 'max-age=63072000; includeSubDomains');
  }
  return response;
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
};
