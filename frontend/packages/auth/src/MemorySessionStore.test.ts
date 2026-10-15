import { describe, expect, it, vi } from 'vitest';
import { createMemorySessionStore } from './MemorySessionStore';

const sampleSession = {
  accessToken: 'access-1',
  accessTokenExpiresAtUtc: '2026-01-01T00:00:00Z',
  refreshToken: 'refresh-1',
  refreshTokenExpiresAtUtc: '2026-01-02T00:00:00Z',
  emailConfirmed: true,
};

describe('createMemorySessionStore', () => {
  it('starts without a session', () => {
    const store = createMemorySessionStore();
    expect(store.getSession()).toBeNull();
    expect(store.getAccessToken()).toBeNull();
  });

  it('never touches localStorage or sessionStorage', () => {
    const localSpy = vi.spyOn(Storage.prototype, 'setItem');
    const store = createMemorySessionStore();
    store.setSession(sampleSession);
    expect(store.getAccessToken()).toBe('access-1');
    expect(localSpy).not.toHaveBeenCalled();
    localSpy.mockRestore();
  });

  it('notifies subscribers on change and clears on null', () => {
    const store = createMemorySessionStore();
    const listener = vi.fn();
    const unsubscribe = store.subscribe(listener);

    store.setSession(sampleSession);
    expect(listener).toHaveBeenCalledTimes(1);

    store.setSession(null);
    expect(listener).toHaveBeenCalledTimes(2);
    expect(store.getAccessToken()).toBeNull();

    unsubscribe();
    store.setSession(sampleSession);
    expect(listener).toHaveBeenCalledTimes(2);
  });
});
