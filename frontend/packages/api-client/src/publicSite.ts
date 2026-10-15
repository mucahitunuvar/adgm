import type { ApiClient } from './client';
import type { operations } from './schema';

type GetPublicSiteQuery = NonNullable<operations['GetPublicSite']['parameters']['query']>;
type GetPublicHomeQuery = NonNullable<operations['GetPublicHome']['parameters']['query']>;

/**
 * NOT: Backend OpenAPI şeması `GET /api/v1/public/site` için yanıt gövdesi (response content)
 * tanımlamıyor (bkz. schema.d.ts: `operations['GetPublicSite']['responses'][200]['content']` → `never`).
 * K-05 "tipler yalnız OpenAPI'den üretilir" kuralına uymak için burada dönüş tipi kasıtlı olarak
 * `unknown` bırakılmıştır; `any` veya elle yazılmış bir arayüz KULLANILMAMIŞTIR.
 * Backend bu uca (ve aynı durumdaki Website/Auth modülü uçlarına) yanıt şeması eklediğinde
 * openapi-typescript yeniden üretimi bu fonksiyonu otomatik olarak güçlü tipli hale getirecektir.
 * Kayıt: FRONTEND-ACIK-MADDELER.md §B.
 */
export function getPublicSite(client: ApiClient, query?: GetPublicSiteQuery, revalidate = 60): Promise<unknown> {
  return client.request('/api/v1/public/site', { query, revalidate });
}

/** Aynı not GET /api/v1/public/home için de geçerlidir. */
export function getPublicHome(client: ApiClient, query?: GetPublicHomeQuery, revalidate = 60): Promise<unknown> {
  return client.request('/api/v1/public/home', { query, revalidate });
}
