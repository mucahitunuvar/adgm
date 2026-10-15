import { getApiErrorMessage, getContentItemById, setContentItemRelatedContent, type ContentItemDetailResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, ErrorState, LoadingState, useToast } from '@gm/ui';
import { useMutation, useQueries } from '@tanstack/react-query';
import { useState } from 'react';
import { useContentItemPicker } from './useContentItemPicker';

const MAX_RELATED_CONTENT = 12;

export function ContentItemRelatedContentPanel({
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
  const [relatedIds, setRelatedIds] = useState<string[]>([...detail.relatedContentItemIds]);
  const isDirty = JSON.stringify(relatedIds) !== JSON.stringify(detail.relatedContentItemIds);

  const relatedQueries = useQueries({
    queries: relatedIds.map((id) => ({ queryKey: ['website-content-item', id], queryFn: () => getContentItemById(client, id) })),
  });

  const saveMutation = useMutation({
    mutationFn: () => setContentItemRelatedContent(client, contentItemId, { rowVersion: detail.rowVersion, relatedContentItemIds: relatedIds }),
    onSuccess: () => {
      showToast('İlişkili içerikler güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  async function handleAdd() {
    const picked = await pickContentItem();
    if (!picked || relatedIds.includes(picked.id)) return;
    setRelatedIds([...relatedIds, picked.id]);
  }

  function handleRemove(id: string) {
    setRelatedIds(relatedIds.filter((x) => x !== id));
  }

  return (
    <div>
      <ul className="field-list">
        {relatedIds.map((id, index) => {
          const query = relatedQueries[index];
          if (query?.isLoading) return <li key={id}><LoadingState /></li>;
          if (query?.error) return <li key={id}><ErrorState description={getApiErrorMessage(query.error)} /></li>;
          return (
            <li key={id} className="field-row">
              <span>{query?.data?.translations[0]?.title ?? id}</span>
              <Button type="button" tone="quiet" onClick={() => handleRemove(id)}>
                Kaldır
              </Button>
            </li>
          );
        })}
      </ul>
      <Button type="button" tone="secondary" onClick={() => void handleAdd()} disabled={relatedIds.length >= MAX_RELATED_CONTENT}>
        İçerik ekle
      </Button>{' '}
      <Button type="button" onClick={() => saveMutation.mutate()} disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
      {pickerElement}
    </div>
  );
}
