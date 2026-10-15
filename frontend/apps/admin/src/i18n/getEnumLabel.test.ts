import { describe, expect, it } from 'vitest';
import { getEnumLabel } from './getEnumLabel';

describe('getEnumLabel', () => {
  it('returns the Turkish label for a known member', () => {
    expect(getEnumLabel('UserStatus', 'Active')).toBe('Aktif');
  });

  it('falls back to a visible placeholder for an unknown member instead of throwing', () => {
    expect(getEnumLabel('UserStatus', 'SomeFutureStatus')).toBe('Bilinmeyen (SomeFutureStatus)');
  });
});
