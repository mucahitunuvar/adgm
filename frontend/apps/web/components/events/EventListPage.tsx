import { listPublicEvents } from '@gm/api-client';
import { createServerApiClient } from '../../lib/apiClient';
import { EventCard } from './EventCard';
import { LinkPagination } from '../content/LinkPagination';

const PAGE_SIZE = 12;

/**
 * Etkinlik türleri (`detailTemplate === 'event'`, ör. "Etkinlik"/"Eğitim ve Atölye") için genel
 * içerik listesi yerine kullanılır - `GetPublicEvents` her öğe için `registrationState`/
 * `remainingSpots` döner, genel `listPublicContents` dönmez (ADR-024 §8.1/§11.3).
 */
export async function EventListPage({ contentTypeKey, basePath, when = 'upcoming', page = 1 }: { contentTypeKey: string; basePath: string; when?: string; page?: number }) {
  const client = createServerApiClient();
  const result = await listPublicEvents(client, { typeKey: contentTypeKey, when, page, pageSize: PAGE_SIZE });

  return (
    <div className="container section content-list-page">
      <h1>Etkinlikler</h1>
      <nav className="event-list-filter" aria-label="Zaman filtresi">
        <a href={basePath} aria-current={when === 'upcoming' ? 'true' : undefined} className="button button--quiet">
          Yaklaşan
        </a>
        <a href={`${basePath}?when=past`} aria-current={when === 'past' ? 'true' : undefined} className="button button--quiet">
          Geçmiş
        </a>
        <a href={`${basePath}?when=all`} aria-current={when === 'all' ? 'true' : undefined} className="button button--quiet">
          Tümü
        </a>
      </nav>

      {result.items.length === 0 ? (
        <p className="page-lead">Bu kategoride henüz etkinlik yok.</p>
      ) : (
        <div className="block-content-list__grid">
          {result.items.map((item) => (
            <EventCard key={item.id} item={item} />
          ))}
        </div>
      )}

      <LinkPagination
        basePath={basePath}
        page={Number(result.page)}
        pageSize={Number(result.pageSize)}
        totalCount={Number(result.totalCount)}
        query={{ when: when !== 'upcoming' ? when : undefined }}
      />
    </div>
  );
}
