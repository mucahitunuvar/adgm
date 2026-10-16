import { describe, expect, it } from 'vitest';
import { safeUrl } from './safeUrl';

describe('safeUrl', () => {
  it('allows root-relative paths', () => {
    expect(safeUrl('/haberler/x')).toBe('/haberler/x');
  });

  it('allows https/http/mailto/tel', () => {
    expect(safeUrl('https://example.com')).toBe('https://example.com');
    expect(safeUrl('http://example.com')).toBe('http://example.com');
    expect(safeUrl('mailto:test@example.com')).toBe('mailto:test@example.com');
    expect(safeUrl('tel:+905551112233')).toBe('tel:+905551112233');
  });

  it('rejects javascript: and data: schemes', () => {
    expect(safeUrl('javascript:alert(1)')).toBeUndefined();
    expect(safeUrl('data:text/html,<script>alert(1)</script>')).toBeUndefined();
  });

  it('rejects protocol-relative URLs', () => {
    expect(safeUrl('//evil.example.com')).toBeUndefined();
  });

  it('rejects empty/nullish values', () => {
    expect(safeUrl(null)).toBeUndefined();
    expect(safeUrl(undefined)).toBeUndefined();
    expect(safeUrl('')).toBeUndefined();
    expect(safeUrl('   ')).toBeUndefined();
  });
});
