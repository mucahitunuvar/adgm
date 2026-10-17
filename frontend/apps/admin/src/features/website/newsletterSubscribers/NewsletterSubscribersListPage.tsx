import {
  deleteNewsletterSubscriber,
  exportNewsletterSubscribers,
  getApiErrorMessage,
  getNewsletterSubscribers,
  NewsletterSubscriberStatus,
  type NewsletterSubscriberSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  DataTable,
  downloadBlob,
  FilterBar,
  formatDateTime,
  PageHeader,
  Pagination,
  SelectField,
  StatusBadge,
  TextField,
  useToast,
  type DataTableColumn,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { enumOptions } from '../../../i18n/enumOptions';
import { getEnumLabel } from '../../../i18n/getEnumLabel';

const statusOptions = enumOptions(NewsletterSubscriberStatus, 'NewsletterSubscriberStatus');
const PAGE_SIZE = 50;

interface Filters {
  status: string;
  language: string;
  search: string;
}

const EMPTY_FILTERS: Filters = { status: '', language: '', search: '' };

/**
 * PII ekranı (FRONTEND-FAZ2B-MASTER-PROMPT.md "Kişisel veri kuralları"): filtre metni URL'ye
 * yazılmaz (yalnız bileşen durumunda), `staleTime: 0`/kısa `gcTime`, otomatik yenileme yok.
 */
export function NewsletterSubscribersListPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [draftFilters, setDraftFilters] = useState<Filters>(EMPTY_FILTERS);
  const [filters, setFilters] = useState<Filters>(EMPTY_FILTERS);
  const [page, setPage] = useState(1);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-newsletter-subscribers', filters, page],
    queryFn: () =>
      getNewsletterSubscribers(client, {
        status: filters.status || undefined,
        language: filters.language || undefined,
        search: filters.search || undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
    staleTime: 0,
    gcTime: 30_000,
  });

  const exportMutation = useMutation({
    mutationFn: () => exportNewsletterSubscribers(client, { status: filters.status || undefined, language: filters.language || undefined }),
    onSuccess: ({ blob, filename }) => {
      downloadBlob(blob, filename ?? 'newsletter-subscribers.csv');
      showToast('Dışa aktarma indirildi; erişim kaydına yazıldı.', 'success');
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteNewsletterSubscriber(client, id),
    onSuccess: () => {
      showToast('Abone silindi.', 'success');
      setDeletingId(null);
      void queryClient.invalidateQueries({ queryKey: ['website-newsletter-subscribers'] });
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFilters(draftFilters);
    setPage(1);
  }

  function handleReset() {
    setDraftFilters(EMPTY_FILTERS);
    setFilters(EMPTY_FILTERS);
    setPage(1);
  }

  const columns: DataTableColumn<NewsletterSubscriberSummaryResponse>[] = [
    { key: 'email', header: 'E-posta', render: (row) => row.email },
    { key: 'languageCode', header: 'Dil', render: (row) => row.languageCode },
    {
      key: 'status',
      header: 'Durum',
      render: (row) => (
        <StatusBadge tone={row.status === NewsletterSubscriberStatus.Active ? 'success' : 'neutral'}>
          {getEnumLabel('NewsletterSubscriberStatus', row.status)}
        </StatusBadge>
      ),
    },
    { key: 'subscribedAtUtc', header: 'Kayıt tarihi', render: (row) => formatDateTime(row.subscribedAtUtc) },
    { key: 'confirmedAtUtc', header: 'Onay tarihi', render: (row) => (row.confirmedAtUtc ? formatDateTime(row.confirmedAtUtc) : '—') },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="Bülten aboneleri"
        description="Kişisel veri içerir; her görüntüleme ve dışa aktarma erişim kaydına yazılır."
        actions={
          <Button type="button" onClick={() => exportMutation.mutate()} disabled={exportMutation.isPending}>
            {exportMutation.isPending ? 'Hazırlanıyor…' : 'CSV olarak dışa aktar'}
          </Button>
        }
      />
      <FilterBar onSubmit={handleSubmit} onReset={handleReset}>
        <TextField
          id="newsletter-filter-search"
          label="E-posta ara"
          value={draftFilters.search}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, search: event.target.value }))}
        />
        <SelectField
          id="newsletter-filter-status"
          label="Durum"
          value={draftFilters.status}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, status: event.target.value }))}
        >
          <option value="">Tümü</option>
          {statusOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </SelectField>
        <TextField
          id="newsletter-filter-language"
          label="Dil kodu"
          value={draftFilters.language}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, language: event.target.value }))}
        />
      </FilterBar>
      <DataTable
        caption="Bülten aboneleri"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        emptyTitle="Abone bulunamadı"
        emptyDescription="Bu filtrelerle eşleşen bir abone yok."
        rowActions={(row) => (
          <Button type="button" tone="danger" onClick={() => setDeletingId(row.id)}>
            Sil
          </Button>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
      <ConfirmDialog
        open={deletingId !== null}
        title="Aboneyi sil"
        description="Bu abonelik kaydı (KVKK silme talebi) kalıcı olarak silinecek. Bu işlem geri alınamaz."
        isConfirming={deleteMutation.isPending}
        onConfirm={() => deletingId && deleteMutation.mutate(deletingId)}
        onCancel={() => setDeletingId(null)}
      />
    </div>
  );
}
