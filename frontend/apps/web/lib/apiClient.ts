import { createApiClient } from '@gm/api-client';

const backendOrigin = process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';

/**
 * `apps/web` oturum okumaz (K-07) - `getAccessToken`/`onUnauthorized` kasıtlı olarak verilmez.
 * Her sunucu bileşeni çağrısında yeni bir istemci oluşturmak ucuzdur (durumsuz sarmalayıcı).
 */
export function createServerApiClient() {
  return createApiClient({ baseUrl: backendOrigin });
}
