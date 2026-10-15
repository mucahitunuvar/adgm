import { ApiError } from '@gm/api-client';
import { describe, expect, it } from 'vitest';
import { getAuthErrorMessage } from './authErrorMessages';

describe('getAuthErrorMessage', () => {
  it('maps a known title to its Turkish message', () => {
    const error = new ApiError(401, 'Auth.InvalidCredentials', 'Invalid email or password.');
    expect(getAuthErrorMessage(error)).toBe('E-posta veya parola hatalı.');
  });

  it('adds the Retry-After duration for 429 responses', () => {
    const error = new ApiError(429, 'Too Many Requests', undefined, undefined, undefined, 30);
    expect(getAuthErrorMessage(error)).toBe('Çok sık denediniz. Lütfen 30 saniye sonra tekrar deneyin.');
  });

  it('falls back to a generic message for 429 without Retry-After', () => {
    const error = new ApiError(429, 'Too Many Requests');
    expect(getAuthErrorMessage(error)).toBe('Çok sık denediniz. Lütfen bir süre sonra tekrar deneyin.');
  });

  it('falls back to the detail, then the title, for unknown titles', () => {
    const withDetail = new ApiError(400, 'Validation.Failed', 'Email is required.');
    expect(getAuthErrorMessage(withDetail)).toBe('Email is required.');

    const withoutDetail = new ApiError(400, 'Validation.Failed');
    expect(getAuthErrorMessage(withoutDetail)).toBe('Validation.Failed');
  });

  it('returns a generic message for non-ApiError values', () => {
    expect(getAuthErrorMessage(new Error('network down'))).toBe('Beklenmeyen bir hata oluştu.');
  });
});
