import {
  ContentItemStatus,
  getApiErrorMessage,
  getContentItems,
  getContentTypes,
  getSiteLanguages,
  type ContentItemSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, FilterBar, Pagination, PageHeader, SelectField, StatusBadge, TextField, buttonClassName, type DataTableColumn } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { enumOptions } from '../../../i18n/enumOptions';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { ROUTES } from '../../../routes';
import { CreateContentItemDialog } from './CreateContentItemDialog';

const PAGE_SIZE = 20;
const statusOptions = enumOptions(ContentItemStatus, 'ContentItemStatus');

interface Filters {
  contentTypeId: string;
  status: string;
  languageCode: string;
  search: string;
  isFeaturedOnly: boolean;
}

const EMPTY_FILTERS: Filters = { contentTypeId: '', status: '', languageCode: '', search: '', isFeaturedOnly: false };

export function ContentItemsListPage() {
  const { client } = useAuth();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState(EMPTY_FILTERS);
  const [draftFilters, setDraftFilters] = useState(EMPTY_FILTERS);

  const { data: contentTypes } = useQuery({ queryKey: ['website-content-types'], queryFn: () => getContentTypes(client) });
  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-content-items', filters, page],
    queryFn: () =>
      getContentItems(client, {
        page,
        pageSize: PAGE_SIZE,
        contentTypeId: filters.contentTypeId || undefined,
        status: filters.status || undefined,
        languageCode: filters.languageCode || undefined,
        search: filters.search || undefined,
        isFeatured: filters.isFeaturedOnly ? true : undefined,
      }),
  });

  function applyFilters() {
    setFilters(draftFilters);
    setPage(1);
  }

  function resetFilters() {
    setDraftFilters(EMPTY_FILTERS);
    setFilters(EMPTY_FILTERS);
    setPage(1);
  }

  const columns: DataTableColumn<ContentItemSummaryResponse>[] = [
    { key: 'title', header: 'Başlık', render: (row) => row.title },
    {
      key: 'status',
      header: 'Durum',
      render: (row) => (
        <StatusBadge tone={row.status === 'Published' ? 'success' : row.status === 'Archived' ? 'neutral' : 'warning'}>
          {getEnumLabel('ContentItemStatus', row.status)}
        </StatusBadge>
      ),
    },
    { key: 'isFeatured', header: 'Öne çıkan', render: (row) => (row.isFeatured ? 'Evet' : 'Hayır') },
    { key: 'sortOrder', header: 'Sıra', render: (row) => row.sortOrder, align: 'end' },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="İçerikler"
        actions={
          <>
            <Link to={ROUTES.websiteContentTrash} className={buttonClassName('quiet')}>
              Çöp kutusu
            </Link>{' '}
            <Button onClick={() => setIsCreateOpen(true)}>Yeni içerik</Button>
          </>
        }
      />
      <FilterBar onSubmit={(event) => { event.preventDefault(); applyFilters(); }} onReset={resetFilters}>
        <SelectField
          id="ci-filter-type"
          label="İçerik tipi"
          value={draftFilters.contentTypeId}
          onChange={(event) => setDraftFilters({ ...draftFilters, contentTypeId: event.target.value })}
        >
          <option value="">Tümü</option>
          {(contentTypes ?? []).map((type) => (
            <option key={type.id} value={type.id}>
              {type.defaultLanguageName}
            </option>
          ))}
        </SelectField>
        <SelectField
          id="ci-filter-status"
          label="Durum"
          value={draftFilters.status}
          onChange={(event) => setDraftFilters({ ...draftFilters, status: event.target.value })}
        >
          <option value="">Tümü</option>
          {statusOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </SelectField>
        <SelectField
          id="ci-filter-language"
          label="Dil"
          value={draftFilters.languageCode}
          onChange={(event) => setDraftFilters({ ...draftFilters, languageCode: event.target.value })}
        >
          <option value="">Tümü</option>
          {(languagesData?.items ?? []).map((language) => (
            <option key={language.code} value={language.code}>
              {language.name}
            </option>
          ))}
        </SelectField>
        <TextField
          id="ci-filter-search"
          label="Ara"
          value={draftFilters.search}
          onChange={(event) => setDraftFilters({ ...draftFilters, search: event.target.value })}
        />
      </FilterBar>
      <DataTable
        caption="İçerikler"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <Link to={ROUTES.websiteContentItemDetail(row.id)} className={buttonClassName('quiet')}>
            Görüntüle
          </Link>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
      <CreateContentItemDialog open={isCreateOpen} onCancel={() => setIsCreateOpen(false)} />
    </div>
  );
}
