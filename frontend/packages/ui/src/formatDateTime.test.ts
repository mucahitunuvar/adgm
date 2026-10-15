import { describe, expect, it } from 'vitest';
import { formatDateTime } from './formatDateTime';

describe('formatDateTime', () => {
  it('formats an explicit UTC timestamp in Europe/Istanbul time', () => {
    expect(formatDateTime('2026-01-15T10:00:00Z')).toBe('15.01.2026 13:00');
  });

  it('treats a timestamp without a timezone suffix as UTC (not local time)', () => {
    // Backend bazen ISO gövdesini "Z" olmadan döner (örn. GetCurrentUserResponse.createdAtUtc);
    // yine de UTC olarak ele alınmalı.
    expect(formatDateTime('2026-01-15T10:00:00')).toBe('15.01.2026 13:00');
  });

  it('respects an explicit offset if one is present', () => {
    expect(formatDateTime('2026-01-15T10:00:00+00:00')).toBe('15.01.2026 13:00');
  });
});
