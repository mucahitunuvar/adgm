import {
  activateImpactMetric,
  createImpactMetric,
  deactivateImpactMetric,
  deleteImpactMetric,
  getApiErrorMessage,
  getImpactMetricById,
  getImpactMetrics,
  getSiteLanguages,
  updateImpactMetric,
  updateImpactMetricTranslation,
  type ImpactMetricSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  DataTable,
  EmptyState,
  ErrorState,
  LoadingState,
  PageHeader,
  Pagination,
  StatusBadge,
  useToast,
  type DataTableColumn,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { ImpactMetricEditDialog } from './ImpactMetricEditDialog';

const PAGE_SIZE = 50;

export function ImpactMetricsPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [editingId, setEditingId] = useState<string | null | undefined>(undefined);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data, isLoading, error } = useQuery({
    queryKey: ['website-impact-metrics', page],
    queryFn: () => getImpactMetrics(client, { page, pageSize: PAGE_SIZE }),
  });
  const { data: editingDetail } = useQuery({
    queryKey: ['website-impact-metric', editingId],
    queryFn: () => getImpactMetricById(client, editingId!),
    enabled: Boolean(editingId),
  });

  const languages = languagesData?.items ?? [];

  function onMutated(message: string) {
    showToast(message, 'success');
    void queryClient.invalidateQueries({ queryKey: ['website-impact-metrics'] });
    void queryClient.invalidateQueries({ queryKey: ['website-impact-metric', editingId] });
  }

  function onMutationError(mutationError: unknown) {
    showToast(getApiErrorMessage(mutationError), 'danger');
  }

  const createMutation = useMutation({
    mutationFn: (draft: { value: string; iconKey: string; sortOrder: number; defaultLanguageLabel: string }) =>
      createImpactMetric(client, {
        value: Number(draft.value),
        iconKey: draft.iconKey || null,
        sortOrder: draft.sortOrder,
        defaultLanguageLabel: draft.defaultLanguageLabel,
        defaultLanguageUnit: null,
        defaultLanguagePeriod: null,
        defaultLanguageSource: null,
      }),
    onSuccess: (response) => {
      onMutated('Rakam oluşturuldu.');
      setEditingId(response.id);
    },
    onError: onMutationError,
  });

  const updateCoreMutation = useMutation({
    mutationFn: (draft: { value: string; iconKey: string; sortOrder: number }) => {
      if (!editingDetail) throw new Error('Henüz yüklenmedi.');
      return updateImpactMetric(client, editingDetail.id, {
        rowVersion: editingDetail.rowVersion,
        value: Number(draft.value),
        iconKey: draft.iconKey || null,
        sortOrder: draft.sortOrder,
      });
    },
    onSuccess: () => onMutated('Temel bilgiler kaydedildi.'),
    onError: onMutationError,
  });

  const updateTranslationMutation = useMutation({
    mutationFn: ({
      languageCode,
      draft,
    }: {
      languageCode: string;
      draft: { label: string; unit: string; period: string; source: string };
    }) => {
      if (!editingDetail) throw new Error('Henüz yüklenmedi.');
      return updateImpactMetricTranslation(client, editingDetail.id, languageCode, {
        rowVersion: editingDetail.rowVersion,
        label: draft.label,
        unit: draft.unit || null,
        period: draft.period || null,
        source: draft.source || null,
      });
    },
    onSuccess: () => onMutated('Çeviri kaydedildi.'),
    onError: onMutationError,
  });

  const toggleActiveMutation = useMutation({
    mutationFn: (row: ImpactMetricSummaryResponse) =>
      row.isActive ? deactivateImpactMetric(client, row.id, row.rowVersion) : activateImpactMetric(client, row.id, row.rowVersion),
    onSuccess: (_void, row) => onMutated(row.isActive ? 'Rakam pasifleştirildi.' : 'Rakam etkinleştirildi.'),
    onError: onMutationError,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteImpactMetric(client, id),
    onSuccess: () => {
      onMutated('Rakam silindi.');
      setDeletingId(null);
    },
    onError: onMutationError,
  });

  const columns: DataTableColumn<ImpactMetricSummaryResponse>[] = [
    { key: 'label', header: 'Etiket', render: (row) => row.label },
    { key: 'value', header: 'Değer', render: (row) => `${row.value}${row.unit ? ` ${row.unit}` : ''}`, align: 'end' },
    { key: 'sortOrder', header: 'Sıra', render: (row) => row.sortOrder, align: 'end' },
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
        title="Rakamlar şeridi"
        description="Ana sayfadaki etki rakamları (ör. 'Faydalanılan genç sayısı')."
        actions={<Button onClick={() => setEditingId(null)}>Yeni rakam</Button>}
      />
      {isLoading ? (
        <LoadingState />
      ) : error ? (
        <ErrorState description={getApiErrorMessage(error)} />
      ) : (data?.items ?? []).length === 0 ? (
        <EmptyState title="Henüz rakam yok" description="Yeni rakam ekleyin." />
      ) : (
        <DataTable
          caption="Rakamlar"
          columns={columns}
          rows={data?.items ?? []}
          getRowKey={(row) => row.id}
          rowActions={(row) => (
            <>
              <Button type="button" tone="quiet" onClick={() => setEditingId(row.id)}>
                Düzenle
              </Button>
              <Button type="button" tone="quiet" onClick={() => toggleActiveMutation.mutate(row)} disabled={toggleActiveMutation.isPending}>
                {row.isActive ? 'Pasifleştir' : 'Etkinleştir'}
              </Button>
              <Button type="button" tone="danger" onClick={() => setDeletingId(row.id)}>
                Sil
              </Button>
            </>
          )}
        />
      )}
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}

      <ImpactMetricEditDialog
        key={editingId ?? 'closed'}
        open={editingId !== undefined}
        detail={editingId ? editingDetail ?? null : null}
        languages={languages}
        onSaveCore={(draft) => (editingId ? updateCoreMutation.mutate(draft) : createMutation.mutate(draft))}
        onSaveTranslation={(languageCode, draft) => updateTranslationMutation.mutate({ languageCode, draft })}
        onClose={() => setEditingId(undefined)}
      />
      <ConfirmDialog
        open={deletingId !== null}
        title="Rakamı sil"
        description="Bu rakam kalıcı olarak silinecek. Bu rakamı kullanan bir ana sayfa bloğu varsa bağlantısı kopar."
        isConfirming={deleteMutation.isPending}
        onConfirm={() => deletingId && deleteMutation.mutate(deletingId)}
        onCancel={() => setDeletingId(null)}
      />
    </div>
  );
}
