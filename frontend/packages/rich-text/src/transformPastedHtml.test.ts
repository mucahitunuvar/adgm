import { describe, expect, it } from 'vitest';
import { transformPastedHtml } from './transformPastedHtml';

describe('transformPastedHtml', () => {
  it('removes img, figure and figcaption entirely, keeping surrounding text', () => {
    const input = '<p>Önce</p><figure><img src="https://evil.example.org/x.png" alt="x"><figcaption>Açıklama</figcaption></figure><p>Sonra</p>';
    const output = transformPastedHtml(input);

    expect(output).not.toContain('<img');
    expect(output).not.toContain('<figure');
    expect(output).not.toContain('<figcaption');
    expect(output).toContain('Önce');
    expect(output).toContain('Sonra');
  });

  it('removes a bare img with an onerror XSS payload', () => {
    const output = transformPastedHtml('<p>Metin</p><img src="x" onerror="alert(1)">');

    expect(output).not.toContain('onerror');
    expect(output).not.toContain('<img');
  });

  it('leaves whitelist-shaped markup untouched', () => {
    const input = '<p><strong>Kalın</strong> ve <em>italik</em></p>';
    expect(transformPastedHtml(input)).toContain('Kalın');
    expect(transformPastedHtml(input)).toContain('italik');
  });
});
