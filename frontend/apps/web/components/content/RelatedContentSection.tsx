import { safeUrl } from '@gm/ui';
import type { PublicContentDetailResponse } from '@gm/api-client';

export function RelatedContentSection({ items }: { items: PublicContentDetailResponse['related'] }) {
  if (items.length === 0) return null;
  return (
    <section className="content-detail__section">
      <h2>İlgili içerikler</h2>
      <div className="block-content-list__grid">
        {items.map((item) => {
          const href = safeUrl(item.path);
          const image = safeUrl(item.coverImageUrl);
          const body = (
            <>
              {image && <img src={image} alt={item.title} loading="lazy" />}
              <h3>{item.title}</h3>
              <p>{item.summary}</p>
            </>
          );
          return href ? (
            <a key={item.id} href={href} className="content-card">
              {body}
            </a>
          ) : (
            <div key={item.id} className="content-card content-card--static">
              {body}
            </div>
          );
        })}
      </div>
    </section>
  );
}
