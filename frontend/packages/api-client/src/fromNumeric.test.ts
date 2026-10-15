import { describe, expect, it } from 'vitest';
import { fromNumeric } from './fromNumeric';

describe('fromNumeric', () => {
  it('maps a manifest numeric code back to its member name', () => {
    expect(fromNumeric('EventRegistrationStatus', 2)).toBe('Confirmed');
  });

  it('returns null for an unknown numeric code instead of throwing', () => {
    expect(fromNumeric('EventRegistrationStatus', 999)).toBeNull();
  });
});
