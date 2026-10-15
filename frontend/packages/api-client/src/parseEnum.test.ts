import { describe, expect, it } from 'vitest';
import { parseEnum } from './parseEnum';
import { UserStatus } from './enums.generated';

describe('parseEnum', () => {
  it('returns the known member name unchanged', () => {
    expect(parseEnum(UserStatus, 'Active')).toBe('Active');
  });

  it('returns null for an unknown value instead of throwing', () => {
    expect(parseEnum(UserStatus, 'SomeFutureStatus')).toBeNull();
  });

  it('returns null for null/undefined', () => {
    expect(parseEnum(UserStatus, null)).toBeNull();
    expect(parseEnum(UserStatus, undefined)).toBeNull();
  });
});
