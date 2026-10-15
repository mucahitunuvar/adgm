import {
  discardHomeLayoutDraft,
  getApiErrorMessage,
  getBlockTypes,
  getHomeLayout,
  getHomeLayoutPreview,
  getSiteLanguages,
  publishHomeLayout,
  replaceHomeDraftBlocks,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  ErrorState,
  LoadingState,
  PageHeader,
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

export function HomeLayoutPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data: blockTypes } = useQuery({ queryKey: ['website-block-types'], queryFn: () => getBlockTypes(client) });
  const { data, isLoading, error, refetch } = useQuery({ queryKey: ['website-home-layout'], queryFn: () => getHomeLayout(client) });

  const [blocks, setBlocks] = useState<DraftBlock[] | null>(null);
  const [loadedRowVersion, setLoadedRowVersion] = useState<string | null>(null);
  const [newBlockKey, setNewBlockKey] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [isPublishing, setIsPublishing] = useState(false);
  const [isDiscardOpen, setIsDiscardOpen] = useState(false);
  const [previewBlocks, setPreviewBlocks] = useState<ReturnType<typeof summarizePreview> | null>(null);

  const languages = languagesData?.items ?? [];
  // Yalnız ETKİN diller için boş bir metin taslağı önceden oluşturulur - pasif bir dil için zorunlu
  // ("body is required" gibi) bir alanı boş bırakmak backend'i reddettirirdi; `languages` (sekmeler
  // için) tüm dilleri göstermeye devam eder, bu yalnız `newDraftBlock`'un önceden doldurduğu kümeyi
  // daraltır - yönetici pasif bir dilin sekmesine girip elle doldurursa o dil normal şekilde eklenir.
  const languageCodes = languages.filter((l) => l.isActive).map((l) => l.code);

  if (data && data.rowVersion !== loadedRowVersion) {
    setLoadedRowVersion(data.rowVersion);
    setBlocks(draftBlocksFromResponse(data.draftBlocks));
  }

  if (isLoading || !blocks) return <LoadingState />;
  if (error || !data) return <ErrorState description={error ? getApiErrorMessage(error) : 'Ana sayfa düzeni bulunamadı.'} />;

  const layout = data;
  const currentBlocks = blocks;
  const isDirty = JSON.stringify(draftBlocksToInput(currentBlocks)) !== JSON.stringify(draftBlocksToInput(draftBlocksFromResponse(layout.draftBlocks)));

  function blockTypeFor(key: string) {
    return blockTypes?.find((b) => b.key === key);
  }

  async function handleSaveDraft() {
    setIsSaving(true);
    try {
      const fresh = await getHomeLayout(client);
      if (fresh.rowVersion !== layout.rowVersion) {
        const proceed = window.confirm('Ana sayfa düzeni siz düzenlerken sunucuda değişmiş. Yeniden yükleyip değişikliklerinizi kaybetmek ister misiniz?');
        if (proceed) {
          setLoadedRowVersion(null);
          await refetch();
          setIsSaving(false);
          return;
        }
      }

      await replaceHomeDraftBlocks(client, { rowVersion: layout.rowVersion, blocks: draftBlocksToInput(currentBlocks) });
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
      await publishHomeLayout(client, layout.rowVersion);
      showToast('Ana sayfa düzeni yayınlandı.', 'success');
      setLoadedRowVersion(null);
      void queryClient.invalidateQueries({ queryKey: ['website-home-layout'] });
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    } finally {
      setIsPublishing(false);
    }
  }

  async function handleDiscardDraft() {
    try {
      await discardHomeLayoutDraft(client, layout.rowVersion);
      showToast('Taslak değişiklikler geri alındı.', 'success');
      setLoadedRowVersion(null);
      await refetch();
    } catch (mutationError) {
      showToast(getApiErrorMessage(mutationError), 'danger');
    }
  }

  async function handlePreview() {
    try {
      const response = await getHomeLayoutPreview(client, languages[0]?.code);
      setPreviewBlocks(summarizePreview(response.blocks));
    } catch (previewError) {
      showToast(getApiErrorMessage(previewError), 'danger');
    }
  }

  function addBlock() {
    const blockType = blockTypeFor(newBlockKey);
    if (!blockType) return;
    setBlocks((current) => [...(current ?? []), newDraftBlock(blockType, languageCodes)]);
    setNewBlockKey('');
  }

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title="Ana sayfa düzeni" />
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
              idPrefix={`home-block-${index}`}
              block={block}
              blockType={blockTypeFor(block.blockTypeKey)}
              languages={languages}
              onChange={(next) =>
                setBlocks((current) => (current ?? []).map((b, i) => (i === index ? next : b)))
              }
              onRemove={() => setBlocks((current) => (current ?? []).filter((_, i) => i !== index))}
            />
          )}
        />
      )}

      <div className="field-row">
        <SelectField id="home-new-block" label="Blok türü ekle" value={newBlockKey} onChange={(e) => setNewBlockKey(e.target.value)}>
          <option value="">Seçin</option>
          {(blockTypes ?? []).map((blockType) => (
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
        <Button type="button" tone="quiet" onClick={() => void handlePreview()}>
          Önizle
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

      {previewBlocks && (
        <section>
          <h2>Önizleme (yapısal özet)</h2>
          <p className="field-hint">
            Yönetimde blokların görsel çizimi yapılmıyor (Faz 3&apos;te public renderer var) - sıra, tür ve ana alanlar.
          </p>
          <ol>
            {previewBlocks.map((block, index) => (
              <li key={index}>
                <strong>{block.type}</strong>
                {block.summary && <span> — {block.summary}</span>}
              </li>
            ))}
          </ol>
        </section>
      )}

      <ConfirmDialog
        open={isDiscardOpen}
        title="Taslağı at"
        description="Taslak bloklar, yayındaki haliyle değiştirilecek. Kaydedilmemiş yerel değişiklikleriniz de kaybolur."
        onConfirm={() => {
          setIsDiscardOpen(false);
          void handleDiscardDraft();
        }}
        onCancel={() => setIsDiscardOpen(false)}
      />
    </div>
  );
}

function summarizePreview(blocks: readonly { type: string; settings: unknown; texts: unknown; data: unknown }[]) {
  return blocks.map((block) => {
    const mainText = (block.texts as Record<string, unknown> | null)?.title ?? (block.texts as Record<string, unknown> | null)?.body;
    const summary = typeof mainText === 'string' ? mainText.slice(0, 60) : null;
    return { type: block.type, summary };
  });
}

export default HomeLayoutPage;
