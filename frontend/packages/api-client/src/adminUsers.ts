import type { ApiClient } from './client';
import type { components } from './schema';

export type AdminGetUsersResponse = components['schemas']['AdminGetUsersResponse'];
export type AdminUserListItemResponse = components['schemas']['AdminUserListItemResponse'];
export type AdminUserDetailResponse = components['schemas']['AdminUserDetailResponse'];

export interface ListUsersParams {
  email?: string;
  role?: string;
  status?: string;
  isLockedOut?: boolean;
  emailConfirmed?: boolean;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/auth/admin/users`. Yalnız Admin. */
export function listUsers(client: ApiClient, params: ListUsersParams = {}): Promise<AdminGetUsersResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/auth/admin/users', { query });
}

/** `GET /api/v1/auth/admin/users/{userId}`. Yalnız Admin. */
export function getUser(client: ApiClient, userId: string): Promise<AdminUserDetailResponse> {
  return client.request(`/api/v1/auth/admin/users/${encodeURIComponent(userId)}`);
}

/** `PUT /api/v1/auth/admin/users/{userId}/role`. Yalnız Admin. */
export function changeUserRole(client: ApiClient, userId: string, newRole: string): Promise<void> {
  return client.request(`/api/v1/auth/admin/users/${encodeURIComponent(userId)}/role`, {
    method: 'PUT',
    body: { newRole },
  });
}

/** `PUT /api/v1/auth/admin/users/{userId}/deactivate`. Yalnız Admin. */
export function deactivateUser(client: ApiClient, userId: string): Promise<void> {
  return client.request(`/api/v1/auth/admin/users/${encodeURIComponent(userId)}/deactivate`, { method: 'PUT' });
}

/** `PUT /api/v1/auth/admin/users/{userId}/reactivate`. Yalnız Admin. */
export function reactivateUser(client: ApiClient, userId: string): Promise<void> {
  return client.request(`/api/v1/auth/admin/users/${encodeURIComponent(userId)}/reactivate`, { method: 'PUT' });
}

/** `PUT /api/v1/auth/admin/users/{userId}/unlock`. Yalnız Admin. */
export function unlockUser(client: ApiClient, userId: string): Promise<void> {
  return client.request(`/api/v1/auth/admin/users/${encodeURIComponent(userId)}/unlock`, { method: 'PUT' });
}
