import { cache } from 'react';
import { getPublicSite, type PublicSiteResponse } from '@gm/api-client';
import { createServerApiClient } from './apiClient';

/** `GET /public/site` - ortak ilke: ~60 sn `revalidate`. Tek render geçişinde (layout + sayfa) tekrar istenirse `cache()` ile hafızaya alınır. */
export const getSite = cache(async (): Promise<PublicSiteResponse> => {
  const client = createServerApiClient();
  return getPublicSite(client);
});
