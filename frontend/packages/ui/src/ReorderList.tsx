'use client';

import { ArrowDown, ArrowUp } from 'lucide-react';
import type { ReactNode } from 'react';
import { useState } from 'react';

export interface ReorderListProps<T> {
  items: readonly T[];
  getKey: (item: T) => string;
  /** Duyuru metninde kullanılır (ör. "Ana Sayfa"), görünür etiketten ayrı tutulur ki `renderItem` serbestçe zengin içerik dönebilsin. */
  getLabel: (item: T) => string;
  renderItem: (item: T, index: number) => ReactNode;
  onMove: (fromIndex: number, toIndex: number) => void;
  disabled?: boolean;
}

/**
 * Sürükle-bırak YOK (FRONTEND-FAZ2-MASTER-PROMPT.md "ortak ilkeler": onaylı liste dışı) - yalnız
 * erişilebilir "yukarı/aşağı taşı" düğmeleri. Her taşıma `aria-live="polite"` bölgesiyle duyurulur
 * (ör. "Ana Sayfa, 2. sıraya taşındı") - ekran okuyucu kullanıcı sıra değişikliğini görsel geri
 * bildirim olmadan da anlar.
 */
export function ReorderList<T>({ items, getKey, getLabel, renderItem, onMove, disabled = false }: ReorderListProps<T>) {
  const [announcement, setAnnouncement] = useState('');

  function move(index: number, direction: -1 | 1) {
    const targetIndex = index + direction;
    if (targetIndex < 0 || targetIndex >= items.length) return;
    onMove(index, targetIndex);
    setAnnouncement(`${getLabel(items[index]!)}, ${targetIndex + 1}. sıraya taşındı.`);
  }

  return (
    <div className="reorder-list">
      <ul className="reorder-list__items">
        {items.map((item, index) => (
          <li key={getKey(item)} className="reorder-list__item">
            <div className="reorder-list__content">{renderItem(item, index)}</div>
            <div className="reorder-list__controls">
              <button
                type="button"
                className="reorder-list__button"
                aria-label={`${getLabel(item)} öğesini yukarı taşı`}
                disabled={disabled || index === 0}
                onClick={() => move(index, -1)}
              >
                <ArrowUp aria-hidden="true" size={16} />
              </button>
              <button
                type="button"
                className="reorder-list__button"
                aria-label={`${getLabel(item)} öğesini aşağı taşı`}
                disabled={disabled || index === items.length - 1}
                onClick={() => move(index, 1)}
              >
                <ArrowDown aria-hidden="true" size={16} />
              </button>
            </div>
          </li>
        ))}
      </ul>
      <p className="sr-only" role="status" aria-live="polite">
        {announcement}
      </p>
    </div>
  );
}
