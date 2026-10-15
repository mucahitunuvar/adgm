import type { ApiClient } from './client';
import type { components } from './schema';

export type MenuResponse = components['schemas']['MenuResponse'];
export type MenuItemResponse = components['schemas']['MenuItemResponse'];
export type MenuItemTranslationResponse = components['schemas']['MenuItemTranslationResponse'];
export type MenuItemLinkResponse = components['schemas']['MenuItemLinkResponse'];
export type MenuItemTreeInput = components['schemas']['MenuItemTreeInput'];
export type MenuItemTranslationInput = components['schemas']['MenuItemTranslationInput'];
export type MenuItemLinkInput = components['schemas']['MenuItemLinkInput'];
export type ReplaceMenuItemsRequest = components['schemas']['ReplaceMenuItemsRequest'];

/** `GET /api/v1/admin/website/menus` - her konum (Header/Utility/Footer) için bir `MenuResponse`. */
export function getMenus(client: ApiClient): Promise<MenuResponse[]> {
  return client.request('/api/v1/admin/website/menus');
}

export function getMenuByLocation(client: ApiClient, location: string): Promise<MenuResponse> {
  return client.request(`/api/v1/admin/website/menus/${encodeURIComponent(location)}`);
}

/**
 * `PUT .../menus/{location}`. `items` düz liste (iç içe `children` değil) - her öğe kendi
 * `parentTempId`'siyle ağaçtaki yerini belirtir; yeni öğeler için `tempId` istemci tarafında üretilir
 * (zaten var olan öğeler için kendi gerçek `id`'si `tempId` olarak kullanılabilir). Backend `rowVersion`
 * eşleşmezse 409 döner (eşzamanlılık koruması var).
 */
export function replaceMenuItems(client: ApiClient, location: string, body: ReplaceMenuItemsRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/menus/${encodeURIComponent(location)}`, { method: 'PUT', body });
}
