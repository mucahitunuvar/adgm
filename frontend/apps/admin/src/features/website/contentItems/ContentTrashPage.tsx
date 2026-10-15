import {
  getApiErrorMessage,
  getContentTrash,
  permanentlyDeleteContentItem,
  restoreContentItem,
  type ContentItemTrashSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, ConfirmDialog, DataTable, Pagination, PageHeader, buttonClassName, formatDateTime, useToast, type DataTableColumn } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../../routes';

const PAGE_SIZE = 20;

/** `ContentItem.TrashRetentionDays = 30`: `eligibleForPermanentDeletionAtUtc`'den önce de kalıcı silinebilir (Admin yetkisiyle). */
export function ContentTrashPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const queryKey = ['website-content-trash', page] as const;
  const { data, isLoading, error } = useQuery({ queryKey, queryFn: () => getContentTrash(client, { page, pageSize: PAGE_SIZE }) });

  function onMutated(message: string) {
    showToast(message, 'success');
    void queryClient.invalidateQueries({ queryKey });
  }

  const restoreMutation = useMutation({
    mutationFn: (id: string) => restoreContentItem(client, id),
    onSuccess: () => onMutated('İçerik geri alındı.'),
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => permanentlyDeleteContentItem(client, id),
    onSuccess: () => {
      onMutated('İçerik kalıcı olarak silindi.');
      setDeletingId(null);
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const columns: DataTableColumn<ContentItemTrashSummaryResponse>[] = [
    { key: 'title', header: 'Başlık', render: (row) => row.title },
    { key: 'deletedAtUtc', header: 'Silinme tarihi', render: (row) => formatDateTime(row.deletedAtUtc) },
    { key: 'eligibleForPermanentDeletionAtUtc', header: 'Kalıcı silme için uygun', render: (row) => formatDateTime(row.eligibleForPermanentDeletionAtUtc) },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="İçerik çöp kutusu"
        description="Çöp kutusundaki içerikler 30 gün tutulur; bu süre dolmadan da kalıcı silinebilir."
        actions={
          <Link to={ROUTES.websiteContentItems} className={buttonClassName('quiet')}>
            İçeriklere dön
          </Link>
        }
      />
      <DataTable
        caption="Çöp kutusu"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <>
            <Button type="button" tone="quiet" onClick={() => restoreMutation.mutate(row.id)} disabled={restoreMutation.isPending}>
              Geri al
            </Button>{' '}
            <Button type="button" tone="danger" onClick={() => setDeletingId(row.id)}>
              Kalıcı sil
            </Button>
          </>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
      <ConfirmDialog
        open={deletingId !== null}
        title="İçeriği kalıcı olarak sil"
        description="Bu işlem geri alınamaz. İçerik ve tüm bağlı verileri kalıcı olarak silinecek."
        confirmLabel="Kalıcı sil"
        isConfirming={deleteMutation.isPending}
        onConfirm={() => deleteMutation.mutate(deletingId!)}
        onCancel={() => setDeletingId(null)}
      />
    </div>
  );
}
