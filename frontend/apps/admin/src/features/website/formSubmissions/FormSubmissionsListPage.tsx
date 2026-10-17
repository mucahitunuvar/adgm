import { FormSubmissionStatus, getApiErrorMessage, getFormDefinitions, getFormSubmissions, type FormSubmissionSummaryResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { buttonClassName, DataTable, FilterBar, formatDateTime, PageHeader, Pagination, SelectField, StatusBadge, TextField, type DataTableColumn } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { enumOptions } from '../../../i18n/enumOptions';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { ROUTES } from '../../../routes';

const statusOptions = enumOptions(FormSubmissionStatus, 'FormSubmissionStatus');
const PAGE_SIZE = 50;

interface Filters {
  formKey: string;
  status: string;
  archived: '' | 'true' | 'false';
  referenceNumber: string;
  fromDate: string;
  toDate: string;
}

const EMPTY_FILTERS: Filters = { formKey: '', status: '', archived: '', referenceNumber: '', fromDate: '', toDate: '' };

/**
 * PII ekranı (FRONTEND-FAZ2B-MASTER-PROMPT.md "Kişisel veri kuralları", 2b-1'deki
 * `NewsletterSubscribersListPage` ile aynı desen): filtre metni URL'ye yazılmaz (yalnız bileşen
 * durumunda), `staleTime: 0`/kısa `gcTime`, otomatik yenileme yok. Liste özeti (referans no, form,
 * durum, tarih, atanan kullanıcı kimliği) kişi adı/e-postası taşımaz - bunlar yalnız detay sayfasında.
 */
export function FormSubmissionsListPage() {
  const { client } = useAuth();
  const [draftFilters, setDraftFilters] = useState<Filters>(EMPTY_FILTERS);
  const [filters, setFilters] = useState<Filters>(EMPTY_FILTERS);
  const [page, setPage] = useState(1);

  const { data: forms } = useQuery({ queryKey: ['website-forms'], queryFn: () => getFormDefinitions(client) });

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-form-submissions', filters, page],
    queryFn: () =>
      getFormSubmissions(client, {
        formKey: filters.formKey || undefined,
        status: filters.status || undefined,
        archived: filters.archived === '' ? undefined : filters.archived === 'true',
        referenceNumber: filters.referenceNumber || undefined,
        from: filters.fromDate ? `${filters.fromDate}T00:00:00.000Z` : undefined,
        to: filters.toDate ? `${filters.toDate}T23:59:59.999Z` : undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
    staleTime: 0,
    gcTime: 30_000,
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

  const columns: DataTableColumn<FormSubmissionSummaryResponse>[] = [
    { key: 'referenceNumber', header: 'Referans no', render: (row) => row.referenceNumber },
    { key: 'formKey', header: 'Form', render: (row) => row.formKey },
    {
      key: 'status',
      header: 'Durum',
      render: (row) => (
        <StatusBadge tone={statusTone(row.status)}>{getEnumLabel('FormSubmissionStatus', row.status)}</StatusBadge>
      ),
    },
    { key: 'submittedAtUtc', header: 'Gönderim tarihi', render: (row) => formatDateTime(row.submittedAtUtc) },
    { key: 'attachmentCount', header: 'Ek dosya', render: (row) => row.attachmentCount, align: 'end' },
    {
      key: 'isArchived',
      header: 'Arşiv',
      render: (row) => <StatusBadge tone={row.isArchived ? 'neutral' : 'success'}>{row.isArchived ? 'Arşivde' : 'Etkin'}</StatusBadge>,
    },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title="Form başvuruları" description="Kişisel veri içerir; her detay görüntülemesi erişim kaydına yazılır." />
      <FilterBar onSubmit={handleSubmit} onReset={handleReset}>
        <SelectField
          id="submission-filter-form"
          label="Form"
          value={draftFilters.formKey}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, formKey: event.target.value }))}
        >
          <option value="">Tümü</option>
          {(forms ?? []).map((form) => (
            <option key={form.id} value={form.key}>
              {form.title} ({form.key})
            </option>
          ))}
        </SelectField>
        <SelectField
          id="submission-filter-status"
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
        <SelectField
          id="submission-filter-archived"
          label="Arşiv durumu"
          value={draftFilters.archived}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, archived: event.target.value as Filters['archived'] }))}
        >
          <option value="">Tümü</option>
          <option value="false">Etkin</option>
          <option value="true">Arşivde</option>
        </SelectField>
        <TextField
          id="submission-filter-reference"
          label="Referans no"
          value={draftFilters.referenceNumber}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, referenceNumber: event.target.value }))}
        />
        <TextField
          id="submission-filter-from"
          label="Başlangıç tarihi"
          type="date"
          value={draftFilters.fromDate}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, fromDate: event.target.value }))}
        />
        <TextField
          id="submission-filter-to"
          label="Bitiş tarihi"
          type="date"
          value={draftFilters.toDate}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, toDate: event.target.value }))}
        />
      </FilterBar>
      <DataTable
        caption="Form başvuruları"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        emptyTitle="Başvuru bulunamadı"
        emptyDescription="Bu filtrelerle eşleşen bir başvuru yok."
        rowActions={(row) => (
          <Link to={ROUTES.websiteFormSubmissionDetail(row.id)} className={buttonClassName('quiet')}>
            Görüntüle
          </Link>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
    </div>
  );
}

function statusTone(status: string): 'success' | 'warning' | 'danger' | 'neutral' {
  switch (status) {
    case FormSubmissionStatus.Approved:
    case FormSubmissionStatus.Completed:
      return 'success';
    case FormSubmissionStatus.Rejected:
      return 'danger';
    case FormSubmissionStatus.AwaitingInfo:
    case FormSubmissionStatus.InReview:
      return 'warning';
    default:
      return 'neutral';
  }
}
