import type { ApiClient } from './client';
import type { components } from './schema';

export type PublicLookupItemResponse = components['schemas']['PublicLookupItemResponse'];
export type PagedResultOfPublicLookupItemResponse = components['schemas']['PagedResultOfPublicLookupItemResponse'];

// AGENTS.md §40/PERFORMANCE.md §11: PagedRequest.MaxPageSize (backend, 100) caps every page - a
// dropdown wants every active row in one request, not its own pagination UI, so this is the largest
// single page the backend will actually return.
const DROPDOWN_PAGE_SIZE = 100;

/**
 * FRONTEND-ACIK-MADDELER.md madde 39: `/ilanlar`'ın il/pozisyon/departman/çalışma şekli/çalışma yeri
 * tipi dropdown'larının tek veri kaynağı - yalnız `{id, name}` (aktif kayıtlar), `public-read` rate
 * limit'e tabi. 60 sn `revalidate`, diğer tüm public liste uçlarıyla (`listPublicJobs` vb.) aynı.
 */
function listPublicLookupItems(
  client: ApiClient,
  routeSegment: 'provinces' | 'positions' | 'departments' | 'employment-types' | 'work-location-types',
  revalidate = 60,
): Promise<PagedResultOfPublicLookupItemResponse> {
  return client.request(`/api/v1/public/reference-data/${routeSegment}`, {
    query: { pageSize: DROPDOWN_PAGE_SIZE },
    revalidate,
  });
}

export function listPublicProvinces(client: ApiClient, revalidate = 60) {
  return listPublicLookupItems(client, 'provinces', revalidate);
}

export function listPublicPositions(client: ApiClient, revalidate = 60) {
  return listPublicLookupItems(client, 'positions', revalidate);
}

export function listPublicDepartments(client: ApiClient, revalidate = 60) {
  return listPublicLookupItems(client, 'departments', revalidate);
}

export function listPublicEmploymentTypes(client: ApiClient, revalidate = 60) {
  return listPublicLookupItems(client, 'employment-types', revalidate);
}

export function listPublicWorkLocationTypes(client: ApiClient, revalidate = 60) {
  return listPublicLookupItems(client, 'work-location-types', revalidate);
}
