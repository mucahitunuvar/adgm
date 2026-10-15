import type { VideoSummaryResponse } from '@gm/api-client';
import { useRef, useState, type ReactNode } from 'react';
import { VideoPickerDialog } from './VideoPickerDialog';

export function useVideoPicker(): { pickVideo: () => Promise<VideoSummaryResponse | null>; pickerElement: ReactNode } {
  const [open, setOpen] = useState(false);
  const resolverRef = useRef<((value: VideoSummaryResponse | null) => void) | null>(null);

  function pickVideo(): Promise<VideoSummaryResponse | null> {
    setOpen(true);
    return new Promise((resolve) => {
      resolverRef.current = resolve;
    });
  }

  function handleSelect(video: VideoSummaryResponse) {
    setOpen(false);
    resolverRef.current?.(video);
    resolverRef.current = null;
  }

  function handleCancel() {
    setOpen(false);
    resolverRef.current?.(null);
    resolverRef.current = null;
  }

  return {
    pickVideo,
    pickerElement: <VideoPickerDialog open={open} onCancel={handleCancel} onSelect={handleSelect} />,
  };
}
