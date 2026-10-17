import {
  createRedirect,
  deleteRedirect,
  getApiErrorMessage,
  getRedirects,
  getSiteLanguages,
  updateRedirect,
  type RedirectResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  DataTable,
  FilterBar,
  formatDateTime,
  Pagination,
  SelectField,
  StatusBadge,
  TextField,
  useToast,
  type DataTableColumn,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { RedirectDialog, type RedirectDraft } from './RedirectDialog';

const PAGE_SIZE = 50;

interface Filters {
  languageCode: string;
  isAutomatic: string;
  search: string;
}

const EMPTY_FILTERS: Filters = { languageCode: '', isAutomatic: '', search: '' };

export function RedirectsListTab() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [draftFilters, setDraftFilters] = useState<Filters>(EMPTY_FILTERS);
  const [filters, setFilters] = useState<Filters>(EMPTY_FILTERS);
  const [page, setPage] = useState(1);
  const [editingTarget, setEditingTarget] = useState<RedirectResponse | null | undefined>(undefined);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data, isLoading, error } = useQuery({
    queryKey: ['website-redirects', filters, page],
    queryFn: () =>
      getRedirects(client, {
        languageCode: filters.languageCode || undefined,
        isAutomatic: filters.isAutomatic === '' ? undefined : filters.isAutomatic === 'true',
        search: filters.search || undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
  });

  const languages = languagesData?.items ?? [];

  function onMutated(message: string) {
    showToast(message, 'success');
    void queryClient.invalidateQueries({ queryKey: ['website-redirects'] });
    setEditingTarget(undefined);
  }

  const createMutation = useMutation({
    mutationFn: (draft: RedirectDraft) =>
      createRedirect(client, {
        languageCode: draft.languageCode,
        fromPath: draft.fromPath,
        targetKind: draft.target.targetKind,
        targetContentItemId: draft.target.targetContentItemId,
        targetPath: draft.target.targetPath,
        statusCode: draft.target.statusCode,
      }),
    onSuccess: () => onMutated('Yönlendirme oluşturuldu.'),
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, draft }: { id: string; draft: RedirectDraft }) =>
      updateRedirect(client, id, {
        targetKind: draft.target.targetKind,
        targetContentItemId: draft.target.targetContentItemId,
        targetPath: draft.target.targetPath,
        statusCode: draft.target.statusCode,
      }),
    onSuccess: () => onMutated('Yönlendirme güncellendi.'),
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteRedirect(client, id),
    onSuccess: () => {
      showToast('Yönlendirme silindi.', 'success');
      setDeletingId(null);
      void queryClient.invalidateQueries({ queryKey: ['website-redirects'] });
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

  const columns: DataTableColumn<RedirectResponse>[] = [
    { key: 'fromPath', header: 'Kaynak yol', render: (row) => `/${row.fromPath}` },
    { key: 'languageCode', header: 'Dil', render: (row) => row.languageCode },
    { key: 'target', header: 'Hedef', render: (row) => (row.targetPath ? row.targetPath : row.targetContentItemId ? 'İçerik' : '—') },
    { key: 'statusCode', header: 'Tür', render: (row) => row.statusCode },
    { key: 'isAutomatic', header: 'Kaynak', render: (row) => (row.isAutomatic ? <StatusBadge tone="info">Otomatik</StatusBadge> : <StatusBadge tone="neutral">Elle</StatusBadge>) },
    { key: 'hitCount', header: 'İsabet', render: (row) => row.hitCount, align: 'end' },
    { key: 'lastHitAtUtc', header: 'Son isabet', render: (row) => (row.lastHitAtUtc ? formatDateTime(row.lastHitAtUtc) : '—') },
  ];

  return (
    <div>
      <FilterBar onSubmit={handleSubmit} onReset={handleReset}>
        <TextField
          id="redirects-filter-search"
          label="Kaynak yol ara"
          value={draftFilters.search}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, search: event.target.value }))}
        />
        <SelectField
          id="redirects-filter-language"
          label="Dil"
          value={draftFilters.languageCode}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, languageCode: event.target.value }))}
        >
          <option value="">Tümü</option>
          {languages.map((language) => (
            <option key={language.code} value={language.code}>
              {language.name}
            </option>
          ))}
        </SelectField>
        <SelectField
          id="redirects-filter-automatic"
          label="Kaynak"
          value={draftFilters.isAutomatic}
          onChange={(event) => setDraftFilters((prev) => ({ ...prev, isAutomatic: event.target.value }))}
        >
          <option value="">Tümü</option>
          <option value="true">Otomatik</option>
          <option value="false">Elle oluşturulmuş</option>
        </SelectField>
      </FilterBar>
      <Button type="button" onClick={() => setEditingTarget(null)}>
        Yeni yönlendirme
      </Button>
      <DataTable
        caption="Yönlendirmeler"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <>
            {!row.isAutomatic && (
              <Button type="button" tone="quiet" onClick={() => setEditingTarget(row)}>
                Düzenle
              </Button>
            )}
            <Button type="button" tone="danger" onClick={() => setDeletingId(row.id)}>
              Sil
            </Button>
          </>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}

      <RedirectDialog
        key={editingTarget?.id ?? 'new'}
        open={editingTarget !== undefined}
        redirect={editingTarget ?? null}
        languages={languages}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
        onConfirm={(draft) => (editingTarget ? updateMutation.mutate({ id: editingTarget.id, draft }) : createMutation.mutate(draft))}
        onCancel={() => setEditingTarget(undefined)}
      />
      <ConfirmDialog
        open={deletingId !== null}
        title="Yönlendirmeyi sil"
        description="Bu yönlendirme kalıcı olarak silinecek; bu kaynak yola gelen ziyaretçiler artık 404 alır."
        isConfirming={deleteMutation.isPending}
        onConfirm={() => deletingId && deleteMutation.mutate(deletingId)}
        onCancel={() => setDeletingId(null)}
      />
    </div>
  );
}
