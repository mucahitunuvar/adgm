import { safeUrl, SafeHtml } from '@gm/ui';
import { BlockImage } from './BlockImage';

interface ImageTextData {
  image: { small?: string | null; medium?: string | null; large?: string | null; original: string } | null;
  href: string | null;
}

export function ImageTextBlock({
  settings,
  texts,
  data,
}: {
  settings: { imagePosition?: string } | null;
  texts: { title?: string; body?: string; linkLabel?: string } | null;
  data: ImageTextData | null;
}) {
  if (!texts?.title && !texts?.body) return null;
  const href = safeUrl(data?.href);
  const imageOnRight = settings?.imagePosition === 'right';

  return (
    <section className="container section block-image-text">
      <div className={`block-image-text__grid${imageOnRight ? ' block-image-text__grid--reverse' : ''}`}>
        {data?.image && <BlockImage image={data.image} alt={texts?.title ?? ''} className="block-image-text__image" />}
        <div>
          {texts?.title && <h2>{texts.title}</h2>}
          {texts?.body && <SafeHtml html={texts.body} />}
          {href && texts?.linkLabel && (
            <a className="button button--secondary" href={href}>
              {texts.linkLabel}
            </a>
          )}
        </div>
      </div>
    </section>
  );
}
