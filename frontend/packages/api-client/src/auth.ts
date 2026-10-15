import type { ApiClient } from './client';
import type { components } from './schema';

export type LoginResponse = components['schemas']['LoginResponse'];
export type GetCurrentUserResponse = components['schemas']['GetCurrentUserResponse'];

/** `POST /api/v1/auth/login`. 400/401/403 `ProblemDetails`; 429 boş gövdeli. */
export function login(client: ApiClient, email: string, password: string): Promise<LoginResponse> {
  return client.request('/api/v1/auth/login', { method: 'POST', body: { email, password } });
}

/** `POST /api/v1/auth/refresh-token`. Aynı refresh token iki kez gönderilmemeli (rotation). */
export function refreshAccessToken(client: ApiClient, refreshToken: string): Promise<LoginResponse> {
  return client.request('/api/v1/auth/refresh-token', { method: 'POST', body: { refreshToken } });
}

/** `POST /api/v1/auth/logout`. 204. */
export function logout(client: ApiClient, refreshToken: string): Promise<void> {
  return client.request('/api/v1/auth/logout', { method: 'POST', body: { refreshToken } });
}

/** `GET /api/v1/auth/me`. */
export function getCurrentUser(client: ApiClient): Promise<GetCurrentUserResponse> {
  return client.request('/api/v1/auth/me');
}

/** `POST /api/v1/auth/forgot-password`. Her zaman 204 döner (anti-enumeration). */
export function forgotPassword(client: ApiClient, email: string): Promise<void> {
  return client.request('/api/v1/auth/forgot-password', { method: 'POST', body: { email } });
}

/** `POST /api/v1/auth/reset-password`. */
export function resetPassword(client: ApiClient, token: string, newPassword: string): Promise<void> {
  return client.request('/api/v1/auth/reset-password', { method: 'POST', body: { token, newPassword } });
}

/** `GET /api/v1/auth/verify-email?token=`. Doğrulama e-postasındaki bağlantı için. */
export function verifyEmailByLink(client: ApiClient, token: string): Promise<void> {
  return client.request('/api/v1/auth/verify-email', { method: 'GET', query: { token } });
}

/** `POST /api/v1/auth/verify-email`. Token'ı URL'den okuyup gövdede gönderen akış için. */
export function verifyEmail(client: ApiClient, token: string): Promise<void> {
  return client.request('/api/v1/auth/verify-email', { method: 'POST', body: { token } });
}

/** `POST /api/v1/auth/resend-verification-email`. Her zaman 204 döner (anti-enumeration). */
export function resendVerificationEmail(client: ApiClient, email: string): Promise<void> {
  return client.request('/api/v1/auth/resend-verification-email', { method: 'POST', body: { email } });
}

/** `PUT /api/v1/auth/change-password`. */
export function changePassword(client: ApiClient, currentPassword: string, newPassword: string): Promise<void> {
  return client.request('/api/v1/auth/change-password', { method: 'PUT', body: { currentPassword, newPassword } });
}
