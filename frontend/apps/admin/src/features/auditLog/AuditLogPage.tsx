import { listAuditLog, AdminActionType, type AdminAuditLogItemResponse } from '@gm/api-client';
import { getAuthErrorMessage, useAuth } from '@gm/auth';
import {
  DataTable,
  FilterBar,
  PageHeader,
  Pagination,
  SelectField,
  TextField,
  formatDateTime,
  type DataTableColumn,
} from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { useSearchParams } from 'react-router-dom';
import { enumOptions } from '../../i18n/enumOptions';
import { getEnumLabel } from '../../i18n/getEnumLabel';

const PAGE_SIZE = 20;
const actionTypeOptions = enumOptions(AdminActionType, 'AdminActionType');

interface Filters {
  targetUserId: string;
  actionType: string;
  fromDate: string;
  toDate: string;
}

function emptyFilters(targetUserId = ''): Filters {
  return { targetUserId, actionType: '', fromDate: '', toDate: '' };
}

/** Yalnız Admin, salt okunur. Kişisel veri sızdırmamak için e-posta burada gösterilmez, yalnız kullanıcı kimliği. */
export function AuditLogPage() {
  const { client } = useAuth();
  const [searchParams] = useSearchParams();
  const initialUserId = searchParams.get('userId') ?? '';

  const [draftFilters, setDraftFilters] = useState<Filters>(emptyFilters(initialUserId));
  const [filters, setFilters] = useState<Filters>(emptyFilters(initialUserId));
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = useQuery({
    queryKey: ['admin-audit-log', filters, page],
    queryFn: () =>
      listAuditLog(client, {
        targetUserId: filters.targetUserId || undefined,
        actionType: filters.actionType || undefined,
        fromUtc: filters.fromDate ? `${filters.fromDate}T00:00:00.000Z` : undefined,
        toUtc: filters.toDate ? `${filters.toDate}T23:59:59.999Z` : undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFilters(draftFilters);
    setPage(1);
  }

  function handleReset() {
    setDraftFilters(emptyFilters());
    setFilters(emptyFilters());
    setPage(1);
  }

  const columns: DataTableColumn<AdminAuditLogItemResponse>[] = [
    { key: 'actionType', header: 'Eylem', render: (row) => getEnumLabel('AdminActionType', row.actionType) },
    { key: 'targetUserId', header: 'Hedef kullanıcı', render: (row) => <code>{row.targetUserId}</code> },
    { key: 'adminUserId', header: 'İşlemi yapan', render: (row) => <code>{row.adminUserId}</code> },
    { key: 'occurredAtUtc', header: 'Tarih', render: (row) => formatDateTime(row.occurredAtUtc) },
    { key: 'details', header: 'Ayrıntı', render: (row) => row.details ?? '—' },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Yönetim" title="Denetim kaydı" description="Kullanıcı yönetimi işlemlerinin salt okunur kaydı." />
      <FilterBar onSubmit={handleSubmit} onReset={handleReset}>
        <TextField
          id="audit-filter-user"
          label="Hedef kullanıcı kimliği"
          hint="Kullanıcılar sayfasındaki bir kullanıcının kimliğini yapıştırın."
          value={draftFilters.targetUserId}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, targetUserId: event.target.value }))}
        />
        <SelectField
          id="audit-filter-action"
          label="Eylem"
          value={draftFilters.actionType}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, actionType: event.target.value }))}
        >
          <option value="">Tümü</option>
          {actionTypeOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </SelectField>
        <TextField
          id="audit-filter-from"
          label="Başlangıç tarihi"
          type="date"
          value={draftFilters.fromDate}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, fromDate: event.target.value }))}
        />
        <TextField
          id="audit-filter-to"
          label="Bitiş tarihi"
          type="date"
          value={draftFilters.toDate}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, toDate: event.target.value }))}
        />
      </FilterBar>
      <DataTable
        caption="Denetim kaydı"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getAuthErrorMessage(error) : undefined}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
    </div>
  );
}
