import type { Metadata } from 'next';
import type {
  PublicContentDetailResponse,
  PublicContentListResponse,
  PublicHomeResponse,
  RouteResolutionResponse,
} from '@gm/api-client';

// Next.js ebeveyn (root layout) metadata'sından DEVRALMAK için bir alanı tamamen ATLAMAK gerekir -
// `{ title: undefined }` döndürmek ebeveynin değerini `undefined` ile EZER (basit obje birleştirme),
// "sağlanmadı" anlamına gelmez. Bu yüzden boş değerler burada koşullu yayılım (spread) ile hiç
// eklenmez, `|| undefined` ile "boş" bırakılmaz (canlı doğrulandı: ikincisi `<title>` etiketini
// tamamen kaybettiriyordu - axe `document-title` ihlali).
/**
 * Üretim derlemesine karşı doğrulandı (Faz 3 Koşu 3E, 16 Ekim): liste SEO'sunda `canonicalUrl`
 * backend'den açık override yoksa `ContentType.RoutePrefix`'e düşüyor ve bu alan başında `/`
 * TAŞIMIYOR (ör. `"haberler"`) - olduğu gibi basılırsa Next bunu geçerli yoldan RELATİF sayıp
 * `<link rel="canonical" href="haberler">` (kök-göreli değil) üretiyor; canlı `next build && next
 * start`'a karşı `curl` ile görüldü, mutlak URL ile de başlamıyor. Bu yüzden her yerde tek bir
 * normalizasyon noktasından geçirilir; backend sözleşmesi değiştirilmedi.
 */
function normalizeCanonical(value: string | null | undefined, fallbackPath: string): string {
  const trimmed = value?.trim();
  if (!trimmed) return fallbackPath;
  if (trimmed.startsWith('/') || /^https?:\/\//i.test(trimmed)) return trimmed;
  return `/${trimmed}`;
}

export function metadataFromHomeSeo(home: PublicHomeResponse): Metadata {
  const { seo } = home;
  const ogTitle = seo.ogTitle || seo.metaTitle;
  const ogDescription = seo.ogDescription || seo.metaDescription;
  const canonical = normalizeCanonical(seo.canonicalUrl, '/');

  return {
    ...(seo.metaTitle && { title: seo.metaTitle }),
    ...(seo.metaDescription && { description: seo.metaDescription }),
    alternates: { canonical },
    ...(seo.noIndex && { robots: { index: false, follow: false } }),
    openGraph: {
      ...(ogTitle && { title: ogTitle }),
      ...(ogDescription && { description: ogDescription }),
      ...(seo.ogImageUrl && { images: [{ url: seo.ogImageUrl }] }),
      url: canonical,
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
  const title = seo?.metaTitle || resolution.name;
  const languages = buildHreflang(resolution.alternates);

  return {
    ...(title && { title }),
    ...(seo?.metaDescription && { description: seo.metaDescription }),
    alternates: {
      canonical: normalizeCanonical(seo?.canonicalUrl, path),
      ...(languages && { languages }),
    },
    ...(seo?.noIndex && { robots: { index: false, follow: false } }),
  };
}

/** K-12: tek aktif dilde (`tr`) hreflang alternatifi basılmaz. */
function buildHreflang(alternates: readonly { languageCode: string; path: string }[] | null | undefined): Record<string, string> | undefined {
  if (!alternates || alternates.length < 2) return undefined;
  const map: Record<string, string> = {};
  for (const alternate of alternates) {
    map[alternate.languageCode] = alternate.path;
  }
  return map;
}

/** `GET /public/contents/{id}` zaten tam çözülmüş SEO alanlarını döner (ADR-024 §15 `ContentSeoResolver`). */
export function metadataFromContentDetail(detail: PublicContentDetailResponse): Metadata {
  const { seo } = detail;
  const ogTitle = seo.ogTitle || seo.metaTitle || detail.title;
  const ogDescription = seo.ogDescription || seo.metaDescription || detail.summary;
  const languages = buildHreflang(detail.alternates);
  const canonical = normalizeCanonical(seo.canonicalUrl, detail.path);

  return {
    title: seo.metaTitle || detail.title,
    ...(seo.metaDescription && { description: seo.metaDescription }),
    alternates: {
      canonical,
      ...(languages && { languages }),
    },
    ...(seo.noIndex && { robots: { index: false, follow: false } }),
    openGraph: {
      title: ogTitle,
      ...(ogDescription && { description: ogDescription }),
      ...(seo.ogImageUrl && { images: [{ url: seo.ogImageUrl }] }),
      url: canonical,
    },
  };
}

export function metadataFromContentList(list: PublicContentListResponse, path: string): Metadata {
  const { seo } = list;
  const languages = buildHreflang(list.alternates);
  const canonical = normalizeCanonical(seo.canonicalUrl, path);

  return {
    ...(seo.metaTitle && { title: seo.metaTitle }),
    ...(seo.metaDescription && { description: seo.metaDescription }),
    alternates: {
      canonical,
      ...(languages && { languages }),
    },
    ...(seo.noIndex && { robots: { index: false, follow: false } }),
    openGraph: {
      ...(seo.ogTitle || seo.metaTitle ? { title: seo.ogTitle || seo.metaTitle } : {}),
      ...(seo.ogDescription || seo.metaDescription ? { description: seo.ogDescription || seo.metaDescription } : {}),
      ...(seo.ogImageUrl && { images: [{ url: seo.ogImageUrl }] }),
      url: canonical,
    },
  };
}
