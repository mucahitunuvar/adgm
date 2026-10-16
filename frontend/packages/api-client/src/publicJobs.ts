import type { ApiClient } from './client';
import type { components } from './schema';

export type PublicJobListItemResponse = components['schemas']['PublicJobListItemResponse'];
export type PagedResultOfPublicJobListItemResponse = components['schemas']['PagedResultOfPublicJobListItemResponse'];
export type GetPublicJobDetailResponse = components['schemas']['GetPublicJobDetailResponse'];

export interface ListPublicJobsParams {
  provinceId?: string;
  employmentTypeId?: string;
  workLocationTypeId?: string;
  positionId?: string;
  departmentId?: string;
  companyId?: string;
  isForDisabledCandidates?: boolean;
  q?: string;
  page?: number;
  pageSize?: number;
}

/** `job-list` bloğu veri taşımaz (ADR-024 §8.1) - bu, ana sayfadaki "öne çıkan ilanlar" bölümünün tek veri kaynağıdır. */
export function listPublicJobs(
  client: ApiClient,
  params: ListPublicJobsParams = {},
  revalidate = 60,
): Promise<PagedResultOfPublicJobListItemResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/public/jobs', { query, revalidate });
}

/** Kapanmış/görünmez ilan 404 (`Job.NotFound`) döner, 410 değil (BLOK-ESLEMESI.md önkoşul 3-c). */
export function getPublicJobDetail(client: ApiClient, slug: string, revalidate = 60): Promise<GetPublicJobDetailResponse> {
  return client.request(`/api/v1/public/jobs/${encodeURIComponent(slug)}`, { revalidate });
}

/**
 * Şirket logosu hazır bir sarmalayıcı uç değil, doğrudan `<img src>` olarak kullanılan bir public
 * medya yoludur (ADR-024 §6: public medya uzun ömürlü cache header'larıyla servis edilir).
 */
export function publicCompanyLogoUrl(companyId: string): string {
  return `/api/v1/public/companies/${encodeURIComponent(companyId)}/logo`;
}
