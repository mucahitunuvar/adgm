import { describe, expect, it, vi } from 'vitest';
import type { ApiClient } from '@gm/api-client';
import { createAuthController } from './createAuthController';
import { createMemorySessionStore } from './MemorySessionStore';

const loginBody = {
  accessToken: 'access-1',
  accessTokenExpiresAtUtc: '2026-01-01T00:00:00Z',
  refreshToken: 'refresh-1',
  refreshTokenExpiresAtUtc: '2026-01-02T00:00:00Z',
  emailConfirmed: true,
};

const meBody = {
  userId: 'u-1',
  email: 'ayse@example.com',
  role: 'Employer',
  status: 'Active',
  createdAtUtc: '2025-01-01T00:00:00Z',
  emailConfirmed: true,
};

function fakeClient(request: ApiClient['request']): ApiClient {
  return { request };
}

describe('createAuthController', () => {
  it('login stores the session (memory only) and loads the current user', async () => {
    const store = createMemorySessionStore();
    const request = vi.fn().mockResolvedValueOnce(loginBody).mockResolvedValueOnce(meBody);
    const controller = createAuthController(fakeClient(request), store);

    const user = await controller.login('ayse@example.com', 'secret');

    expect(user.role).toBe('Employer');
    expect(store.getAccessToken()).toBe('access-1');
    expect(request).toHaveBeenNthCalledWith(1, '/api/v1/auth/login', {
      method: 'POST',
      body: { email: 'ayse@example.com', password: 'secret' },
    });
    expect(request).toHaveBeenNthCalledWith(2, '/api/v1/auth/me');
  });

  it('runs only one refresh at a time for concurrent callers', async () => {
    const store = createMemorySessionStore();
    store.setSession({ ...loginBody, refreshToken: 'refresh-1' });
    let resolveRefresh!: (value: typeof loginBody) => void;
    const request = vi.fn().mockReturnValue(
      new Promise((resolve) => {
        resolveRefresh = resolve;
      }),
    );
    const controller = createAuthController(fakeClient(request), store);

    const first = controller.refresh();
    const second = controller.refresh();
    resolveRefresh({ ...loginBody, accessToken: 'access-2' });
    const [firstResult, secondResult] = await Promise.all([first, second]);

    expect(firstResult).toBe(true);
    expect(secondResult).toBe(true);
    expect(request).toHaveBeenCalledTimes(1);
    expect(store.getAccessToken()).toBe('access-2');
  });

  it('clears the session when refresh fails', async () => {
    const store = createMemorySessionStore();
    store.setSession(loginBody);
    const request = vi.fn().mockRejectedValue(new Error('refresh reddedildi'));
    const controller = createAuthController(fakeClient(request), store);

    const result = await controller.refresh();

    expect(result).toBe(false);
    expect(store.getSession()).toBeNull();
  });

  it('clears the local session on logout even if the backend call fails', async () => {
    const store = createMemorySessionStore();
    store.setSession(loginBody);
    const request = vi.fn().mockRejectedValue(new Error('ağ hatası'));
    const controller = createAuthController(fakeClient(request), store);

    await controller.logout();

    expect(store.getSession()).toBeNull();
    expect(controller.getUser()).toBeNull();
  });
});
