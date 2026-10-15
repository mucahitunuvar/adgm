import { listUsers, UserRole, UserStatus, type AdminUserListItemResponse } from '@gm/api-client';
import { getAuthErrorMessage, useAuth } from '@gm/auth';
import {
  DataTable,
  FilterBar,
  PageHeader,
  Pagination,
  SelectField,
  StatusBadge,
  TextField,
  buttonClassName,
  formatDateTime,
  type DataTableColumn,
} from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { enumOptions } from '../../i18n/enumOptions';
import { getEnumLabel } from '../../i18n/getEnumLabel';
import { ROUTES } from '../../routes';

const roleOptions = enumOptions(UserRole, 'UserRole');
const statusOptions = enumOptions(UserStatus, 'UserStatus');

const PAGE_SIZE = 20;

interface Filters {
  email: string;
  role: string;
  status: string;
}

const EMPTY_FILTERS: Filters = { email: '', role: '', status: '' };

export function UsersListPage() {
  const { client } = useAuth();
  const [draftFilters, setDraftFilters] = useState<Filters>(EMPTY_FILTERS);
  const [filters, setFilters] = useState<Filters>(EMPTY_FILTERS);
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = useQuery({
    queryKey: ['admin-users', filters, page],
    queryFn: () =>
      listUsers(client, {
        email: filters.email || undefined,
        role: filters.role || undefined,
        status: filters.status || undefined,
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
    setDraftFilters(EMPTY_FILTERS);
    setFilters(EMPTY_FILTERS);
    setPage(1);
  }

  const columns: DataTableColumn<AdminUserListItemResponse>[] = [
    { key: 'email', header: 'E-posta', render: (row) => row.email },
    { key: 'role', header: 'Rol', render: (row) => getEnumLabel('UserRole', row.role) },
    {
      key: 'status',
      header: 'Durum',
      render: (row) => (
        <StatusBadge tone={row.status === UserStatus.Active ? 'success' : 'neutral'}>
          {getEnumLabel('UserStatus', row.status)}
        </StatusBadge>
      ),
    },
    {
      key: 'emailConfirmed',
      header: 'E-posta doğrulaması',
      render: (row) => (
        <StatusBadge tone={row.emailConfirmed ? 'success' : 'warning'}>
          {row.emailConfirmed ? 'Doğrulandı' : 'Doğrulanmadı'}
        </StatusBadge>
      ),
    },
    {
      key: 'isLockedOut',
      header: 'Kilit',
      render: (row) => (row.isLockedOut ? <StatusBadge tone="danger">Kilitli</StatusBadge> : '—'),
    },
    { key: 'createdAtUtc', header: 'Kayıt tarihi', render: (row) => formatDateTime(row.createdAtUtc) },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Yönetim" title="Kullanıcılar" />
      <FilterBar onSubmit={handleSubmit} onReset={handleReset}>
        <TextField
          id="users-filter-email"
          label="E-posta"
          value={draftFilters.email}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, email: event.target.value }))}
        />
        <SelectField
          id="users-filter-role"
          label="Rol"
          value={draftFilters.role}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, role: event.target.value }))}
        >
          <option value="">Tümü</option>
          {roleOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </SelectField>
        <SelectField
          id="users-filter-status"
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
      </FilterBar>
      <DataTable
        caption="Kullanıcılar"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.userId}
        isLoading={isLoading}
        error={error ? getAuthErrorMessage(error) : undefined}
        rowActions={(row) => (
          <Link to={ROUTES.userDetail(row.userId)} className={buttonClassName('quiet')}>
            Görüntüle
          </Link>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
    </div>
  );
}
