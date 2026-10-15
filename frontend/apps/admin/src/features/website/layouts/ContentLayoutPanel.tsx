import {
  discardContentLayoutDraft,
  getApiErrorMessage,
  getBlockTypes,
  getContentLayout,
  getSiteLanguages,
  publishContentLayout,
  replaceContentDraftBlocks,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  ErrorState,
  LoadingState,
  ReorderList,
  SelectField,
  StatusBadge,
  UnsavedChangesGuard,
  confirmDiscardChanges,
  formatDateTime,
  useToast,
} from '@gm/ui';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { LayoutBlockEditor } from './LayoutBlockEditor';
import { draftBlocksFromResponse, draftBlocksToInput, newDraftBlock, type DraftBlock } from './layoutTypes';

/**
 * `layouts/content/{id}` - `HomeLayoutPage`'le AYNI motor (`LayoutBlockEditor`/`BlockFieldRenderer`),
 * farkı: backend'in içerik düzeni için ayrı bir önizleme ucu YOK (yalnız ana sayfanın var) - bu yüzden
 * "Önizle" düğmesi burada yok, taslak blok listesinin kendisi zaten tek görünür hal.
 */
export function ContentLayoutPanel({ contentItemId }: { contentItemId: string }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data: blockTypes } = useQuery({ queryKey: ['website-block-types'], queryFn: () => getBlockTypes(client) });
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['website-content-layout', contentItemId],
    queryFn: () => getContentLayout(client, contentItemId),
  });

  const [blocks, setBlocks] = useState<DraftBlock[] | null>(null);
  const [loadedRowVersion, setLoadedRowVersion] = useState<string | null>(null);
  const [newBlockKey, setNewBlockKey] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [isPublishing, setIsPublishing] = useState(false);
  const [isDiscardOpen, setIsDiscardOpen] = useState(false);

  const languages = languagesData?.items ?? [];
  // Bkz. HomeLayoutPage.tsx'teki aynı not: yalnız etkin diller için boş metin taslağı önceden oluşturulur.
  const languageCodes = languages.filter((l) => l.isActive).map((l) => l.code);

  if (data && data.rowVersion !== loadedRowVersion) {
    setLoadedRowVersion(data.rowVersion);
    setBlocks(draftBlocksFromResponse(data.draftBlocks));
  }

  if (isLoading || !blocks) return <LoadingState />;
  if (error || !data) return <ErrorState description={error ? getApiErrorMessage(error) : 'Düzen bulunamadı.'} />;

  const layout = data;
  const currentBlocks = blocks;
  const isDirty =
    JSON.stringify(draftBlocksToInput(currentBlocks)) !== JSON.stringify(draftBlocksToInput(draftBlocksFromResponse(layout.draftBlocks)));

  function blockTypeFor(key: string) {
    return blockTypes?.find((b) => b.key === key);
  }

  async function handleSaveDraft() {
    setIsSaving(true);
    try {
      const fresh = await getContentLayout(client, contentItemId);
      if (fresh.rowVersion !== layout.rowVersion) {
        const proceed = window.confirm('Bu düzen siz düzenlerken sunucuda değişmiş. Yeniden yükleyip değişikliklerinizi kaybetmek ister misiniz?');
        if (proceed) {
          setLoadedRowVersion(null);
          await refetch();
          setIsSaving(false);
          return;
        }
      }

      await replaceContentDraftBlocks(client, contentItemId, { rowVersion: layout.rowVersion, blocks: draftBlocksToInput(currentBlocks) });
      showToast('Taslak kaydedildi.', 'success');
      setLoadedRowVersion(null);
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
      if ((mutationError as { status?: number }).status === 409) {
        setLoadedRowVersion(null);
        await refetch();
      }
    } finally {
      setIsSaving(false);
    }
  }

  async function handlePublish() {
    setIsPublishing(true);
    try {
      await publishContentLayout(client, contentItemId, layout.rowVersion);
      showToast('Düzen yayınlandı.', 'success');
      setLoadedRowVersion(null);
      void queryClient.invalidateQueries({ queryKey: ['website-content-layout', contentItemId] });
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    } finally {
      setIsPublishing(false);
    }
  }

  async function handleDiscardDraft() {
    try {
      await discardContentLayoutDraft(client, contentItemId, layout.rowVersion);
      showToast('Taslak değişiklikler geri alındı.', 'success');
      setLoadedRowVersion(null);
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    }
  }

  function addBlock() {
    const blockType = blockTypeFor(newBlockKey);
    if (!blockType) return;
    setBlocks((current) => [...(current ?? []), newDraftBlock(blockType, languageCodes)]);
    setNewBlockKey('');
  }

  return (
    <div>
      <UnsavedChangesGuard isDirty={isDirty} />
      <div className="field-row" style={{ alignItems: 'center' }}>
        <StatusBadge tone={layout.hasUnpublishedChanges ? 'warning' : 'success'}>
          {layout.hasUnpublishedChanges ? 'Yayınlanmamış değişiklikler var' : 'Taslak = yayındaki hal'}
        </StatusBadge>
        {layout.publishedAtUtc && <span className="field-hint">{`Son yayın: ${formatDateTime(layout.publishedAtUtc)}`}</span>}
      </div>

      {currentBlocks.length === 0 ? (
        <p className="field-hint">Henüz blok yok.</p>
      ) : (
        <ReorderList
          items={currentBlocks}
          getKey={(block) => block.tempKey}
          getLabel={(block) => block.blockTypeKey}
          onMove={(fromIndex, toIndex) =>
            setBlocks((current) => {
              const list = [...(current ?? [])];
              const [moved] = list.splice(fromIndex, 1);
              list.splice(toIndex, 0, moved!);
              return list;
            })
          }
          renderItem={(block, index) => (
            <LayoutBlockEditor
              idPrefix={`content-block-${index}`}
              block={block}
              blockType={blockTypeFor(block.blockTypeKey)}
              languages={languages}
              onChange={(next) => setBlocks((current) => (current ?? []).map((b, i) => (i === index ? next : b)))}
              onRemove={() => setBlocks((current) => (current ?? []).filter((_, i) => i !== index))}
            />
          )}
        />
      )}

      <div className="field-row">
        <SelectField id="content-new-block" label="Blok türü ekle" value={newBlockKey} onChange={(e) => setNewBlockKey(e.target.value)}>
          <option value="">Seçin</option>
          {(blockTypes ?? [])
            .filter((blockType) => blockType.allowedTargets.includes('Content'))
            .map((blockType) => (
              <option key={blockType.key} value={blockType.key}>
                {blockType.key}
              </option>
            ))}
        </SelectField>
        <Button type="button" tone="secondary" onClick={addBlock} disabled={!newBlockKey}>
          Blok ekle
        </Button>
      </div>

      <div className="field-row">
        <Button type="button" onClick={() => void handleSaveDraft()} disabled={!isDirty || isSaving}>
          {isSaving ? 'Kaydediliyor…' : 'Taslağı kaydet'}
        </Button>
        <Button type="button" tone="secondary" onClick={() => void handlePublish()} disabled={isPublishing}>
          {isPublishing ? 'Yayınlanıyor…' : 'Yayınla'}
        </Button>
        <Button type="button" tone="quiet" onClick={() => setIsDiscardOpen(true)} disabled={!layout.hasUnpublishedChanges}>
          Taslağı at
        </Button>
        <Button
          type="button"
          tone="quiet"
          onClick={() => {
            if (!confirmDiscardChanges(isDirty)) return;
            setLoadedRowVersion(null);
            void refetch();
          }}
        >
          Değişiklikleri geri al
        </Button>
      </div>

      <ConfirmDialog
        open={isDiscardOpen}
        title="Taslağı at"
        description="Taslak bloklar, yayındaki haliyle değiştirilecek."
        onConfirm={() => {
          setIsDiscardOpen(false);
          void handleDiscardDraft();
        }}
        onCancel={() => setIsDiscardOpen(false)}
      />
    </div>
  );
}
