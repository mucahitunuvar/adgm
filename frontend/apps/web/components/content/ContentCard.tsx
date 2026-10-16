import { safeUrl, formatDateTime } from '@gm/ui';
import { BlockImage } from '../blocks/BlockImage';
import type { PublicContentListItemResponse } from '@gm/api-client';

/** `cards`/`grid`/`list` şablonları için ortak kart - `HasDetailPage=false` türlerde `path=null` olur, bu durumda bağlantısız gösterilir. */
export function ContentCard({ item }: { item: PublicContentListItemResponse }) {
  const href = safeUrl(item.path);
  const image = item.coverImage ?? item.detailImage;
  const body = (
    <>
      {image && <BlockImage image={image} alt={item.title} sizes="(min-width: 900px) 33vw, 100vw" />}
      <h3>{item.title}</h3>
      <p className="content-card__date">{formatDateTime(item.effectivePublishDate)}</p>
      {item.summary && <p>{item.summary}</p>}
    </>
  );
  return href ? (
    <a href={href} className="content-card">
      {body}
    </a>
  ) : (
    <div className="content-card content-card--static">{body}</div>
  );
}
