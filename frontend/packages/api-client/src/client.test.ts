import { describe, expect, it, vi } from 'vitest';
import { createApiClient } from './client';
import { ApiError } from './ApiError';

function jsonResponse(status: number, body: unknown, headers: Record<string, string> = {}) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json', ...headers },
  });
}

describe('createApiClient', () => {
  it('sends Authorization header when a token is available', async () => {
    const fetchImpl = vi.fn().mockResolvedValue(jsonResponse(200, { ok: true }));
    const client = createApiClient({ baseUrl: 'http://api.test', getAccessToken: () => 'abc123', fetchImpl });

    await client.request('/foo');

    const [, init] = fetchImpl.mock.calls[0] as [string, RequestInit];
    expect((init.headers as Record<string, string>).Authorization).toBe('Bearer abc123');
  });

  it('throws ApiError with ProblemDetails fields on a non-2xx response', async () => {
    const fetchImpl = vi.fn().mockResolvedValue(
      jsonResponse(422, { title: 'İş kuralı ihlali', detail: 'Kapasite doldu', errors: { email: ['Zorunlu alan'] } }),
    );
    const client = createApiClient({ baseUrl: 'http://api.test', fetchImpl });

    await expect(client.request('/foo')).rejects.toSatisfy((error: unknown) => {
      expect(error).toBeInstanceOf(ApiError);
      const apiError = error as ApiError;
      expect(apiError.status).toBe(422);
      expect(apiError.title).toBe('İş kuralı ihlali');
      expect(apiError.errors).toEqual({ email: ['Zorunlu alan'] });
      return true;
    });
  });

  it('captures Retry-After on 429 without retrying automatically', async () => {
    const fetchImpl = vi.fn().mockResolvedValue(
      jsonResponse(429, { title: 'Çok fazla istek' }, { 'Retry-After': '30' }),
    );
    const client = createApiClient({ baseUrl: 'http://api.test', fetchImpl });

    await expect(client.request('/foo')).rejects.toSatisfy((error: unknown) => {
      const apiError = error as ApiError;
      expect(apiError.status).toBe(429);
      expect(apiError.retryAfterSeconds).toBe(30);
      return true;
    });
    expect(fetchImpl).toHaveBeenCalledTimes(1);
  });

  it('calls onUnauthorized once and retries exactly one time on 401', async () => {
    const fetchImpl = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse(401, { title: 'Oturum doğrulanamadı' }))
      .mockResolvedValueOnce(jsonResponse(200, { ok: true }));
    const onUnauthorized = vi.fn().mockResolvedValue(undefined);
    const client = createApiClient({ baseUrl: 'http://api.test', fetchImpl, onUnauthorized });

    const result = await client.request<{ ok: boolean }>('/foo');

    expect(result).toEqual({ ok: true });
    expect(onUnauthorized).toHaveBeenCalledTimes(1);
    expect(fetchImpl).toHaveBeenCalledTimes(2);
  });

  it('does not loop forever if the retry also returns 401', async () => {
    const fetchImpl = vi.fn().mockResolvedValue(jsonResponse(401, { title: 'Oturum doğrulanamadı' }));
    const onUnauthorized = vi.fn().mockResolvedValue(undefined);
    const client = createApiClient({ baseUrl: 'http://api.test', fetchImpl, onUnauthorized });

    await expect(client.request('/foo')).rejects.toBeInstanceOf(ApiError);
    expect(fetchImpl).toHaveBeenCalledTimes(2);
  });
});
