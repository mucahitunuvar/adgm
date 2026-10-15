import { createApiClient, type ApiClient } from '@gm/api-client';
import { createMemorySessionStore } from './MemorySessionStore';
import { createAuthController, type AuthController } from './createAuthController';
import type { SessionStore } from './SessionStore';

export interface AuthSession {
  client: ApiClient;
  store: SessionStore;
  controller: AuthController;
}

/**
 * `ApiClient` ve `AuthController`'ı birbirine bağlar: 401 alındığında istemci
 * `controller.refresh()`'i çağırır (tek seferlik, kilitli). `Authorization: Bearer`
 * başlığı yalnız burada, api-client üzerinden eklenir (FRONTEND-GUVENLIK.md §2).
 */
export function createAuthSession(options: { baseUrl: string; fetchImpl?: typeof fetch }): AuthSession {
  const store = createMemorySessionStore();
  let controller: AuthController | null = null;

  const client = createApiClient({
    baseUrl: options.baseUrl,
    fetchImpl: options.fetchImpl,
    getAccessToken: () => store.getAccessToken(),
    onUnauthorized: async () => {
      await controller?.refresh();
    },
  });

  controller = createAuthController(client, store);

  return { client, store, controller };
}
