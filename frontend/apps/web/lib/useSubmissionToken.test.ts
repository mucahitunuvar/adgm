import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, renderHook } from '@testing-library/react';
import { REFRESH_BUFFER_MS, TOKEN_LIFETIME_MS, useSubmissionToken } from './useSubmissionToken';

async function advance(ms: number) {
  await act(async () => {
    await vi.advanceTimersByTimeAsync(ms);
  });
}

// KOŞU 3G Görev 2: arka plan tazeleme zamanlayıcısının (süresi dolmadan önce kendiliğinden yeni bir
// jeton istemesi) ve ağ hatasında kendi kendini onarmasının gerçek davranışını kanıtlar - gerçek
// backend'e karşı 2 saat beklemek pratik olmadığı için sahte zamanlayıcılarla (`vi.useFakeTimers`)
// simüle edilir.
function tokenResponse(token: string) {
  return new Response(JSON.stringify({ token }), { status: 200, headers: { 'Content-Type': 'application/json' } });
}

describe('useSubmissionToken', () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.unstubAllGlobals();
  });

  it('fetches a token on mount', async () => {
    fetchMock.mockResolvedValueOnce(tokenResponse('token-1'));

    const { result } = renderHook(() => useSubmissionToken());
    await advance(0);

    expect(result.current).toBe('token-1');
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it('does nothing when disabled', async () => {
    const { result } = renderHook(() => useSubmissionToken(false));
    await advance(TOKEN_LIFETIME_MS);

    expect(result.current).toBeNull();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('refreshes the token in the background before it expires, well ahead of any realistic submit', async () => {
    fetchMock.mockResolvedValueOnce(tokenResponse('token-1')).mockResolvedValueOnce(tokenResponse('token-2'));

    const { result } = renderHook(() => useSubmissionToken());
    await advance(0);
    expect(result.current).toBe('token-1');

    // Süresi dolmadan hemen önce (tampon payından az önce) hâlâ ilk jeton kullanılabilir olmalı.
    await advance(TOKEN_LIFETIME_MS - REFRESH_BUFFER_MS - 1_000);
    expect(result.current).toBe('token-1');
    expect(fetchMock).toHaveBeenCalledTimes(1);

    // Tampon payına girildiğinde arka planda kendiliğinden tazelenir - hiçbir gönderim tetiklemeden.
    await advance(2_000);
    expect(result.current).toBe('token-2');
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  it('retries shortly after a failed refresh instead of giving up', async () => {
    fetchMock
      .mockResolvedValueOnce(tokenResponse('token-1'))
      .mockRejectedValueOnce(new Error('network error'))
      .mockResolvedValueOnce(tokenResponse('token-2'));

    const { result } = renderHook(() => useSubmissionToken());
    await advance(0);
    expect(result.current).toBe('token-1');

    await advance(TOKEN_LIFETIME_MS - REFRESH_BUFFER_MS);
    expect(fetchMock).toHaveBeenCalledTimes(2);
    // Başarısız tazeleme eski jetonu KORUR (geçerliyse gönderim hâlâ çalışsın) - sessizce düşürülmez.
    expect(result.current).toBe('token-1');

    await advance(30_000);
    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(result.current).toBe('token-2');
  });

  it('stops scheduling further requests after unmount', async () => {
    fetchMock.mockResolvedValueOnce(tokenResponse('token-1'));

    const { unmount } = renderHook(() => useSubmissionToken());
    await advance(0);
    expect(fetchMock).toHaveBeenCalledTimes(1);

    unmount();
    await advance(TOKEN_LIFETIME_MS * 2);

    expect(fetchMock).toHaveBeenCalledTimes(1);
  });
});
