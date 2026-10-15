import { describe, expect, it } from 'vitest';
import { isAllowedLinkHref } from './isAllowedLinkHref';

describe('isAllowedLinkHref', () => {
  it.each(['http://example.org', 'https://example.org/yol', 'mailto:ad@example.org', 'tel:+905551112233'])(
    'allows the absolute scheme %s',
    (href) => {
      expect(isAllowedLinkHref(href)).toBe(true);
    },
  );

  it.each(['/ilanlar/ornek', 'ilanlar/ornek', '#bolum', '?q=x'])('allows the relative/in-site href %s', (href) => {
    expect(isAllowedLinkHref(href)).toBe(true);
  });

  it.each([
    'javascript:alert(1)',
    'JaVaScRiPt:alert(1)',
    'data:text/html,<script>alert(1)</script>',
    'vbscript:msgbox(1)',
    '//evil.example.org/yol',
    '',
    '   ',
  ])('rejects the disallowed href %s', (href) => {
    expect(isAllowedLinkHref(href)).toBe(false);
  });
});
