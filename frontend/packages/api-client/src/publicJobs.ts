import type { ApiClient } from './client';
import type { components } from './schema';

export type PublicJobListItemResponse = components['schemas']['PublicJobListItemResponse'];
export type PagedResultOfPublicJobListItemResponse = components['schemas']['PagedResultOfPublicJobListItemResponse'];

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
