import { useEffect, useRef, type ReactNode } from 'react';
import { X } from 'lucide-react';

export function AccessibleDialog({
  open,
  onClose,
  title,
  children,
  className = '',
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  className?: string;
}) {
  const surfaceRef = useRef<HTMLDivElement>(null);
  const closeRef = useRef<HTMLButtonElement>(null);
  const previousFocus = useRef<HTMLElement | null>(null);

  useEffect(() => {
    if (!open) return undefined;
    previousFocus.current = document.activeElement as HTMLElement;
    const originalOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    closeRef.current?.focus();
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose();
      if (event.key !== 'Tab') return;
      const nodes = surfaceRef.current?.querySelectorAll<HTMLElement>(
        'button, a, input, select, textarea, iframe, [tabindex]:not([tabindex="-1"])',
      );
      if (!nodes?.length) return;
      const first = nodes[0];
      const last = nodes[nodes.length - 1];
      if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus(); }
      if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first?.focus(); }
    };
    document.addEventListener('keydown', onKeyDown);
    return () => {
      document.body.style.overflow = originalOverflow;
      document.removeEventListener('keydown', onKeyDown);
      previousFocus.current?.focus();
    };
  }, [onClose, open]);

  if (!open) return null;
  return (
    <div
      className={`accessible-dialog ${className}`.trim()}
      role="dialog"
      aria-modal="true"
      aria-labelledby="accessible-dialog-title"
      onMouseDown={(event) => { if (event.target === event.currentTarget) onClose(); }}
    >
      <div className="accessible-dialog__surface" ref={surfaceRef}>
        <button ref={closeRef} className="accessible-dialog__close" type="button" onClick={onClose}>
          <X aria-hidden="true" />
          <span className="sr-only">Pencereyi kapat</span>
        </button>
        <h2 className="sr-only" id="accessible-dialog-title">{title}</h2>
        {children}
      </div>
    </div>
  );
}
