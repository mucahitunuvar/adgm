import { createApiClient, type PublicSiteResponse } from '@gm/api-client';

// Faz 3 Koşu 1, karar 7: middleware HER istekte backend'e gidip `maintenanceModeEnabled`'ı
// sormaz - tek Node süreci için basit, modül-kapsamlı bir TTL önbelleği yeterli (bu, backend'in
// kendi ~60 sn "public-site" önbelleğiyle aynı mertebede, daha kısa tutuldu ki bakım moduna
// geçiş en fazla bu kadar gecikmeyle yansısın). `GET /public/site` backend'in bakım modu
// middleware'inden BİLEREK muaf tutulan tek public uçtur (bu sorgunun kendisi 503 almaz).
const TTL_MS = 10_000;

export interface MaintenanceStatus {
  enabled: boolean;
  message: string;
}

const DEFAULT_STATUS: MaintenanceStatus = { enabled: false, message: '' };

let cached: { status: MaintenanceStatus; checkedAtMs: number } | null = null;

export async function getMaintenanceStatus(backendOrigin: string): Promise<MaintenanceStatus> {
  const now = Date.now();
  if (cached && now - cached.checkedAtMs < TTL_MS) {
    return cached.status;
  }

  try {
    const client = createApiClient({ baseUrl: backendOrigin });
    const site = await client.request<PublicSiteResponse>('/api/v1/public/site', { cache: 'no-store' });
    cached = { status: { enabled: site.maintenanceModeEnabled, message: site.maintenanceMessage }, checkedAtMs: now };
    return cached.status;
  } catch {
    // Backend'e ulaşılamıyorsa bakım sayfasını zorlamak yanlış bir pozitif olur - önceki bilinen
    // değere (yoksa kapalıya) düşülür; bir sonraki istek tekrar dener.
    return cached?.status ?? DEFAULT_STATUS;
  }
}
