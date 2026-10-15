import type { ApiClient } from './client';
import type { components } from './schema';

export type BlockTypeResponse = components['schemas']['BlockTypeResponse'];
export type BlockTypeFieldDescriptor = components['schemas']['BlockTypeFieldDescriptor'];

/** `GET /api/v1/admin/website/block-types` - kod tabanlı blok kataloğu, tek kaynaktan üretilir. */
export function getBlockTypes(client: ApiClient): Promise<BlockTypeResponse[]> {
  return client.request('/api/v1/admin/website/block-types');
}
