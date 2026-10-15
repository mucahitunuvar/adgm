import {
  activateSiteLanguage,
  createSiteLanguage,
  deactivateSiteLanguage,
  getApiErrorMessage,
  getSiteLanguages,
  setDefaultSiteLanguage,
  updateSiteLanguage,
  type SiteLanguageResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, PageHeader, StatusBadge, type DataTableColumn, useToast } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { CreateLanguageDialog } from './CreateLanguageDialog';
import { EditLanguageDialog } from './EditLanguageDialog';

const QUERY_KEY = ['website-languages'] as const;

export function LanguagesPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingLanguage, setEditingLanguage] = useState<SiteLanguageResponse | null>(null);

  const { data, isLoading, error } = useQuery({ queryKey: QUERY_KEY, queryFn: () => getSiteLanguages(client) });

  function onMutated(message: string) {
    showToast(message, 'success');
    void queryClient.invalidateQueries({ queryKey: QUERY_KEY });
  }

  function onMutationError(mutationError: unknown) {
    showToast(getApiErrorMessage(mutationError), 'danger');
  }

  const createMutation = useMutation({
    mutationFn: (input: { code: string; name: string; sortOrder: number }) => createSiteLanguage(client, input),
    onSuccess: () => {
      onMutated('Dil oluşturuldu.');
      setIsCreateOpen(false);
    },
    onError: onMutationError,
  });

  const updateMutation = useMutation({
    mutationFn: (input: { id: string; name: string; sortOrder: number }) =>
      updateSiteLanguage(client, input.id, { name: input.name, sortOrder: input.sortOrder }),
    onSuccess: () => {
      onMutated('Dil güncellendi.');
      setEditingLanguage(null);
    },
    onError: onMutationError,
  });

  const activateMutation = useMutation({
    mutationFn: (id: string) => activateSiteLanguage(client, id),
    onSuccess: () => onMutated('Dil etkinleştirildi.'),
    onError: onMutationError,
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => deactivateSiteLanguage(client, id),
    onSuccess: () => onMutated('Dil pasifleştirildi.'),
    onError: onMutationError,
  });

  const setDefaultMutation = useMutation({
    mutationFn: (id: string) => setDefaultSiteLanguage(client, id),
    onSuccess: () => onMutated('Varsayılan dil güncellendi.'),
    onError: onMutationError,
  });

  const columns: DataTableColumn<SiteLanguageResponse>[] = [
    { key: 'code', header: 'Kod', render: (row) => row.code },
    { key: 'name', header: 'Ad', render: (row) => row.name },
    { key: 'sortOrder', header: 'Sıra', render: (row) => row.sortOrder, align: 'end' },
    {
      key: 'isDefault',
      header: 'Varsayılan',
      render: (row) => (row.isDefault ? <StatusBadge tone="info">Varsayılan</StatusBadge> : '—'),
    },
    {
      key: 'isActive',
      header: 'Durum',
      render: (row) => <StatusBadge tone={row.isActive ? 'success' : 'neutral'}>{row.isActive ? 'Etkin' : 'Pasif'}</StatusBadge>,
    },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="Diller"
        description="Public sitede içerik sekmeleri bu dillere göre gösterilir. Tek etkin dil varsa sekmeler gizlenir."
        actions={<Button onClick={() => setIsCreateOpen(true)}>Yeni dil ekle</Button>}
      />
      <DataTable
        caption="Diller"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <>
            <Button tone="quiet" onClick={() => setEditingLanguage(row)}>
              Düzenle
            </Button>{' '}
            {row.isActive ? (
              <Button tone="quiet" onClick={() => deactivateMutation.mutate(row.id)} disabled={row.isDefault || deactivateMutation.isPending}>
                Pasifleştir
              </Button>
            ) : (
              <Button tone="quiet" onClick={() => activateMutation.mutate(row.id)} disabled={activateMutation.isPending}>
                Etkinleştir
              </Button>
            )}{' '}
            {!row.isDefault && (
              <Button tone="quiet" onClick={() => setDefaultMutation.mutate(row.id)} disabled={setDefaultMutation.isPending}>
                Varsayılan yap
              </Button>
            )}
          </>
        )}
      />
      <CreateLanguageDialog
        open={isCreateOpen}
        isSubmitting={createMutation.isPending}
        onConfirm={(input) => createMutation.mutate(input)}
        onCancel={() => setIsCreateOpen(false)}
      />
      <EditLanguageDialog
        key={editingLanguage?.id ?? 'none'}
        language={editingLanguage}
        isSubmitting={updateMutation.isPending}
        onConfirm={(input) => editingLanguage && updateMutation.mutate({ id: editingLanguage.id, ...input })}
        onCancel={() => setEditingLanguage(null)}
      />
    </div>
  );
}
