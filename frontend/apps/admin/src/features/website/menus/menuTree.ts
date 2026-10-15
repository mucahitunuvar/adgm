import type { MenuItemResponse, MenuItemTreeInput } from '@gm/api-client';
import type { LinkTargetValue } from '../shared/LinkTargetField';

/**
 * Sunucudaki gerçek `id`'yi `tempId` olarak taşır - zaten var olan bir öğe için backend'in
 * `ReplaceMenuItemsRequest`'i (`MenuItemTreeInput`) yeni/var olan ayrımı yapmaz, her öğe kendi
 * `tempId`'siyle (gerçek veya istemci tarafında üretilmiş) ağaçtaki yerini belirtir.
 */
export interface DraftMenuItem {
  tempId: string;
  parentTempId: string | null;
  sortOrder: number;
  isActive: boolean;
  link: LinkTargetValue | null;
  openInNewTab: boolean;
  iconKey: string | null;
  translations: Array<{ languageCode: string; label: string }>;
  /** Yalnız bilgi amaçlı (sunucudan) - kaydedilen veriye dahil değil. */
  brokenLinkReason?: string | null;
}

export function draftItemsFromResponse(items: readonly MenuItemResponse[]): DraftMenuItem[] {
  return items.map((item) => ({
    tempId: item.id,
    parentTempId: item.parentId,
    sortOrder: Number(item.sortOrder),
    isActive: item.isActive,
    link: item.link,
    openInNewTab: item.openInNewTab,
    iconKey: item.iconKey,
    translations: item.translations.map((t) => ({ languageCode: t.languageCode, label: t.label })),
    brokenLinkReason: item.brokenLinkReason,
  }));
}

export function draftItemsToInput(items: readonly DraftMenuItem[]): MenuItemTreeInput[] {
  return items.map((item) => ({
    tempId: item.tempId,
    parentTempId: item.parentTempId,
    sortOrder: item.sortOrder,
    isActive: item.isActive,
    link: item.link,
    openInNewTab: item.openInNewTab,
    iconKey: item.iconKey,
    translations: item.translations,
  }));
}

export function getChildren(items: readonly DraftMenuItem[], parentTempId: string | null): DraftMenuItem[] {
  return items.filter((item) => item.parentTempId === parentTempId).sort((a, b) => a.sortOrder - b.sortOrder);
}

/** Yalnız AYNI üst öğenin kardeşleri arasında taşır - `sortOrder`'ları yeniden sayılandırır. */
export function moveSibling(items: readonly DraftMenuItem[], tempId: string, direction: -1 | 1): DraftMenuItem[] {
  const target = items.find((item) => item.tempId === tempId);
  if (!target) return [...items];

  const siblings = getChildren(items, target.parentTempId);
  const index = siblings.findIndex((item) => item.tempId === tempId);
  const swapIndex = index + direction;
  if (swapIndex < 0 || swapIndex >= siblings.length) return [...items];

  const reordered = [...siblings];
  const [moved] = reordered.splice(index, 1);
  reordered.splice(swapIndex, 0, moved!);
  const sortOrderByTempId = new Map(reordered.map((item, i) => [item.tempId, i]));

  return items.map((item) =>
    sortOrderByTempId.has(item.tempId) ? { ...item, sortOrder: sortOrderByTempId.get(item.tempId)! } : item,
  );
}

/** Bir öğeyi ve TÜM alt ağacını kaldırır. */
export function removeWithDescendants(items: readonly DraftMenuItem[], tempId: string): DraftMenuItem[] {
  const toRemove = new Set([tempId]);
  let changed = true;
  while (changed) {
    changed = false;
    for (const item of items) {
      if (item.parentTempId && toRemove.has(item.parentTempId) && !toRemove.has(item.tempId)) {
        toRemove.add(item.tempId);
        changed = true;
      }
    }
  }
  return items.filter((item) => !toRemove.has(item.tempId));
}
