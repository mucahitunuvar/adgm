import {
  getCurrentUser as apiGetCurrentUser,
  login as apiLogin,
  logout as apiLogout,
  refreshAccessToken as apiRefreshAccessToken,
  type ApiClient,
  type GetCurrentUserResponse,
} from '@gm/api-client';
import { isUserRole } from './roles';
import type { SessionStore, SessionUser } from './SessionStore';

function toUser(body: GetCurrentUserResponse): SessionUser {
  if (!isUserRole(body.role)) {
    throw new Error(`Bilinmeyen rol: ${body.role}`);
  }
  return { ...body, role: body.role };
}

export interface AuthController {
  getUser(): SessionUser | null;
  subscribeUser(listener: () => void): () => void;
  login(email: string, password: string): Promise<SessionUser>;
  logout(): Promise<void>;
  /** Eşzamanlı isteklerde tek refresh (kilitli). Başarısızsa oturumu temizler. */
  refresh(): Promise<boolean>;
  loadCurrentUser(): Promise<SessionUser | null>;
}

export function createAuthController(client: ApiClient, store: SessionStore): AuthController {
  let user: SessionUser | null = null;
  const userListeners = new Set<() => void>();
  let refreshPromise: Promise<boolean> | null = null;

  function setUser(next: SessionUser | null) {
    user = next;
    for (const listener of userListeners) listener();
  }

  async function login(email: string, password: string): Promise<SessionUser> {
    const session = await apiLogin(client, email, password);
    store.setSession(session);
    const me = await apiGetCurrentUser(client);
    const sessionUser = toUser(me);
    setUser(sessionUser);
    return sessionUser;
  }

  async function logout(): Promise<void> {
    const session = store.getSession();
    try {
      if (session) {
        await apiLogout(client, session.refreshToken);
      }
    } catch {
      // Backend çağrısı başarısız olsa bile (ağ hatası vb.) yerel oturum temizlenir;
      // kullanıcı için "çıkış yapıldı" garantidir. Hata burada yutulur.
    } finally {
      // Yalnız istemcide silmek yetmez ama backend çağrısı başarısız olsa da bellek temizlenir
      // (FRONTEND-GUVENLIK.md §2) — kullanıcı en azından bu cihazda çıkış yapmış olur.
      store.setSession(null);
      setUser(null);
    }
  }

  function refresh(): Promise<boolean> {
    if (refreshPromise) return refreshPromise;
    const session = store.getSession();
    if (!session) return Promise.resolve(false);

    refreshPromise = (async () => {
      try {
        const next = await apiRefreshAccessToken(client, session.refreshToken);
        store.setSession(next);
        return true;
      } catch {
        store.setSession(null);
        setUser(null);
        return false;
      } finally {
        refreshPromise = null;
      }
    })();
    return refreshPromise;
  }

  async function loadCurrentUser(): Promise<SessionUser | null> {
    if (!store.getAccessToken()) return null;
    try {
      const me = await apiGetCurrentUser(client);
      const sessionUser = toUser(me);
      setUser(sessionUser);
      return sessionUser;
    } catch {
      return null;
    }
  }

  return {
    getUser: () => user,
    subscribeUser: (listener) => {
      userListeners.add(listener);
      return () => userListeners.delete(listener);
    },
    login,
    logout,
    refresh,
    loadCurrentUser,
  };
}
