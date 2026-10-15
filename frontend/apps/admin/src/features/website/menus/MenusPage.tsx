import { getApiErrorMessage, getMenuByLocation, getSiteLanguages, replaceMenuItems } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  EmptyState,
  ErrorState,
  LoadingState,
  PageHeader,
  StatusBadge,
  Tabs,
  UnsavedChangesGuard,
  confirmDiscardChanges,
  useToast,
} from '@gm/ui';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { ArrowDown, ArrowUp } from 'lucide-react';
import { MenuItemEditDialog } from './MenuItemEditDialog';
import { draftItemsFromResponse, draftItemsToInput, getChildren, moveSibling, removeWithDescendants, type DraftMenuItem } from './menuTree';

const LOCATIONS = [
  { key: 'Header', label: 'Üst menü' },
  { key: 'Utility', label: 'Yardımcı menü' },
  { key: 'Footer', label: 'Alt menü (footer)' },
];

function newTempId(): string {
  return `new-${crypto.randomUUID()}`;
}

export function MenusPage() {
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [activeLocation, setActiveLocation] = useState(LOCATIONS[0]!.key);

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title="Menüler" description="Üst menü, yardımcı menü ve alt menü (footer) içeriği." />
      <Tabs
        items={LOCATIONS.map((location) => ({
          key: location.key,
          label: location.label,
          content: (
            <MenuLocationEditor
              location={location.key}
              onMutated={() => void queryClient.invalidateQueries({ queryKey: ['website-menu', location.key] })}
              showToast={showToast}
            />
          ),
        }))}
        activeKey={activeLocation}
        onChange={setActiveLocation}
      />
    </div>
  );
}

function MenuLocationEditor({
  location,
  onMutated,
  showToast,
}: {
  location: string;
  onMutated: () => void;
  showToast: (message: string, tone: 'success' | 'danger') => void;
}) {
  const { client } = useAuth();
  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['website-menu', location],
    queryFn: () => getMenuByLocation(client, location),
  });

  const [items, setItems] = useState<DraftMenuItem[] | null>(null);
  const [loadedRowVersion, setLoadedRowVersion] = useState<string | null>(null);
  const [editingTempId, setEditingTempId] = useState<string | null>(null);
  const [deletingTempId, setDeletingTempId] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const languages = languagesData?.items ?? [];

  // Sunucudan yeni veri gelince (ilk yükleme veya kaydet/yeniden yükle sonrası) taslağı sıfırlar -
  // render sırasında karşılaştırma, `useEffect` içinde senkron `setState` yerine.
  if (data && data.rowVersion !== loadedRowVersion) {
    setLoadedRowVersion(data.rowVersion);
    setItems(draftItemsFromResponse(data.items));
  }

  if (isLoading || !items) return <LoadingState />;
  if (error) return <ErrorState description={getApiErrorMessage(error)} />;

  // `items`'ı yeni bir `const`'a bağlar: TS'in null-daraltması iç içe fonksiyon bildirimlerinin
  // (aşağıdaki `addItem`/`renderNode` vb.) gövdesine taşınmıyor, ayrı bir sabit gerekiyor.
  const currentItems = items;
  const isDirty =
    JSON.stringify(draftItemsToInput(currentItems)) !== JSON.stringify(draftItemsToInput(draftItemsFromResponse(data!.items)));

  function updateItem(next: DraftMenuItem) {
    setItems((current) => (current ?? []).map((item) => (item.tempId === next.tempId ? next : item)));
  }

  function addItem(parentTempId: string | null) {
    const siblingCount = getChildren(currentItems, parentTempId).length;
    const draft: DraftMenuItem = {
      tempId: newTempId(),
      parentTempId,
      sortOrder: siblingCount,
      isActive: true,
      link: null,
      openInNewTab: false,
      iconKey: null,
      translations: [],
    };
    setItems((current) => [...(current ?? []), draft]);
    setEditingTempId(draft.tempId);
  }

  async function handleSave() {
    if (!data) return;
    setIsSaving(true);
    try {
      // Kaydetmeden önce sunucudaki güncel listeyi yeniden oku - başka biri bu arada değiştirmiş
      // olabilir (409'u BEKLEMEDEN olası farkı önceden haber ver).
      const fresh = await getMenuByLocation(client, location);
      if (fresh.rowVersion !== data.rowVersion) {
        const proceed = window.confirm(
          'Bu menü siz düzenlerken sunucuda değişmiş. Listeyi yeniden yükleyip değişikliklerinizi kaybetmek ister misiniz? ' +
            '(Vazgeç seçilirse kaydetmeyi deneyecek, sunucu çakışmayı 409 ile reddedebilir.)',
        );
        if (proceed) {
          setLoadedRowVersion(null);
          await refetch();
          setIsSaving(false);
          return;
        }
      }

      await replaceMenuItems(client, location, { rowVersion: data.rowVersion, items: draftItemsToInput(currentItems) });
      showToast('Menü kaydedildi.', 'success');
      setLoadedRowVersion(null);
      onMutated();
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

  function renderNode(item: DraftMenuItem, depth: number) {
    const siblings = getChildren(currentItems, item.parentTempId);
    const indexInSiblings = siblings.findIndex((s) => s.tempId === item.tempId);
    const label = item.translations[0]?.label || '(etiketsiz)';
    const children = getChildren(currentItems, item.tempId);

    return (
      <li key={item.tempId} className="tree-list__item" role="treeitem" aria-expanded={children.length > 0 ? true : undefined}>
        <div className="tree-list__row" style={{ paddingLeft: `${depth * 1.5}rem` }}>
          <strong>{label}</strong>
          <StatusBadge tone={item.isActive ? 'success' : 'neutral'}>{item.isActive ? 'Etkin' : 'Pasif'}</StatusBadge>
          {item.brokenLinkReason && <StatusBadge tone="warning">{item.brokenLinkReason}</StatusBadge>}
          <button
            type="button"
            className="reorder-list__button"
            aria-label={`${label} öğesini yukarı taşı`}
            disabled={indexInSiblings === 0}
            onClick={() => setItems(moveSibling(currentItems, item.tempId, -1))}
          >
            <ArrowUp aria-hidden="true" size={16} />
          </button>
          <button
            type="button"
            className="reorder-list__button"
            aria-label={`${label} öğesini aşağı taşı`}
            disabled={indexInSiblings === siblings.length - 1}
            onClick={() => setItems(moveSibling(currentItems, item.tempId, 1))}
          >
            <ArrowDown aria-hidden="true" size={16} />
          </button>
          <Button type="button" tone="quiet" onClick={() => setEditingTempId(item.tempId)}>
            Düzenle
          </Button>
          <Button type="button" tone="quiet" onClick={() => addItem(item.tempId)}>
            Alt öğe ekle
          </Button>
          <Button type="button" tone="danger" onClick={() => setDeletingTempId(item.tempId)}>
            Sil
          </Button>
        </div>
        {children.length > 0 && (
          <ul className="tree-list__children" role="group">
            {children.map((child) => renderNode(child, depth + 1))}
          </ul>
        )}
      </li>
    );
  }

  const roots = getChildren(currentItems, null);
  const editingItem = editingTempId ? currentItems.find((item) => item.tempId === editingTempId) ?? null : null;

  return (
    <div>
      <UnsavedChangesGuard isDirty={isDirty} />
      {roots.length === 0 ? (
        <EmptyState title="Bu menüde henüz öğe yok" description="Kök öğe ekleyin." />
      ) : (
        <ul className="tree-list" role="tree">
          {roots.map((item) => renderNode(item, 0))}
        </ul>
      )}
      <div className="field-row">
        <Button type="button" tone="secondary" onClick={() => addItem(null)}>
          Kök öğe ekle
        </Button>
        <Button type="button" onClick={() => void handleSave()} disabled={!isDirty || isSaving}>
          {isSaving ? 'Kaydediliyor…' : 'Kaydet'}
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

      <MenuItemEditDialog
        key={editingTempId ?? 'closed'}
        open={editingTempId !== null}
        item={editingItem}
        languages={languages}
        onSave={(next) => {
          updateItem(next);
          setEditingTempId(null);
        }}
        onCancel={() => setEditingTempId(null)}
      />
      <ConfirmDialog
        open={deletingTempId !== null}
        title="Menü öğesini sil"
        description="Bu öğe ve varsa alt öğeleri taslaktan kaldırılacak. Kaydetmeden listeyi terk ederseniz bu değişiklik uygulanmaz."
        onConfirm={() => {
          if (deletingTempId) setItems(removeWithDescendants(currentItems, deletingTempId));
          setDeletingTempId(null);
        }}
        onCancel={() => setDeletingTempId(null)}
      />
    </div>
  );
}
