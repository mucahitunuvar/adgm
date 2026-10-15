import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { Editor } from '@tiptap/core';
import { describe, expect, it } from 'vitest';
import { createRichTextExtensions, type WhitelistProfile } from './schema';
import { ALLOWED_LINK_SCHEMES } from './isAllowedLinkHref';

/**
 * Backend'in `docs/api/html-whitelist.v1.json` manifesti (kodun TEK kaynağı -
 * `tests/ArchitectureTests/HtmlWhitelistContractTests.cs` tarafından denetlenir) Website ve Employer
 * modülleri için ayrı kural kümeleri taşır. Bu dosya, editörün HER İKİ profilde de ürettiği
 * etiket/öznitelik kümesinin kendi modülünün `AllowedTags`/`AllowedAttributes`'inin ALT KÜMESİ
 * olduğunu doğrular (editör whitelist dışı bir şey üretemez) ve bağlantı şema listesinin birebir eşit
 * olduğunu doğrular. Manifest değişip burada beklenmeyen bir fark çıkarsa test, hangi kuralın
 * uyuşmadığını adıyla söyleyerek düşer.
 */
const __dirname = path.dirname(fileURLToPath(import.meta.url));
const manifestPath = path.join(__dirname, '..', '..', '..', '..', 'docs', 'api', 'html-whitelist.v1.json');

interface ManifestModule {
  AllowedTags: string[];
  AllowedAttributes: string[];
  AllowedHrefSchemes: string[];
  [key: string]: unknown;
}

const manifest = JSON.parse(readFileSync(manifestPath, 'utf8')) as {
  Website: ManifestModule;
  Employer: ManifestModule;
};

/**
 * Her profili tüm düğüm/işaret türlerini en az bir kez (ve her öznitelikleriyle) içeren bir örnek
 * üzerinden çalıştırır - ProseMirror'ın gerçek ayrıştırma/render yolu, elle tutulan bir etiket listesi
 * değil (bkz. schema.test.ts'teki aynı yöntem).
 */
const SAMPLE_HTML: Record<WhitelistProfile, string> = {
  website:
    '<h2>h2</h2><h3>h3</h3><h4>h4</h4>' +
    '<p><strong>b</strong> <em>i</em> <u>u</u> <s>s</s></p>' +
    '<ul><li>li</li></ul><ol><li>li</li></ol>' +
    '<blockquote>bq</blockquote>' +
    '<hr>' +
    '<table><tr><th>th</th></tr><tr><td>td</td></tr></table>' +
    '<figure><img src="https://example.org/x.png" alt="a" title="t" width="10" height="20"><figcaption>cap</figcaption></figure>' +
    '<p><a href="https://example.org" target="_blank">link</a></p>' +
    '<p>satir1<br>satir2</p>',
  employer:
    '<h2>h2</h2><h3>h3</h3><h4>h4</h4>' +
    '<p><strong>b</strong> <em>i</em> <u>u</u></p>' +
    '<ul><li>li</li></ul><ol><li>li</li></ol>' +
    '<blockquote>bq</blockquote>' +
    '<p><a href="https://example.org" target="_blank">link</a></p>' +
    '<p>satir1<br>satir2</p>',
};

function renderSample(profile: WhitelistProfile): { tags: Set<string>; attributes: Set<string> } {
  const editor = new Editor({ extensions: createRichTextExtensions(profile), content: SAMPLE_HTML[profile] });
  const html = editor.getHTML();
  editor.destroy();

  const doc = new DOMParser().parseFromString(html, 'text/html');
  const tags = new Set<string>();
  const attributes = new Set<string>();
  doc.body.querySelectorAll('*').forEach((el) => {
    tags.add(el.tagName.toLowerCase());
    for (const attr of Array.from(el.attributes)) attributes.add(attr.name);
  });

  return { tags, attributes };
}

describe('schema <-> docs/api/html-whitelist.v1.json kontrat testi', () => {
  const profiles: Array<[WhitelistProfile, 'Website' | 'Employer']> = [
    ['website', 'Website'],
    ['employer', 'Employer'],
  ];

  it.each(profiles)('%s profilinin ürettiği etiket/öznitelik kümesi, manifestteki %s kural kümesinin alt kümesidir', (profile, moduleName) => {
    const { tags, attributes } = renderSample(profile);
    const allowedTags = new Set(manifest[moduleName].AllowedTags);
    const allowedAttributes = new Set(manifest[moduleName].AllowedAttributes);

    const extraTags = [...tags].filter((tag) => !allowedTags.has(tag)).sort();
    const extraAttributes = [...attributes].filter((attr) => !allowedAttributes.has(attr)).sort();

    expect(extraTags, `${moduleName}.AllowedTags dışında üretilen etiket(ler): ${extraTags.join(', ')}`).toEqual([]);
    expect(
      extraAttributes,
      `${moduleName}.AllowedAttributes dışında üretilen öznitelik(ler): ${extraAttributes.join(', ')}`,
    ).toEqual([]);
  });

  it('bağlantı şema listesi (ALLOWED_LINK_SCHEMES), manifestteki Website ve Employer AllowedHrefSchemes ile birebir eşittir', () => {
    const website = [...manifest.Website.AllowedHrefSchemes].sort();
    const employer = [...manifest.Employer.AllowedHrefSchemes].sort();
    const editorSchemes = [...ALLOWED_LINK_SCHEMES].sort();

    expect(editorSchemes).toEqual(website);
    expect(editorSchemes).toEqual(employer);
  });

  /**
   * GÖREV 3: backend'in izin verdiği ama editörün KASITLI OLARAK üretmediği şeylerin açık listesi.
   * Bu liste, manifestteki `AllowedTags`/`AllowedAttributes` ile editörün yukarıdaki örnekten
   * ürettiği kümenin FARKINDAN türetilir - elle tutulan bir kopya değil. Manifestte yeni bir kural
   * eklenip editör bunu üretmeye başlamadan bu liste güncellenmezse (ya da tam tersi, editör bir şey
   * üretmeyi bırakıp liste güncellenmezse), aşağıdaki test düşer.
   */
  const INTENTIONALLY_UNSUPPORTED: Record<'Website' | 'Employer', { tags: string[]; attributes: string[] }> = {
    Website: {
      tags: [
        // Bold/Italic uzantıları her zaman anlamsal etiketleri (`strong`/`em`) üretir, `b`/`i`'yi
        // hiç üretmez - backend ikisini de eşdeğer kabul eder, editör yalnız birini seçer.
        'b',
        'i',
        // iframe/YouTube gömme bu dilimde kapalı (A11 §3, FRONTEND-ACIK-MADDELER.md B28) - Faz 3'te
        // CSP frame-src kararıyla birlikte ele alınacak.
        'iframe',
        // Tablo başlık hücreleri (`<th>`) ayrı bir `<thead>` sarmalayıcısı olmadan `<tbody>` içinde
        // üretilir (bkz. TableNode.ts) - backend ayrı `thead` sarmalayıcısını da kabul eder ama
        // editör bunu hiç üretmez.
        'thead',
      ],
      attributes: [],
    },
    Employer: {
      // Bold/Italic uzantıları her zaman `strong`/`em` üretir (bkz. yukarıdaki Website notu) -
      // Employer'da tablo/thead yok, bu yüzden listesi daha kısa.
      tags: ['b', 'i'],
      // `LinkMark.ts`'in `addAttributes()`'i `title`'ı hiç tanımlamaz (yalnız href/target) - Website
      // profilinde `title` yalnız `img` üzerinden üretilir, Employer'da img/figure yok, bu yüzden
      // `title` burada tamamen kullanılmaz.
      attributes: ['title'],
    },
  };

  it.each(profiles)('%s profilinde manifestin izin verip editörün kasıtlı olarak üretmediği şeyler belgelenen listeyle eşleşir', (profile, moduleName) => {
    const { tags, attributes } = renderSample(profile);
    const allowedTags = [...manifest[moduleName].AllowedTags].sort();
    const allowedAttributes = [...manifest[moduleName].AllowedAttributes].sort();

    const unusedTags = allowedTags.filter((tag) => !tags.has(tag));
    const unusedAttributes = allowedAttributes.filter((attr) => !attributes.has(attr));

    expect(unusedTags, 'Yeni bir fark bulundu - üstteki INTENTIONALLY_UNSUPPORTED listesini güncelleyin.').toEqual(
      INTENTIONALLY_UNSUPPORTED[moduleName].tags,
    );
    expect(
      unusedAttributes,
      'Yeni bir fark bulundu - üstteki INTENTIONALLY_UNSUPPORTED listesini güncelleyin.',
    ).toEqual(INTENTIONALLY_UNSUPPORTED[moduleName].attributes);
  });

  it('manifestin iki modülünün de, editörün doğrudan tüketmediği (yalnız backend icin olan) kuralları belgelenmiş durumda', () => {
    const consumedKeys = new Set(['AllowedTags', 'AllowedAttributes', 'AllowedHrefSchemes']);
    const websiteOnlyKeys = Object.keys(manifest.Website)
      .filter((key) => !consumedKeys.has(key))
      .sort();
    const employerOnlyKeys = Object.keys(manifest.Employer)
      .filter((key) => !consumedKeys.has(key))
      .sort();

    // RelValuesOnBlankTarget: `rel` özniteliğini backend üretir (bkz. LinkMark.ts), editör hiç basmaz.
    // Website'e özel: UnwrappableTags/AllowedIframeHost/AllowedIframeEmbedPathPrefix/
    // ImageSourcePathTraversalTokens - backend'in KENDİ sanitizer'ının giriş toleransı/iframe-src
    // doğrulaması, editörün şemasıyla ilgisi yok (editör bu etiketleri hiç üretmez).
    expect(websiteOnlyKeys).toEqual([
      'AllowedIframeEmbedPathPrefix',
      'AllowedIframeHost',
      'ImageSourcePathTraversalTokens',
      'RelValuesOnBlankTarget',
      'UnwrappableTags',
    ]);
    expect(employerOnlyKeys).toEqual(['RelValuesOnBlankTarget']);
  });
});
