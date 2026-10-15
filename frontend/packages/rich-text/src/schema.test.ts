import { Editor } from '@tiptap/core';
import { describe, expect, it } from 'vitest';
import { richTextExtensions } from './schema';

/**
 * A11 §2/§6a: şema whitelist dışı hiçbir düğüm/işaret/öznitelik üretemez. `new Editor({ content })`
 * tam olarak ProseMirror'ın gerçek yapıştırma/HTML-ayrıştırma yolunu kullanır (bir tarayıcı
 * paste olayını taklit etmekten daha güvenilir ve deterministik) - `content` burada "kullanıcının
 * panoya kopyaladığı/yapıştırdığı ham HTML" rolünü oynar.
 */
function parseAndSerialize(html: string): string {
  const editor = new Editor({ extensions: richTextExtensions, content: html });
  const result = editor.getHTML();
  editor.destroy();
  return result;
}

describe('richTextExtensions schema - XSS ve whitelist uyumu', () => {
  const xssPayloads: Array<[string, string]> = [
    ['script etiketi', '<p>Metin</p><script>alert(1)</script>'],
    ['img onerror', '<img src="x" onerror="alert(1)">'],
    ['javascript: href', '<p><a href="javascript:alert(1)">tıkla</a></p>'],
    ['data: URI href', '<p><a href="data:text/html,<script>alert(1)</script>">tıkla</a></p>'],
    ['style özniteliği', '<p style="background:url(javascript:alert(1))">Metin</p>'],
    ['iframe/srcdoc', '<iframe srcdoc="<script>alert(1)</script>"></iframe>'],
    ['svg onload', '<svg onload="alert(1)"><circle /></svg>'],
    ['div ile class/onclick', '<div class="x" onclick="alert(1)">Metin</div>'],
  ];

  it.each(xssPayloads)('%s hiçbir biçimde editörün çıktısına sızmaz', (_label, payload) => {
    const html = parseAndSerialize(payload);

    expect(html).not.toContain('<script');
    expect(html).not.toContain('onerror');
    expect(html).not.toContain('onload');
    expect(html).not.toContain('onclick');
    expect(html).not.toContain('javascript:');
    expect(html.toLowerCase()).not.toContain('data:text/html');
    expect(html).not.toContain('style=');
    expect(html).not.toContain('<iframe');
    expect(html).not.toContain('<svg');
    expect(html).not.toContain('<div');
  });

  it('img etiketi figure/figcaption dışında (doğrudan HTML olarak ayrıştırıldığında da) kalıcı olmaz', () => {
    // Not: bu, ProseMirror'ın HTML ayrıştırmasını test eder - uygulamanın kendi yapıştırma
    // süzgeci (transformPastedHtml.ts) devrede değildir. Şema seviyesinde img'in kendisi hâlâ
    // tanımlı (figure içinde kullanılmak üzere) olduğu için BU test img'in TAMAMEN kaybolmasını
    // değil, onerror gibi whitelist dışı özniteliklerin hiçbir zaman hayatta kalmamasını doğrular -
    // gerçek "yapıştırmada img hiç görünmesin" garantisi transformPastedHtml.test.ts'te.
    const html = parseAndSerialize('<img src="https://example.org/x.png" onerror="alert(1)" onclick="alert(2)">');
    expect(html).not.toContain('onerror');
    expect(html).not.toContain('onclick');
  });

  it('desteklenen biçimlendirme (kalın, italik, başlık, liste, alıntı, tablo, hr) beklenen etiketleri üretir', () => {
    const editor = new Editor({ extensions: richTextExtensions });
    editor.commands.setContent(
      '<h2>Başlık</h2><p><strong>kalın</strong> <em>italik</em> <u>altı çizili</u> <s>üstü çizili</s></p>' +
        '<ul><li>a</li></ul><ol><li>b</li></ol><blockquote>alıntı</blockquote><hr>' +
        '<table><tr><th>h</th></tr><tr><td>d</td></tr></table>',
    );
    const html = editor.getHTML();
    editor.destroy();

    for (const tag of ['<h2', '<strong', '<em', '<u>', '<s>', '<ul', '<li', '<ol', '<blockquote', '<hr', '<table', '<th', '<td']) {
      expect(html).toContain(tag);
    }
  });

  it('h1/h5/h6 desteklenmez (yalnız h2-h4)', () => {
    const html = parseAndSerialize('<h1>bir</h1><h5>beş</h5><h6>altı</h6>');
    expect(html).not.toContain('<h1');
    expect(html).not.toContain('<h5');
    expect(html).not.toContain('<h6');
  });

  it('code/pre whitelist dışıdır, şemada hiç tanımlı değildir', () => {
    const html = parseAndSerialize('<pre><code>const x = 1;</code></pre>');
    expect(html).not.toContain('<pre');
    expect(html).not.toContain('<code');
  });

  it('iframe/youtube gömme bu dilimde kapalı - şemada hiç tanımlı değildir (A11 §3)', () => {
    const html = parseAndSerialize('<iframe src="https://www.youtube-nocookie.com/embed/abc123"></iframe>');
    expect(html).not.toContain('<iframe');
  });
});
