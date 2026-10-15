import {
  getApiErrorMessage,
  getContentItemById,
  getContentItems,
  setContentItemParent,
  type ContentItemDetailResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, LoadingState, useToast, type DataTableColumn } from '@gm/ui';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../../routes';
import { useContentItemPicker } from './useContentItemPicker';

export function ContentItemHierarchyPanel({
  contentItemId,
  detail,
  onSaved,
}: {
  contentItemId: string;
  detail: ContentItemDetailResponse;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const { pickContentItem, pickerElement } = useContentItemPicker(contentItemId);

  const { data: parent, isLoading: isParentLoading } = useQuery({
    queryKey: ['website-content-item', detail.parentId],
    queryFn: () => getContentItemById(client, detail.parentId!),
    enabled: Boolean(detail.parentId),
  });

  const { data: children, isLoading: areChildrenLoading, error: childrenError } = useQuery({
    queryKey: ['website-content-items', { parentId: contentItemId }],
    queryFn: () => getContentItems(client, { parentId: contentItemId, pageSize: 100 }),
  });

  const setParentMutation = useMutation({
    mutationFn: (parentId: string | null) => setContentItemParent(client, contentItemId, { rowVersion: detail.rowVersion, parentId }),
    onSuccess: () => {
      showToast('Üst içerik güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  async function handleChangeParent() {
    const picked = await pickContentItem();
    if (!picked) return;
    setParentMutation.mutate(picked.id);
  }

  const columns: DataTableColumn<{ id: string; title: string }>[] = [{ key: 'title', header: 'Başlık', render: (row) => row.title }];

  return (
    <div>
      <div className="field-row">
        <strong>Üst içerik:</strong>{' '}
        {detail.parentId ? (
          isParentLoading ? (
            <LoadingState />
          ) : (
            <Link to={ROUTES.websiteContentItemDetail(detail.parentId)}>{parent?.translations[0]?.title ?? detail.parentId}</Link>
          )
        ) : (
          <span>Yok (kök seviye)</span>
        )}
        <Button type="button" tone="quiet" onClick={() => void handleChangeParent()} disabled={setParentMutation.isPending}>
          Değiştir
        </Button>
        {detail.parentId && (
          <Button type="button" tone="quiet" onClick={() => setParentMutation.mutate(null)} disabled={setParentMutation.isPending}>
            Kök seviyeye taşı
          </Button>
        )}
      </div>
      <h3>Alt içerikler</h3>
      <DataTable
        caption="Alt içerikler"
        columns={columns}
        rows={children?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={areChildrenLoading}
        error={childrenError ? getApiErrorMessage(childrenError) : undefined}
        rowActions={(row) => (
          <Link to={ROUTES.websiteContentItemDetail(row.id)} className="button button--quiet">
            Görüntüle
          </Link>
        )}
      />
      {pickerElement}
    </div>
  );
}
