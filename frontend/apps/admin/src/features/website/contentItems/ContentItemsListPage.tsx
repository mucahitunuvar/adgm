import {
  ContentItemStatus,
  getApiErrorMessage,
  getContentCategoriesByType,
  getContentItems,
  getContentTypes,
  getSiteLanguages,
  listTags,
  type ContentCategoryTreeItemResponse,
  type ContentItemSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  CheckboxField,
  DataTable,
  FilterBar,
  Pagination,
  PageHeader,
  SelectField,
  StatusBadge,
  TextField,
  buttonClassName,
  type DataTableColumn,
} from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { enumOptions } from '../../../i18n/enumOptions';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { ROUTES } from '../../../routes';
import { CreateContentItemDialog } from './CreateContentItemDialog';

const PAGE_SIZE = 20;
const statusOptions = enumOptions(ContentItemStatus, 'ContentItemStatus');

/**
 * Backend'in `ContentItemSortField`/`ContentItemSortDirection`'ı (GetContentItems, Application.Abstractions)
 * - bunlar backend'in Domain enum sözleşmesinin (`docs/api/enums.v1.json`, `enums.generated.ts`) parçası
 * DEĞİL (bilerek: sıralama alanı adları bir Domain kavramı değil, yalnız bu sorgunun kendi sözlüğü),
 * bu yüzden elle tutuluyor - `GetContentItemsQueryHandler`'daki `EnumParsing.TryParseDefined` ile
 * birebir eşleşmeli.
 */
const ORDER_BY_OPTIONS: Array<{ value: string; label: string }> = [
  { value: 'Title', label: 'Başlık' },
  { value: 'CreatedAt', label: 'Oluşturulma tarihi' },
  { value: 'UpdatedAt', label: 'Güncellenme tarihi' },
  { value: 'PublishAt', label: 'Yayın tarihi' },
  { value: 'SortOrder', label: 'Sıra' },
];

const DIRECTION_OPTIONS: Array<{ value: string; label: string }> = [
  { value: 'Asc', label: 'Artan' },
  { value: 'Desc', label: 'Azalan' },
];

interface Filters {
  contentTypeId: string;
  status: string;
  languageCode: string;
  search: string;
  isFeaturedOnly: boolean;
  categoryId: string;
  tagId: string;
  publishedFromDate: string;
  publishedToDate: string;
  createdFromDate: string;
  createdToDate: string;
  orderBy: string;
  direction: string;
}

const EMPTY_FILTERS: Filters = {
  contentTypeId: '',
  status: '',
  languageCode: '',
  search: '',
  isFeaturedOnly: false,
  categoryId: '',
  tagId: '',
  publishedFromDate: '',
  publishedToDate: '',
  createdFromDate: '',
  createdToDate: '',
  orderBy: '',
  direction: '',
};

function filtersFromSearchParams(searchParams: URLSearchParams): Filters {
  return {
    contentTypeId: searchParams.get('contentTypeId') ?? '',
    status: searchParams.get('status') ?? '',
    languageCode: searchParams.get('languageCode') ?? '',
    search: searchParams.get('search') ?? '',
    isFeaturedOnly: searchParams.get('isFeatured') === 'true',
    categoryId: searchParams.get('categoryId') ?? '',
    tagId: searchParams.get('tagId') ?? '',
    publishedFromDate: searchParams.get('publishedFrom') ?? '',
    publishedToDate: searchParams.get('publishedTo') ?? '',
    createdFromDate: searchParams.get('createdFrom') ?? '',
    createdToDate: searchParams.get('createdTo') ?? '',
    orderBy: searchParams.get('orderBy') ?? '',
    direction: searchParams.get('direction') ?? '',
  };
}

function pageFromSearchParams(searchParams: URLSearchParams): number {
  const raw = Number(searchParams.get('page'));
  return Number.isInteger(raw) && raw > 0 ? raw : 1;
}

function searchParamsFromFiltersAndPage(filters: Filters, page: number): URLSearchParams {
  const params = new URLSearchParams();
  if (filters.contentTypeId) params.set('contentTypeId', filters.contentTypeId);
  if (filters.status) params.set('status', filters.status);
  if (filters.languageCode) params.set('languageCode', filters.languageCode);
  if (filters.search) params.set('search', filters.search);
  if (filters.isFeaturedOnly) params.set('isFeatured', 'true');
  if (filters.categoryId) params.set('categoryId', filters.categoryId);
  if (filters.tagId) params.set('tagId', filters.tagId);
  if (filters.publishedFromDate) params.set('publishedFrom', filters.publishedFromDate);
  if (filters.publishedToDate) params.set('publishedTo', filters.publishedToDate);
  if (filters.createdFromDate) params.set('createdFrom', filters.createdFromDate);
  if (filters.createdToDate) params.set('createdTo', filters.createdToDate);
  if (filters.orderBy) params.set('orderBy', filters.orderBy);
  if (filters.direction) params.set('direction', filters.direction);
  if (page > 1) params.set('page', String(page));
  return params;
}

/** Seçilen takvim gününün SONUNU backend'in yarı açık (`< to`) aralığına çevirir: ertesi günün başlangıcı. */
function dayEndAsHalfOpenUpperBoundUtc(dateOnly: string): string {
  const nextDay = new Date(`${dateOnly}T00:00:00.000Z`);
  nextDay.setUTCDate(nextDay.getUTCDate() + 1);
  return nextDay.toISOString();
}

function flattenCategories(tree: readonly ContentCategoryTreeItemResponse[]): Array<{ id: string; label: string }> {
  const flat: Array<{ id: string; label: string }> = [];
  for (const root of tree) {
    flat.push({ id: root.id, label: root.name });
    for (const child of root.children) {
      flat.push({ id: child.id, label: `— ${child.name}` });
    }
  }
  return flat;
}

export function ContentItemsListPage() {
  const { client } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const page = pageFromSearchParams(searchParams);
  const filters = filtersFromSearchParams(searchParams);
  const [draftFilters, setDraftFilters] = useState(filters);

  // URL geri/ileri ile değiştiğinde (ör. tarayıcı geri tuşu) taslak formu da senkronlar - aksi halde
  // form, kullanıcının az önce vazgeçtiği taslak değerleri göstermeye devam ederdi. Render sırasında
  // (effect içinde DEĞİL) karşılaştırıp düzeltmek React'ın "prop değişince state'i ayarla" deseni -
  // ekstra bir commit sonrası render'a gerek kalmaz.
  const [syncedSearchParamsString, setSyncedSearchParamsString] = useState(searchParams.toString());
  if (searchParams.toString() !== syncedSearchParamsString) {
    setSyncedSearchParamsString(searchParams.toString());
    setDraftFilters(filters);
  }

  const { data: contentTypes } = useQuery({ queryKey: ['website-content-types'], queryFn: () => getContentTypes(client) });
  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data: categoriesTree } = useQuery({
    queryKey: ['website-content-categories', draftFilters.contentTypeId],
    queryFn: () => getContentCategoriesByType(client, draftFilters.contentTypeId),
    enabled: Boolean(draftFilters.contentTypeId),
  });
  const { data: tagsPage } = useQuery({ queryKey: ['website-tags-all'], queryFn: () => listTags(client, { pageSize: 200 }) });

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
        categoryId: filters.categoryId || undefined,
        tagId: filters.tagId || undefined,
        publishedFromUtc: filters.publishedFromDate ? `${filters.publishedFromDate}T00:00:00.000Z` : undefined,
        publishedToUtc: filters.publishedToDate ? dayEndAsHalfOpenUpperBoundUtc(filters.publishedToDate) : undefined,
        createdFromUtc: filters.createdFromDate ? `${filters.createdFromDate}T00:00:00.000Z` : undefined,
        createdToUtc: filters.createdToDate ? dayEndAsHalfOpenUpperBoundUtc(filters.createdToDate) : undefined,
        orderBy: filters.orderBy || undefined,
        direction: filters.direction || undefined,
      }),
  });

  function commit(nextFilters: Filters, nextPage: number) {
    setSearchParams(searchParamsFromFiltersAndPage(nextFilters, nextPage));
  }

  function applyFilters() {
    commit(draftFilters, 1);
  }

  function resetFilters() {
    setDraftFilters(EMPTY_FILTERS);
    commit(EMPTY_FILTERS, 1);
  }

  function handleSortChange(key: string) {
    const nextDirection = filters.orderBy === key && filters.direction === 'Asc' ? 'Desc' : 'Asc';
    commit({ ...filters, orderBy: key, direction: nextDirection }, 1);
  }

  const categoryOptions = flattenCategories(categoriesTree ?? []);

  const columns: DataTableColumn<ContentItemSummaryResponse>[] = [
    { key: 'Title', header: 'Başlık', render: (row) => row.title, sortable: true },
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
    { key: 'SortOrder', header: 'Sıra', render: (row) => row.sortOrder, align: 'end', sortable: true },
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
          onChange={(event) => setDraftFilters({ ...draftFilters, contentTypeId: event.target.value, categoryId: '' })}
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
        <CheckboxField
          id="ci-filter-featured"
          label="Yalnız öne çıkanlar"
          checked={draftFilters.isFeaturedOnly}
          onChange={(event) => setDraftFilters({ ...draftFilters, isFeaturedOnly: event.target.checked })}
        />
        <SelectField
          id="ci-filter-category"
          label="Kategori"
          hint={draftFilters.contentTypeId ? undefined : 'Önce içerik tipi seçin'}
          value={draftFilters.categoryId}
          disabled={!draftFilters.contentTypeId}
          onChange={(event) => setDraftFilters({ ...draftFilters, categoryId: event.target.value })}
        >
          <option value="">Tümü</option>
          {categoryOptions.map((option) => (
            <option key={option.id} value={option.id}>
              {option.label}
            </option>
          ))}
        </SelectField>
        <SelectField
          id="ci-filter-tag"
          label="Etiket"
          value={draftFilters.tagId}
          onChange={(event) => setDraftFilters({ ...draftFilters, tagId: event.target.value })}
        >
          <option value="">Tümü</option>
          {(tagsPage?.items ?? []).map((tag) => (
            <option key={tag.id} value={tag.id}>
              {tag.name}
            </option>
          ))}
        </SelectField>
        <TextField
          id="ci-filter-published-from"
          label="Yayın başlangıcı"
          type="date"
          value={draftFilters.publishedFromDate}
          onChange={(event) => setDraftFilters({ ...draftFilters, publishedFromDate: event.target.value })}
        />
        <TextField
          id="ci-filter-published-to"
          label="Yayın bitişi"
          type="date"
          value={draftFilters.publishedToDate}
          onChange={(event) => setDraftFilters({ ...draftFilters, publishedToDate: event.target.value })}
        />
        <TextField
          id="ci-filter-created-from"
          label="Oluşturulma başlangıcı"
          type="date"
          value={draftFilters.createdFromDate}
          onChange={(event) => setDraftFilters({ ...draftFilters, createdFromDate: event.target.value })}
        />
        <TextField
          id="ci-filter-created-to"
          label="Oluşturulma bitişi"
          type="date"
          value={draftFilters.createdToDate}
          onChange={(event) => setDraftFilters({ ...draftFilters, createdToDate: event.target.value })}
        />
        <SelectField
          id="ci-filter-order-by"
          label="Sırala"
          value={draftFilters.orderBy}
          onChange={(event) => setDraftFilters({ ...draftFilters, orderBy: event.target.value })}
        >
          <option value="">Varsayılan (sıra)</option>
          {ORDER_BY_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </SelectField>
        <SelectField
          id="ci-filter-direction"
          label="Yön"
          value={draftFilters.direction}
          disabled={!draftFilters.orderBy}
          onChange={(event) => setDraftFilters({ ...draftFilters, direction: event.target.value })}
        >
          <option value="">Artan</option>
          {DIRECTION_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </SelectField>
      </FilterBar>
      <DataTable
        caption="İçerikler"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        sortKey={filters.orderBy || undefined}
        sortDirection={filters.direction === 'Desc' ? 'desc' : 'asc'}
        onSortChange={handleSortChange}
        rowActions={(row) => (
          <Link to={ROUTES.websiteContentItemDetail(row.id)} className={buttonClassName('quiet')}>
            Görüntüle
          </Link>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={(nextPage) => commit(filters, nextPage)} />}
      <CreateContentItemDialog open={isCreateOpen} onCancel={() => setIsCreateOpen(false)} />
    </div>
  );
}
