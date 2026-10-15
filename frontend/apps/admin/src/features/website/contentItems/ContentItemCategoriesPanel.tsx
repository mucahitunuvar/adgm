import {
  getApiErrorMessage,
  getContentCategoriesByType,
  setContentItemCategories,
  type ContentCategoryTreeItemResponse,
  type ContentItemDetailResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, CheckboxField, EmptyState, ErrorState, LoadingState, TreeList, useToast, type TreeListNode } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';

function toTreeNodes(items: readonly ContentCategoryTreeItemResponse[]): Array<TreeListNode<ContentCategoryTreeItemResponse>> {
  return items.map((item) => ({ id: item.id, data: item, children: toTreeNodes(item.children) }));
}

export function ContentItemCategoriesPanel({
  contentItemId,
  contentTypeId,
  detail,
  onSaved,
}: {
  contentItemId: string;
  contentTypeId: string;
  detail: ContentItemDetailResponse;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const { data, isLoading, error } = useQuery({
    queryKey: ['website-content-categories', contentTypeId],
    queryFn: () => getContentCategoriesByType(client, contentTypeId),
  });

  const [selectedIds, setSelectedIds] = useState<string[]>([...detail.categoryIds]);
  const isDirty = JSON.stringify([...selectedIds].sort()) !== JSON.stringify([...detail.categoryIds].sort());

  function toggle(id: string) {
    setSelectedIds((current) => (current.includes(id) ? current.filter((x) => x !== id) : [...current, id]));
  }

  async function handleSave() {
    try {
      await setContentItemCategories(client, contentItemId, { rowVersion: detail.rowVersion, categoryIds: selectedIds });
      showToast('Kategoriler güncellendi.', 'success');
      onSaved();
    } catch (saveError) {
      showToast(getApiErrorMessage(saveError), 'danger');
    }
  }

  if (isLoading) return <LoadingState />;
  if (error) return <ErrorState description={getApiErrorMessage(error)} />;

  const nodes = toTreeNodes(data ?? []);
  if (nodes.length === 0) {
    return <EmptyState title="Henüz kategori yok" description="Bu içerik tipi için kategori tanımlanmamış." />;
  }

  return (
    <div>
      <TreeList
        nodes={nodes}
        renderNode={(category) => (
          <CheckboxField
            id={`ci-category-${category.id}`}
            label={category.name}
            checked={selectedIds.includes(category.id)}
            onChange={() => toggle(category.id)}
          />
        )}
      />
      <Button type="button" onClick={() => void handleSave()} disabled={!isDirty}>
        Kaydet
      </Button>
    </div>
  );
}
