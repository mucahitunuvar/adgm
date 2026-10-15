import type { ContentItemSummaryResponse } from '@gm/api-client';
import { useRef, useState, type ReactNode } from 'react';
import { ContentItemPickerDialog } from './ContentItemPickerDialog';

/** `useMediaPicker`'ın aynı komut deseni: `pickContentItem()` bir Promise döner. */
export function useContentItemPicker(excludeId?: string): {
  pickContentItem: () => Promise<ContentItemSummaryResponse | null>;
  pickerElement: ReactNode;
} {
  const [open, setOpen] = useState(false);
  const resolverRef = useRef<((value: ContentItemSummaryResponse | null) => void) | null>(null);

  function pickContentItem(): Promise<ContentItemSummaryResponse | null> {
    setOpen(true);
    return new Promise((resolve) => {
      resolverRef.current = resolve;
    });
  }

  function handleSelect(item: ContentItemSummaryResponse) {
    setOpen(false);
    resolverRef.current?.(item);
    resolverRef.current = null;
  }

  function handleCancel() {
    setOpen(false);
    resolverRef.current?.(null);
    resolverRef.current = null;
  }

  return {
    pickContentItem,
    pickerElement: <ContentItemPickerDialog open={open} excludeId={excludeId} onCancel={handleCancel} onSelect={handleSelect} />,
  };
}
