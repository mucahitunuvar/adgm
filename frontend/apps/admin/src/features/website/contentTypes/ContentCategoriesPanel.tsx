import {
  activateContentCategory,
  createContentCategory,
  deactivateContentCategory,
  deleteContentCategory,
  getApiErrorMessage,
  getContentCategoriesByType,
  updateContentCategory,
  type ContentCategoryTreeItemResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, EmptyState, ErrorState, LoadingState, StatusBadge, TextField, TreeList, type TreeListNode, useToast } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { CreateCategoryDialog, type CreateCategoryDialogInput } from './CreateCategoryDialog';

function toTreeNodes(items: readonly ContentCategoryTreeItemResponse[]): Array<TreeListNode<ContentCategoryTreeItemResponse>> {
  return items.map((item) => ({ id: item.id, data: item, children: toTreeNodes(item.children) }));
}

export function ContentCategoriesPanel({ contentTypeId }: { contentTypeId: string }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const queryKey = ['website-content-categories', contentTypeId] as const;
  const [createParentId, setCreateParentId] = useState<string | null | undefined>(undefined);

  const { data, isLoading, error } = useQuery({ queryKey, queryFn: () => getContentCategoriesByType(client, contentTypeId) });

  function onMutated(message: string) {
    showToast(message, 'success');
    void queryClient.invalidateQueries({ queryKey });
  }

  function onMutationError(mutationError: unknown) {
    showToast(getApiErrorMessage(mutationError), 'danger');
  }

  const createMutation = useMutation({
    mutationFn: (input: CreateCategoryDialogInput) =>
      createContentCategory(client, contentTypeId, {
        parentId: createParentId ?? null,
        sortOrder: input.sortOrder,
        defaultLanguageName: input.defaultLanguageName,
        defaultLanguageSlug: input.defaultLanguageSlug,
        seo: {
          metaTitle: null,
          metaDescription: null,
          metaKeywords: null,
          ogTitle: null,
          ogDescription: null,
          ogImageMediaId: null,
          canonicalUrl: null,
          noIndex: false,
        },
      }),
    onSuccess: () => {
      onMutated('Kategori oluşturuldu.');
      setCreateParentId(undefined);
    },
    onError: onMutationError,
  });

  if (isLoading) return <LoadingState />;
  if (error) return <ErrorState description={getApiErrorMessage(error)} />;

  const nodes = toTreeNodes(data ?? []);

  return (
    <div>
      <p className="field-hint">
        Çeviri ve SEO alanları düzenlemesi şu an desteklenmiyor: backend kategori için çeviri bazlı okuma ucu (
        <code>GetContentCategoryById</code> benzeri) yayınlamıyor; yalnız yazma (PUT) ucu var. Körlemesine yazmak var olan
        SEO verisinin üzerine boş değer yazabileceğinden bu ekrana eklenmedi (bkz. tamamlama raporu, madde a).
      </p>
      {nodes.length === 0 && <EmptyState title="Henüz kategori yok" description="Bu içerik tipi için kök kategori ekleyin." />}
      <TreeList
        nodes={nodes}
        renderNode={(category) => (
          <CategoryRow
            contentTypeId={contentTypeId}
            category={category}
            onAddChild={() => setCreateParentId(category.id)}
            onMutated={onMutated}
            onMutationError={onMutationError}
          />
        )}
      />
      <Button type="button" tone="secondary" onClick={() => setCreateParentId(null)}>
        Kök kategori ekle
      </Button>
      <CreateCategoryDialog
        key={createParentId === undefined ? 'closed' : createParentId === null ? 'root' : createParentId}
        parentId={createParentId}
        isSubmitting={createMutation.isPending}
        onConfirm={(input) => createMutation.mutate(input)}
        onCancel={() => setCreateParentId(undefined)}
      />
    </div>
  );
}

function CategoryRow({
  contentTypeId,
  category,
  onAddChild,
  onMutated,
  onMutationError,
}: {
  contentTypeId: string;
  category: ContentCategoryTreeItemResponse;
  onAddChild: () => void;
  onMutated: (message: string) => void;
  onMutationError: (error: unknown) => void;
}) {
  const { client } = useAuth();
  const [sortOrder, setSortOrder] = useState(Number(category.sortOrder));
  const isSortDirty = sortOrder !== Number(category.sortOrder);

  const saveSortMutation = useMutation({
    mutationFn: () => updateContentCategory(client, contentTypeId, category.id, { rowVersion: category.rowVersion, sortOrder }),
    onSuccess: () => onMutated('Sıra güncellendi.'),
    onError: onMutationError,
  });
  const activateMutation = useMutation({
    mutationFn: () => activateContentCategory(client, contentTypeId, category.id, { rowVersion: category.rowVersion }),
    onSuccess: () => onMutated('Kategori etkinleştirildi.'),
    onError: onMutationError,
  });
  const deactivateMutation = useMutation({
    mutationFn: () => deactivateContentCategory(client, contentTypeId, category.id, { rowVersion: category.rowVersion }),
    onSuccess: () => onMutated('Kategori pasifleştirildi.'),
    onError: onMutationError,
  });
  const deleteMutation = useMutation({
    mutationFn: () => deleteContentCategory(client, contentTypeId, category.id),
    onSuccess: () => onMutated('Kategori silindi.'),
    onError: onMutationError,
  });

  const canDelete = category.children.length === 0 && Number(category.assignedContentCount) === 0;

  return (
    <div className="field-row" style={{ alignItems: 'center' }}>
      <strong>{category.name}</strong>
      <span className="field-hint">/{category.slug}</span>
      <StatusBadge tone={category.isActive ? 'success' : 'neutral'}>{category.isActive ? 'Etkin' : 'Pasif'}</StatusBadge>
      <span className="field-hint">{category.assignedContentCount} içerik</span>
      <TextField
        id={`category-sort-${category.id}`}
        label="Sıra"
        type="number"
        value={sortOrder}
        onChange={(e) => setSortOrder(Number(e.target.value))}
      />
      <Button type="button" tone="quiet" onClick={() => saveSortMutation.mutate()} disabled={!isSortDirty || saveSortMutation.isPending}>
        Sırayı kaydet
      </Button>
      {category.isActive ? (
        <Button type="button" tone="quiet" onClick={() => deactivateMutation.mutate()} disabled={deactivateMutation.isPending}>
          Pasifleştir
        </Button>
      ) : (
        <Button type="button" tone="quiet" onClick={() => activateMutation.mutate()} disabled={activateMutation.isPending}>
          Etkinleştir
        </Button>
      )}
      <Button type="button" tone="quiet" onClick={onAddChild}>
        Alt kategori ekle
      </Button>
      <Button
        type="button"
        tone="danger"
        onClick={() => deleteMutation.mutate()}
        disabled={!canDelete || deleteMutation.isPending}
        title={canDelete ? undefined : 'Alt kategorisi veya atanmış içeriği olan kategori silinemez.'}
      >
        Sil
      </Button>
    </div>
  );
}
