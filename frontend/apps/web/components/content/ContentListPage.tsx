import { listPublicContents } from '@gm/api-client';
import { JsonLd } from '@gm/ui';
import { createServerApiClient } from '../../lib/apiClient';
import { ContentCard } from './ContentCard';
import { TeamMemberCard } from './TeamMemberCard';
import { FaqAccordionList } from './FaqAccordionList';
import { DocumentListSection } from './DocumentListSection';
import { CategoryFilterNav } from './CategoryFilterNav';
import { ContentSearchBox } from './ContentSearchBox';
import { LinkPagination } from './LinkPagination';

interface ContentListPageProps {
  contentTypeKey: string;
  basePath: string;
  category?: string;
  search?: string;
  page?: number;
  nonce?: string;
}

const PAGE_SIZE = 12;

/**
 * Genel liste şablonu (ADR-024 §4.1 `ListTemplate`): `cards`/`grid`/`list` → kart ızgarası,
 * `team-grid` → bağlantısız ekip kartları, `faq-accordion` → akordeon, `document-list` → ek
 * dosya indirme listesi. Etkinlik türleri (`detailTemplate === 'event'`) ayrı bir bileşende
 * (`EventListPage`) ele alınır - burada hiç çağrılmaz.
 */
export async function ContentListPage({ contentTypeKey, basePath, category, search, page = 1, nonce }: ContentListPageProps) {
  const client = createServerApiClient();
  const list = await listPublicContents(client, {
    type: contentTypeKey,
    category,
    search,
    page,
    pageSize: PAGE_SIZE,
  });

  const items = list.items.items;
  const pageHeading = <h1>{list.contentTypeName}</h1>;

  return (
    <div className="container section content-list-page">
      {list.jsonLd && <JsonLd data={list.jsonLd} nonce={nonce} />}
      {pageHeading}
      <div className="content-list-page__toolbar">
        {list.categories.length > 0 && (
          <CategoryFilterNav basePath={basePath} categories={list.categories} selectedSlug={list.selectedCategory?.slug} search={search} />
        )}
        <ContentSearchBox basePath={basePath} category={list.selectedCategory?.slug} defaultValue={search} />
      </div>

      {items.length === 0 ? (
        <p className="page-lead">Bu kategoride henüz içerik yok.</p>
      ) : list.listTemplate === 'team-grid' ? (
        <div className="team-grid">
          {items.map((item) => (
            <TeamMemberCard key={item.id} item={item} />
          ))}
        </div>
      ) : list.listTemplate === 'faq-accordion' ? (
        <FaqAccordionList items={items} />
      ) : list.listTemplate === 'document-list' ? (
        <DocumentListSection items={items} />
      ) : (
        <div className="block-content-list__grid">
          {items.map((item) => (
            <ContentCard key={item.id} item={item} />
          ))}
        </div>
      )}

      <LinkPagination
        basePath={basePath}
        page={Number(list.items.page)}
        pageSize={Number(list.items.pageSize)}
        totalCount={Number(list.items.totalCount)}
        query={{ category: list.selectedCategory?.slug, search }}
      />
    </div>
  );
}
