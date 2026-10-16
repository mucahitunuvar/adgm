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
 */
export function BlockImage({
  image,
  alt,
  sizes = '100vw',
  className = '',
}: {
  image: BlockImageVariants;
  alt: string;
  sizes?: string;
  className?: string;
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
      loading="lazy"
      className={className}
    />
  );
}
