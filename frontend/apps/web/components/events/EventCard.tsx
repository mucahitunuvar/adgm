import { safeUrl, formatDateTime } from '@gm/ui';
import { BlockImage } from '../blocks/BlockImage';
import type { PublicEventListItemResponse } from '@gm/api-client';
import { eventFormatLabel, registrationStateLabel } from './eventLabels';

export function EventCard({ item }: { item: PublicEventListItemResponse }) {
  const href = safeUrl(item.path);
  const body = (
    <>
      {item.coverImage && <BlockImage image={item.coverImage} alt={item.title} sizes="(min-width: 900px) 33vw, 100vw" />}
      <h3>{item.title}</h3>
      <p className="content-card__date">{formatDateTime(item.event.startsAtUtc)}</p>
      <p>{eventFormatLabel(item.event.format)}</p>
      <p className="event-card__state">{item.event.isCancelled ? 'İptal edildi' : registrationStateLabel(item.event.registrationState)}</p>
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
