import { safeUrl } from '@gm/ui';
import { BlockImage } from './BlockImage';
import type { ContentListItem } from './ContentListBlock';

/**
 * `registrationState`/`remainingSpots` bu blokta YOK (BLOK-ESLEMESI.md tablo #8 - mimari gereği,
 * canlı çekilir). Koşu 3 kapsamı: etkinlik detay sayfası kayıt durumunu kendi çağrısıyla gösterir;
 * burada yalnız liste kartları var, kayıt rozeti yok.
 */
export function UpcomingEventsBlock({
  texts,
  data,
}: {
  texts: { title?: string; moreLabel?: string } | null;
  data: { items: ContentListItem[] } | null;
}) {
  if (!data || data.items.length === 0) return null;

  return (
    <section className="container section block-upcoming-events">
      {texts?.title && <h2>{texts.title}</h2>}
      <div className="block-content-list__grid">
        {data.items.map((item) => {
          const href = safeUrl(item.path);
          const image = item.coverImage ?? item.detailImage;
          const body = (
            <>
              {image && <BlockImage image={image} alt={item.title} sizes="(min-width: 900px) 33vw, 100vw" />}
              <h3>{item.title}</h3>
              <p>{item.summary}</p>
            </>
          );
          return (
            <div className="content-card" key={item.id}>
              {href ? <a href={href}>{body}</a> : body}
            </div>
          );
        })}
      </div>
    </section>
  );
}
