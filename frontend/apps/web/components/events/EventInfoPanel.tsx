import { SafeHtml, formatDateTime } from '@gm/ui';
import { EventFormat } from '@gm/api-client';
import type { PublicContentDetailEventResponse } from '@gm/api-client';
import { eventFormatLabel } from './eventLabels';

/** `OnlineLink` hiçbir public yanıtta yer almaz (ADR-024 §11.1/§11.3) - burada da basılmaz. */
export function EventInfoPanel({ event }: { event: PublicContentDetailEventResponse }) {
  const showVenue = event.format !== EventFormat.Online && (event.venueName || event.venueAddress);

  return (
    <aside className="event-info-panel">
      {event.isCancelled && <p className="event-info-panel__cancelled">Bu etkinlik iptal edilmiştir.</p>}
      <dl>
        <div>
          <dt>Tarih</dt>
          <dd>
            {formatDateTime(event.startsAtUtc)} – {formatDateTime(event.endsAtUtc)}
          </dd>
        </div>
        <div>
          <dt>Format</dt>
          <dd>{eventFormatLabel(event.format)}</dd>
        </div>
        {showVenue && (
          <div>
            <dt>Yer</dt>
            <dd>
              {event.venueName}
              {event.venueAddress && <> · {event.venueAddress}</>}
            </dd>
          </div>
        )}
        {event.feeInfo && (
          <div>
            <dt>Ücret</dt>
            <dd>{event.feeInfo}</dd>
          </div>
        )}
        {event.instructors && (
          <div>
            <dt>Eğitmenler</dt>
            <dd>{event.instructors}</dd>
          </div>
        )}
        {(event.minAge != null || event.maxAge != null) && (
          <div>
            <dt>Yaş aralığı (bilgi amaçlı)</dt>
            <dd>
              {event.minAge ?? '0'}–{event.maxAge ?? '∞'}
            </dd>
          </div>
        )}
        {event.accessibilityNote && (
          <div>
            <dt>Erişilebilirlik</dt>
            <dd>{event.accessibilityNote}</dd>
          </div>
        )}
      </dl>
      {event.programFlow && (
        <div className="event-info-panel__program">
          <h3>Program akışı</h3>
          <SafeHtml html={event.programFlow} />
        </div>
      )}
    </aside>
  );
}
