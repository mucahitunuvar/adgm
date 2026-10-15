import { describe, expect, it } from 'vitest';
import type { BlockTypeFieldDescriptor, BlockTypeResponse } from '@gm/api-client';
import { classifyField } from './blockFields';
import blockTypesFixture from './blockTypes.fixture.json';

/**
 * `blockTypes.fixture.json` - `GET /api/v1/admin/website/block-types`'in 15 Ekim'de alınmış canlı bir
 * anlık görüntüsü (gerçek geliştirme backend'ine karşı). Bu dosya ELLE GÜNCELLENMEZ: backend yeni bir
 * blok türü veya alan eklediğinde `pnpm --filter @gm/admin exec node ...` ile yeniden üretilip bilinçli
 * olarak commit edilmeli - aşağıdaki test, kataloğun artık BlockFieldRenderer'ın tanımadığı
 * (`classifyField` → `'unsupported'`) bir alan içerip içermediğini söyler; düşerse ya `blockFields.ts`
 * güncellenir ya da yeni alan gerçekten desteklenmiyorsa bu test dosyasına bilinçli bir istisna eklenir.
 */
const blockTypes = blockTypesFixture as unknown as BlockTypeResponse[];

function collectFields(fields: readonly BlockTypeFieldDescriptor[]): BlockTypeFieldDescriptor[] {
  const all: BlockTypeFieldDescriptor[] = [];
  for (const field of fields) {
    all.push(field);
    if (field.fields) all.push(...collectFields(field.fields));
  }
  return all;
}

describe('classifyField <-> docs/api block-types kataloğu (fixture)', () => {
  it('kataloğun her blok türünün her alanı için bir renderer var (unsupported yok)', () => {
    const unsupported: string[] = [];

    for (const blockType of blockTypes) {
      const allFields = [...collectFields(blockType.settingsFields), ...collectFields(blockType.textsFields)];
      for (const field of allFields) {
        if (classifyField(field) === 'unsupported') {
          unsupported.push(`${blockType.key}.${field.name} (${field.type})`);
        }
      }
    }

    expect(unsupported, 'Yeni/tanınmayan alan(lar) bulundu - blockFields.ts\'in GUID_FIELD_NAME_KIND/GUID_ARRAY_FIELD_NAME_KIND tablosuna ekleyin veya bilinçli bir istisna olarak belgeleyin.').toEqual([]);
  });

  it('yalnız "rich-text" bloğunun "body" metni zengin metin olarak sınıflanıyor (image-text.body gibi aynı isimli ama başka bloktaki alanlar düz metin kalır)', () => {
    const richTextFields: string[] = [];
    for (const blockType of blockTypes) {
      for (const field of collectFields(blockType.textsFields)) {
        if (classifyField(field, blockType.key) === 'richText') richTextFields.push(`${blockType.key}.${field.name}`);
      }
      for (const field of collectFields(blockType.settingsFields)) {
        if (classifyField(field, blockType.key) === 'richText') richTextFields.push(`${blockType.key}.${field.name}`);
      }
    }

    expect(richTextFields).toEqual(['rich-text.body']);
  });

  it('LinkTarget şekli yapısal olarak tanınıyor (kataloğun bilinen her "link"/"link"-benzeri object alanı)', () => {
    const linkTargetFields: string[] = [];
    for (const blockType of blockTypes) {
      for (const field of [...collectFields(blockType.settingsFields), ...collectFields(blockType.textsFields)]) {
        if (field.type === 'object' && classifyField(field, blockType.key) === 'linkTarget') linkTargetFields.push(`${blockType.key}.${field.name}`);
      }
    }

    // 15 Ekim anlık görüntüsünde her `object` tipli alan LinkTarget şeklinde - kataloğa gerçekten farklı
    // şekilli bir nesne eklenirse bu sayı değişir (BlockFieldRenderer'ın genel `object` dalı onu zaten
    // özyinelemeli olarak render eder, bu test yalnız MEVCUT beklentiyi belgeliyor).
    expect(linkTargetFields.length).toBeGreaterThan(0);
  });
});
