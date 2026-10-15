/** RFC 9457 ProblemDetails (backend hata biçimi). FRONTEND-API-SOZLESMESI.md §1. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  /** Alan bazlı doğrulama hataları (400). */
  errors?: Record<string, string[]>;
  traceId?: string;
}

/**
 * UI'nin gördüğü tek hata biçimi. Ham sunucu mesajı/stack asla ekrana basılmaz;
 * yalnız `title`/`detail`/`correlationId` gösterilir (FRONTEND-GUVENLIK.md §5).
 */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly title: string,
    public readonly detail?: string,
    public readonly errors?: Readonly<Record<string, string[]>>,
    public readonly correlationId?: string,
    public readonly retryAfterSeconds?: number,
  ) {
    super(title);
    this.name = 'ApiError';
  }

  static async fromResponse(response: Response): Promise<ApiError> {
    const correlationId = response.headers.get('X-Correlation-ID') ?? undefined;
    const retryAfterHeader = response.headers.get('Retry-After');
    const retryAfterSeconds = retryAfterHeader ? Number.parseInt(retryAfterHeader, 10) : undefined;
    const contentType = response.headers.get('Content-Type') ?? '';
    if (contentType.includes('application/json') || contentType.includes('application/problem+json')) {
      try {
        const body = (await response.json()) as ProblemDetails;
        return new ApiError(
          response.status,
          body.title ?? defaultTitle(response.status),
          body.detail,
          body.errors,
          correlationId,
          retryAfterSeconds,
        );
      } catch {
        // JSON ayrıştırılamadı; aşağıdaki genel hataya düş.
      }
    }
    return new ApiError(response.status, defaultTitle(response.status), undefined, undefined, correlationId, retryAfterSeconds);
  }
}

function defaultTitle(status: number): string {
  switch (status) {
    case 401: return 'Oturum doğrulanamadı';
    case 403: return 'Bu işlem için yetkiniz yok';
    case 404: return 'Kayıt bulunamadı';
    case 409: return 'Çakışma oluştu';
    case 422: return 'İş kuralı ihlali';
    case 429: return 'Çok fazla istek gönderildi';
    default: return status >= 500 ? 'Beklenmeyen bir sunucu hatası oluştu' : 'İstek tamamlanamadı';
  }
}
