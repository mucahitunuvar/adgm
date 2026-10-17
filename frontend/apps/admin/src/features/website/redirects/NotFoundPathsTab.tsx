import { convertNotFoundPathToRedirect, deleteNotFoundPath, getApiErrorMessage, getNotFoundPaths, type NotFoundLogResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, ConfirmDialog, DataTable, formatDateTime, Pagination, useToast, type DataTableColumn } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import type { RedirectTargetValue } from '../shared/RedirectTargetFields';
import { ConvertNotFoundPathDialog } from './ConvertNotFoundPathDialog';

const PAGE_SIZE = 50;

export function NotFoundPathsTab() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [converting, setConverting] = useState<NotFoundLogResponse | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-not-found-paths', page],
    queryFn: () => getNotFoundPaths(client, { page, pageSize: PAGE_SIZE }),
  });

  function invalidate() {
    void queryClient.invalidateQueries({ queryKey: ['website-not-found-paths'] });
    void queryClient.invalidateQueries({ queryKey: ['website-redirects'] });
  }

  const convertMutation = useMutation({
    mutationFn: (target: RedirectTargetValue) => {
      if (!converting) throw new Error('Henüz yüklenmedi.');
      return convertNotFoundPathToRedirect(client, converting.id, {
        targetKind: target.targetKind,
        targetContentItemId: target.targetContentItemId,
        targetPath: target.targetPath,
        statusCode: target.statusCode,
      });
    },
    onSuccess: () => {
      showToast('Yönlendirmeye çevrildi.', 'success');
      setConverting(null);
      invalidate();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteNotFoundPath(client, id),
    onSuccess: () => {
      showToast('Kayıt silindi.', 'success');
      setDeletingId(null);
      invalidate();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const columns: DataTableColumn<NotFoundLogResponse>[] = [
    { key: 'path', header: 'Yol', render: (row) => `/${row.path}` },
    { key: 'languageCode', header: 'Dil', render: (row) => row.languageCode },
    { key: 'hitCount', header: 'İsabet', render: (row) => row.hitCount, align: 'end' },
    { key: 'firstSeenAtUtc', header: 'İlk görülme', render: (row) => formatDateTime(row.firstSeenAtUtc) },
    { key: 'lastSeenAtUtc', header: 'Son görülme', render: (row) => formatDateTime(row.lastSeenAtUtc) },
  ];

  return (
    <div>
      <p className="field-hint">En sık 404 alan yollar üstte listelenir.</p>
      <DataTable
        caption="404 yolları"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        emptyTitle="404 kaydı yok"
        emptyDescription="Henüz bulunamayan bir yol kaydedilmedi."
        rowActions={(row) => (
          <>
            <Button type="button" tone="quiet" onClick={() => setConverting(row)}>
              Yönlendirmeye çevir
            </Button>
            <Button type="button" tone="danger" onClick={() => setDeletingId(row.id)}>
              Yoksay/sil
            </Button>
          </>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}

      <ConvertNotFoundPathDialog
        open={converting !== null}
        path={converting}
        isSubmitting={convertMutation.isPending}
        onConfirm={(target) => convertMutation.mutate(target)}
        onCancel={() => setConverting(null)}
      />
      <ConfirmDialog
        open={deletingId !== null}
        title="404 kaydını sil"
        description="Bu kayıt listeden kaldırılacak; yol yine bulunamazsa yeniden loglanır."
        isConfirming={deleteMutation.isPending}
        onConfirm={() => deletingId && deleteMutation.mutate(deletingId)}
        onCancel={() => setDeletingId(null)}
      />
    </div>
  );
}
