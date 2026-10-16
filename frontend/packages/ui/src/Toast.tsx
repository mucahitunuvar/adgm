'use client';

import { X } from 'lucide-react';
import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import type { StatusTone } from './StatusBadge';

interface ToastItem {
  id: string;
  tone: StatusTone;
  message: string;
}

interface ToastContextValue {
  showToast: (message: string, tone?: StatusTone) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const AUTO_DISMISS_MS = 5000;

/** Mutasyon sonrası geri bildirim için tek, uygulama kökünde çağrılan sağlayıcı. */
export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const dismiss = useCallback((id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  const showToast = useCallback(
    (message: string, tone: StatusTone = 'info') => {
      const id = crypto.randomUUID();
      setToasts((prev) => [...prev, { id, tone, message }]);
      setTimeout(() => dismiss(id), AUTO_DISMISS_MS);
    },
    [dismiss],
  );

  const value = useMemo<ToastContextValue>(() => ({ showToast }), [showToast]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast-region" role="status" aria-live="polite">
        {toasts.map((toast) => (
          <div key={toast.id} className={`toast toast--${toast.tone}`}>
            <p>{toast.message}</p>
            <button type="button" className="toast__dismiss" onClick={() => dismiss(toast.id)}>
              <X aria-hidden="true" />
              <span className="sr-only">Bildirimi kapat</span>
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const value = useContext(ToastContext);
  if (!value) throw new Error('useToast, ToastProvider içinde kullanılmalıdır.');
  return value;
}
