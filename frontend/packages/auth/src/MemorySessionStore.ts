import type { Session, SessionStore } from './SessionStore';

/**
 * K-07 geçici model: access/refresh token YALNIZ JS belleğinde (bu closure'da) tutulur.
 * localStorage, sessionStorage, IndexedDB veya çerez KULLANILMAZ. Sayfa yenilenince
 * oturum sıfırlanır; bu kabul edilen geçici ödünleşimdir (FRONTEND-ACIK-MADDELER.md §C).
 */
export function createMemorySessionStore(): SessionStore {
  let session: Session | null = null;
  const listeners = new Set<() => void>();

  function notify() {
    for (const listener of listeners) listener();
  }

  return {
    getSession() {
      return session;
    },
    getAccessToken() {
      return session?.accessToken ?? null;
    },
    setSession(next) {
      session = next;
      notify();
    },
    subscribe(listener) {
      listeners.add(listener);
      return () => listeners.delete(listener);
    },
  };
}
