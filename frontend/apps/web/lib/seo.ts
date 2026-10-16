import type { Metadata } from 'next';
import type { PublicHomeResponse, RouteResolutionResponse } from '@gm/api-client';

/** `GET /public/home`'un hazır SEO bloğu - tüm alanlar zaten çözülmüş (ogImageUrl dahil). */
export function metadataFromHomeSeo(home: PublicHomeResponse): Metadata {
  const { seo } = home;
  return {
    title: seo.metaTitle || undefined,
    description: seo.metaDescription || undefined,
    alternates: { canonical: seo.canonicalUrl || '/' },
    robots: seo.noIndex ? { index: false, follow: false } : undefined,
    openGraph: {
      title: seo.ogTitle || seo.metaTitle || undefined,
      description: seo.ogDescription || seo.metaDescription || undefined,
      images: seo.ogImageUrl ? [{ url: seo.ogImageUrl }] : undefined,
      url: seo.canonicalUrl || '/',
    },
  };
}

/**
 * `routes/resolve`'un kısmi SEO bloğu (Detail/Listing) - ADR-024 §15: içeriğin kendi verisi bu
 * uçta dönmez, `ogImageMediaId` de hazır bir URL değildir. Tam SEO (Detail için `GET
 * /public/contents/{id}`'den) Koşu 3'ün işi; burada yalnız başlık/robots/canonical/hreflang var.
 */
export function metadataFromResolution(resolution: RouteResolutionResponse, path: string): Metadata {
  const seo = resolution.seo;
  return {
    title: seo?.metaTitle || resolution.name || undefined,
    description: seo?.metaDescription || undefined,
    alternates: {
      canonical: seo?.canonicalUrl || path,
      languages: buildHreflang(resolution.alternates),
    },
    robots: seo?.noIndex ? { index: false, follow: false } : undefined,
  };
}

/** K-12: tek aktif dilde (`tr`) hreflang alternatifi basılmaz. */
function buildHreflang(alternates: RouteResolutionResponse['alternates']): Record<string, string> | undefined {
  if (!alternates || alternates.length < 2) return undefined;
  const map: Record<string, string> = {};
  for (const alternate of alternates) {
    map[alternate.languageCode] = alternate.path;
  }
  return map;
}
