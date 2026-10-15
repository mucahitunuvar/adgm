import { ApiError } from './ApiError';

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface ApiRequestInit {
  method?: HttpMethod;
  query?: Readonly<Record<string, string | number | boolean | undefined>>;
  body?: unknown;
  signal?: AbortSignal;
  /** Idempotency-Key: çifte gönderimi önlemek için mutasyonlarda kullanılır. */
  idempotencyKey?: string;
  /**
   * Yalnız anonim public uçlarda kullanılır (apps/web sunucu `fetch`i).
   * Yetkili isteklerde her zaman 'no-store' davranışı geçerlidir (FRONTEND-GUVENLIK.md §5).
   */
  cache?: RequestCache;
  /**
   * Next.js sunucu `fetch` önbellek süresi (saniye). Yalnız anonim public uçlarda kullanılır;
   * Next dışı ortamlarda (Vite) yok sayılır (FRONTEND-MIMARI.md §3).
   */
  revalidate?: number;
}

type FetchInitWithNext = RequestInit & { next?: { revalidate?: number } };

export interface ApiClientOptions {
  baseUrl: string;
  /** Geçerli access token'ı döner; oturum yoksa null. Token yalnız bellekte tutulur (K-07). */
  getAccessToken?: () => string | null | undefined;
  /** 401 alındığında çağrılır (örn. tek seferlik refresh denemesi); auth paketi sağlar. */
  onUnauthorized?: () => void | Promise<void>;
  fetchImpl?: typeof fetch;
}

export interface ApiClient {
  request<TResponse>(path: string, init?: ApiRequestInit): Promise<TResponse>;
}

function buildUrl(baseUrl: string, path: string, query?: ApiRequestInit['query']): string {
  const url = new URL(path.replace(/^\/+/, ''), baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`);
  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined) url.searchParams.set(key, String(value));
    }
  }
  return url.toString();
}

/**
 * Tipli `fetch` sarmalayıcısı. UI bilmez; `Authorization: Bearer` yalnız burada eklenir
 * (FRONTEND-GUVENLIK.md §2). Sunucu (Next) ve tarayıcı tarafında aynı şekilde çalışır.
 */
export function createApiClient(options: ApiClientOptions): ApiClient {
  const fetchImpl = options.fetchImpl ?? fetch;

  async function performRequest<TResponse>(path: string, init: ApiRequestInit, isRetry: boolean): Promise<TResponse> {
    const token = options.getAccessToken?.();
    const headers: Record<string, string> = {};
    if (token) headers.Authorization = `Bearer ${token}`;
    if (init.body !== undefined) headers['Content-Type'] = 'application/json';
    if (init.idempotencyKey) headers['Idempotency-Key'] = init.idempotencyKey;

    const fetchInit: FetchInitWithNext = {
      method: init.method ?? 'GET',
      headers,
      body: init.body !== undefined ? JSON.stringify(init.body) : undefined,
      signal: init.signal,
      cache: init.cache,
    };
    if (init.revalidate !== undefined) fetchInit.next = { revalidate: init.revalidate };

    const response = await fetchImpl(buildUrl(options.baseUrl, path, init.query), fetchInit);

    if (response.status === 401 && !isRetry && options.onUnauthorized) {
      await options.onUnauthorized();
      return performRequest<TResponse>(path, init, true);
    }

    if (!response.ok) {
      throw await ApiError.fromResponse(response);
    }

    if (response.status === 204 || response.headers.get('Content-Length') === '0') {
      return undefined as TResponse;
    }
    const contentType = response.headers.get('Content-Type') ?? '';
    if (!contentType.includes('application/json')) {
      return undefined as TResponse;
    }
    return (await response.json()) as TResponse;
  }

  return {
    request<TResponse>(path: string, init: ApiRequestInit = {}) {
      return performRequest<TResponse>(path, init, false);
    },
  };
}
