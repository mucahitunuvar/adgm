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

export interface BlobResponse {
  blob: Blob;
  /** `Content-Disposition`'dan çıkarılan dosya adı (varsa); kullanılmadan önce çağıran kaçışlamalıdır. */
  filename: string | null;
}

export interface ApiClient {
  request<TResponse>(path: string, init?: ApiRequestInit): Promise<TResponse>;
  /**
   * CSV/dosya indirme gibi `application/json` olmayan yanıtlar için. Aynı Authorization/401-tek-
   * seferlik-refresh davranışını `request` ile paylaşır (FRONTEND-GUVENLIK.md §2); tüketici `Blob`'u
   * geçici bir `objectURL`'e çevirip indirmeyi tetikler, sonra `revokeObjectURL` ile serbest bırakır.
   */
  requestBlob(path: string, init?: ApiRequestInit): Promise<BlobResponse>;
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
/** `Content-Disposition: attachment; filename="x.csv"; filename*=UTF-8''x.csv` biçimlerini ayrıştırır. */
function parseContentDispositionFilename(header: string | null): string | null {
  if (!header) return null;
  const starMatch = /filename\*=(?:UTF-8'')?([^;]+)/i.exec(header);
  if (starMatch?.[1]) {
    try {
      return decodeURIComponent(starMatch[1].trim().replace(/^"|"$/g, ''));
    } catch {
      // Çözümlenemeyen yüzde kodlaması; aşağıdaki düz `filename`'e düş.
    }
  }
  const plainMatch = /filename="?([^";]+)"?/i.exec(header);
  return plainMatch?.[1]?.trim() ?? null;
}

export function createApiClient(options: ApiClientOptions): ApiClient {
  const fetchImpl = options.fetchImpl ?? fetch;

  async function performFetch(path: string, init: ApiRequestInit, isRetry: boolean): Promise<Response> {
    const token = options.getAccessToken?.();
    const headers: Record<string, string> = {};
    if (token) headers.Authorization = `Bearer ${token}`;
    // FormData gövdesi için Content-Type elle verilmez - tarayıcı/fetch kendi multipart
    // sınır (boundary) değerini üretip ekler; biz üretirsek sınır eksik kalır ve backend
    // gövdeyi ayrıştıramaz (ör. UploadMediaAssetEndpoint'in IFormFile bağlaması).
    const isFormData = init.body instanceof FormData;
    if (init.body !== undefined && !isFormData) headers['Content-Type'] = 'application/json';
    if (init.idempotencyKey) headers['Idempotency-Key'] = init.idempotencyKey;

    const fetchInit: FetchInitWithNext = {
      method: init.method ?? 'GET',
      headers,
      body: init.body === undefined ? undefined : isFormData ? (init.body as FormData) : JSON.stringify(init.body),
      signal: init.signal,
      cache: init.cache,
    };
    if (init.revalidate !== undefined) fetchInit.next = { revalidate: init.revalidate };

    const response = await fetchImpl(buildUrl(options.baseUrl, path, init.query), fetchInit);

    if (response.status === 401 && !isRetry && options.onUnauthorized) {
      await options.onUnauthorized();
      return performFetch(path, init, true);
    }

    if (!response.ok) {
      throw await ApiError.fromResponse(response);
    }

    return response;
  }

  async function performRequest<TResponse>(path: string, init: ApiRequestInit): Promise<TResponse> {
    const response = await performFetch(path, init, false);

    if (response.status === 204 || response.headers.get('Content-Length') === '0') {
      return undefined as TResponse;
    }
    const contentType = response.headers.get('Content-Type') ?? '';
    if (!contentType.includes('application/json')) {
      return undefined as TResponse;
    }
    return (await response.json()) as TResponse;
  }

  async function performBlobRequest(path: string, init: ApiRequestInit): Promise<BlobResponse> {
    const response = await performFetch(path, init, false);
    const blob = await response.blob();
    const filename = parseContentDispositionFilename(response.headers.get('Content-Disposition'));
    return { blob, filename };
  }

  return {
    request<TResponse>(path: string, init: ApiRequestInit = {}) {
      return performRequest<TResponse>(path, init);
    },
    requestBlob(path: string, init: ApiRequestInit = {}) {
      return performBlobRequest(path, init);
    },
  };
}
