import { safeUrl } from '@gm/ui';

export interface BlockImageVariants {
  small?: string | null;
  medium?: string | null;
  large?: string | null;
  original: string;
}

/**
 * Backend'in her görsel referansında döndürdüğü hazır `{small,medium,large,original}` dörtlüsünden
 * `srcset` üretir (ADR-024 §6) - frontend kendi path'ini kurmaz. `images.unoptimized: true` olduğu
 * için düz `<img>`; CLS, sabit en-boy oranlı bir sarmalayıcıdan gelir (backend piksel boyutu vermez).
 *
 * `priority` (KOŞU 3F, LCP ölçümü): yalnız "katlama üstü" ilk görsel için `true` verilir (ana sayfa
 * hero slider'ının İLK slaytı, içerik detay sayfasının hero görseli) - `loading="lazy"` atlanır ve
 * `fetchPriority="high"` eklenir ki tarayıcı bu isteği erken keşfedip önceliklendirsin. Ölçüm (Lighthouse,
 * mobil, home/list/detail) LCP elemanının üçünde de bir METİN başlığı olduğunu gösterdi, görsel değil
 * (`lcp-discovery-insight` her sayfada "notApplicable") - yani bu bayrak şu an ölçülen LCP'yi doğrudan
 * değiştirmiyor, ama katlama üstü görseli geç keşfedilir/öncelik dışı bırakmamak genel bir LCP iyi
 * pratiğidir ve maliyeti yoktur (bkz. FRONTEND-ACIK-MADDELER.md'ye eklenen tam ölçüm notu).
 */
export function BlockImage({
  image,
  alt,
  sizes = '100vw',
  className = '',
  priority = false,
}: {
  image: BlockImageVariants;
  alt: string;
  sizes?: string;
  className?: string;
  priority?: boolean;
}) {
  const original = safeUrl(image.original);
  if (!original) return null;

  const srcSetParts: string[] = [];
  const small = safeUrl(image.small);
  const medium = safeUrl(image.medium);
  const large = safeUrl(image.large);
  if (small) srcSetParts.push(`${small} 400w`);
  if (medium) srcSetParts.push(`${medium} 800w`);
  if (large) srcSetParts.push(`${large} 1600w`);

  return (
    <img
      src={medium ?? original}
      srcSet={srcSetParts.length > 0 ? srcSetParts.join(', ') : undefined}
      sizes={srcSetParts.length > 0 ? sizes : undefined}
      alt={alt}
      loading={priority ? 'eager' : 'lazy'}
      fetchPriority={priority ? 'high' : undefined}
      className={className}
    />
  );
}
