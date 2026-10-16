import { safeUrl } from '@gm/ui';
import { BlockImage, type BlockImageVariants } from './BlockImage';
import { Carousel } from './Carousel';

export interface ContentListItem {
  id: string;
  title: string;
  summary: string;
  path?: string | null;
  coverImage?: BlockImageVariants | null;
  body?: string | null;
  detailImage?: BlockImageVariants | null;
}

interface ContentListData {
  items: ContentListItem[];
  moreHref?: string | null;
}

function ItemCard({ item }: { item: ContentListItem }) {
  const href = safeUrl(item.path);
  const image = item.coverImage ?? item.detailImage;
  const body = (
    <>
      {image && <BlockImage image={image} alt={item.title} sizes="(min-width: 900px) 33vw, 100vw" />}
      <h3>{item.title}</h3>
      <p>{item.summary}</p>
    </>
  );
  return href ? <a href={href} className="content-card">{body}</a> : <div className="content-card">{body}</div>;
}

/** Karar özeti: haberler "1 büyük + 2 küçük" - backend'in `view` ayarında bir karşılığı yok, sıralı `items` dizisinin düzeni salt frontend seçimi. */
function NewsAsymmetricLayout({ items }: { items: ContentListItem[] }) {
  const [first, ...rest] = items;
  if (!first) return null;
  return (
    <div className="block-content-list__news-grid">
      <ItemCard item={first} />
      <div className="block-content-list__news-secondary">
        {rest.slice(0, 2).map((item) => (
          <ItemCard key={item.id} item={item} />
        ))}
      </div>
    </div>
  );
}

/** Karar (madde 3): videosuz - görsel + metin, slider. */
function SuccessStorySlider({ items }: { items: ContentListItem[] }) {
  const slides = items.map((item) => {
    const href = safeUrl(item.path);
    const image = item.coverImage ?? item.detailImage;
    const content = (
      <div className="success-story-slide__text">
        <h3>{item.title}</h3>
        {item.summary && <p>{item.summary}</p>}
      </div>
    );
    return (
      <div className="success-story-slide" key={item.id}>
        {image && <BlockImage image={image} alt={item.title} sizes="(min-width: 900px) 50vw, 100vw" />}
        {href ? <a href={href}>{content}</a> : content}
      </div>
    );
  });
  return <Carousel slides={slides} ariaLabel="Başarı hikayeleri" />;
}

function DefaultCardGrid({ items }: { items: ContentListItem[] }) {
  return (
    <div className="block-content-list__grid">
      {items.map((item) => (
        <ItemCard key={item.id} item={item} />
      ))}
    </div>
  );
}

export function ContentListBlock({
  settings,
  texts,
  data,
}: {
  settings: { contentTypeKey?: string } | null;
  texts: { title?: string; moreLabel?: string } | null;
  data: ContentListData | null;
}) {
  if (!data || data.items.length === 0) return null;
  const moreHref = safeUrl(data.moreHref);

  return (
    <section className="container section block-content-list">
      <div className="block-content-list__header">
        {texts?.title && <h2>{texts.title}</h2>}
        {moreHref && texts?.moreLabel && (
          <a className="button button--quiet" href={moreHref}>
            {texts.moreLabel}
          </a>
        )}
      </div>
      {settings?.contentTypeKey === 'success-story' ? (
        <SuccessStorySlider items={data.items} />
      ) : settings?.contentTypeKey === 'news' ? (
        <NewsAsymmetricLayout items={data.items} />
      ) : (
        <DefaultCardGrid items={data.items} />
      )}
    </section>
  );
}
