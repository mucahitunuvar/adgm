import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { ApiClient } from '@gm/api-client';
import { createAuthSession, type AuthSession } from './createAuthSession';
import type { SessionUser } from './SessionStore';
import type { UserRole } from './roles';

interface AuthContextValue {
  user: SessionUser | null;
  isLoading: boolean;
  client: ApiClient;
  login(email: string, password: string): Promise<SessionUser>;
  logout(): Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ baseUrl, children }: { baseUrl: string; children: ReactNode }) {
  const [session] = useState<AuthSession>(() => createAuthSession({ baseUrl }));

  const [user, setUser] = useState<SessionUser | null>(session.controller.getUser());
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => session.controller.subscribeUser(() => setUser(session.controller.getUser())), [session]);

  useEffect(() => {
    let active = true;
    session.controller.loadCurrentUser().finally(() => {
      if (active) setIsLoading(false);
    });
    return () => {
      active = false;
    };
  }, [session]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isLoading,
      client: session.client,
      login: session.controller.login,
      logout: session.controller.logout,
    }),
    [user, isLoading, session],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth, AuthProvider içinde kullanılmalıdır.');
  return value;
}

export function useCurrentUser(): SessionUser | null {
  return useAuth().user;
}

/** Rol/yetki kontrolleri tek yerde (FRONTEND-GUVENLIK.md §7). */
export function useCan(roles: UserRole | readonly UserRole[]): boolean {
  const user = useCurrentUser();
  if (!user) return false;
  const list = Array.isArray(roles) ? roles : [roles];
  return (list as readonly UserRole[]).includes(user.role);
}
