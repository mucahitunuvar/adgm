import { describe, expect, it } from 'vitest';
import { serializeJsonLd } from './serializeJsonLd';

describe('serializeJsonLd', () => {
  it('escapes a script-closing tag so it cannot break out of the <script> element', () => {
    const value = { name: '</script><script>alert(1)</script>' };
    const serialized = serializeJsonLd(value);
    expect(serialized).not.toContain('</script>');
    expect(serialized).toContain('\\u003c/script\\u003e');
  });

  it('escapes ampersands and line separators', () => {
    expect(serializeJsonLd({ a: 'x & y' })).toContain('\\u0026');
    expect(serializeJsonLd({ a: '\u2028\u2029' })).toContain('\\u2028');
  });

  it('round-trips plain values unaffected by escaping', () => {
    const value = { '@type': 'Organization', name: 'Ahmet Demir Gençlik Merkezi' };
    expect(JSON.parse(serializeJsonLd(value).replace(/\\u003c/g, '<').replace(/\\u003e/g, '>'))).toEqual(value);
  });
});
