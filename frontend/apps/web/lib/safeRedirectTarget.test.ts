import { describe, expect, it } from 'vitest';
import { safeRedirectTarget } from './safeRedirectTarget';

describe('safeRedirectTarget', () => {
  it('rejects protocol-relative URLs ("//")', () => {
    expect(safeRedirectTarget('//evil.com')).toBeUndefined();
  });

  it('rejects backslash-prefixed paths ("/\\\\") that browsers treat as protocol-relative', () => {
    expect(safeRedirectTarget('/\\evil.com')).toBeUndefined();
  });

  it('rejects javascript: URLs', () => {
    expect(safeRedirectTarget('javascript:alert(1)')).toBeUndefined();
  });

  it('allows an absolute https URL', () => {
    expect(safeRedirectTarget('https://ok.example')).toBe('https://ok.example');
  });

  it('allows a root-relative path', () => {
    expect(safeRedirectTarget('/ok')).toBe('/ok');
  });

  it('rejects mailto/tel as a redirect target even though safeUrl allows them for href', () => {
    expect(safeRedirectTarget('mailto:test@example.com')).toBeUndefined();
    expect(safeRedirectTarget('tel:+905551112233')).toBeUndefined();
  });

  it('rejects empty/nullish values', () => {
    expect(safeRedirectTarget(null)).toBeUndefined();
    expect(safeRedirectTarget(undefined)).toBeUndefined();
    expect(safeRedirectTarget('')).toBeUndefined();
  });
});
