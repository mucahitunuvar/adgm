import type { BlockTypeFieldDescriptor } from '@gm/api-client';

/**
 * Backend'in alan tanımı (`BlockTypeFieldDescriptor`) yalnız genel bir `type` string'i taşır
 * ("string"/"int"/"decimal"/"boolean"/"guid"/"dateTime"/"object"/"array<X>") - hangi `guid`'in medya,
 * hangisinin içerik/video/slider/rakam/kategori olduğunu ayırt etmez. Bu dosya o ayrımı AÇIKÇA,
 * alan ADI kalıplarıyla yapar (yapısal olarak güvenilir olan tek istisna: `object` alanının
 * `LinkTarget` olup olmadığı, `fields` kümesine bakarak anlaşılır - ada değil).
 *
 * Eşleşmeyen bir `guid`/`array<guid>` alanı KASITLI olarak `'unsupported'` döner - ham bir metin/GUID
 * kutusu çizmek yerine arayüz "desteklenmeyen alan" gösterir (bkz. LayoutBlockFieldsFixture.test.ts:
 * canlı block-types kataloğunun bir anlık görüntüsüne karşı "her alanın bir renderer'ı var" testi,
 * yeni/tanınmayan bir alan eklenince düşer).
 *
 * Backend'e öneri (raporda da var): alan tanımına açık bir ipucu eklense bu tablo gereksiz kalır,
 * ör. `{ "name": "imageMediaId", "type": "guid", "ref": "media" }` veya `{ "name": "body", "type":
 * "string", "richText": true }` - `ref` ∈ media|content|contentType|video|slider|metrics|category.
 */
export type FieldKind =
  | 'text'
  | 'richText'
  | 'int'
  | 'decimal'
  | 'boolean'
  | 'dateTime'
  | 'media'
  | 'mediaList'
  | 'content'
  | 'contentList'
  | 'contentType'
  | 'video'
  | 'slider'
  | 'metricList'
  | 'category'
  | 'linkTarget'
  | 'object'
  | 'arrayOfObject'
  | 'arrayOfString'
  | 'unsupported';

const GUID_FIELD_NAME_KIND: Readonly<Record<string, FieldKind>> = {
  contentItemId: 'content',
  contentTypeId: 'contentType',
  videoId: 'video',
  sliderId: 'slider',
  categoryId: 'category',
};

const GUID_ARRAY_FIELD_NAME_KIND: Readonly<Record<string, FieldKind>> = {
  contentItemIds: 'contentList',
  mediaIds: 'mediaList',
  metricIds: 'metricList',
};

const LINK_TARGET_FIELD_NAMES = ['kind', 'contentItemId', 'contentTypeId', 'internalPath', 'externalUrl'] as const;

export function isLinkTargetShape(fields: readonly BlockTypeFieldDescriptor[] | null | undefined): boolean {
  if (!fields || fields.length !== LINK_TARGET_FIELD_NAMES.length) return false;
  const names = new Set(fields.map((f) => f.name));
  return LINK_TARGET_FIELD_NAMES.every((name) => names.has(name));
}

/**
 * `blockTypeKey` isteğe bağlı ama zengin metin ayrımı için ÖNEMLİ: alan adı `body` birden fazla blok
 * türünde geçiyor (ör. `image-text.body` da var) ama yalnız `rich-text` bloğunun kendi `body`'si gerçek
 * zengin metin - başka bloklardaki aynı isimli alan düz metin kalır (bilerek: canlı katalog
 * anlık görüntüsü `blockFields.test.ts` bunu `image-text.body`'yi de yakalayarak kanıtladı).
 */
export function classifyField(field: BlockTypeFieldDescriptor, blockTypeKey?: string): FieldKind {
  switch (field.type) {
    case 'string':
      return blockTypeKey === 'rich-text' && field.name === 'body' ? 'richText' : 'text';
    case 'int':
      return 'int';
    case 'decimal':
      return 'decimal';
    case 'boolean':
      return 'boolean';
    case 'dateTime':
      return 'dateTime';
    case 'guid':
      if (field.name.endsWith('MediaId')) return 'media';
      return GUID_FIELD_NAME_KIND[field.name] ?? 'unsupported';
    case 'object':
      return isLinkTargetShape(field.fields) ? 'linkTarget' : 'object';
    case 'array<guid>':
      return GUID_ARRAY_FIELD_NAME_KIND[field.name] ?? 'unsupported';
    case 'array<string>':
      return 'arrayOfString';
    case 'array<object>':
      return 'arrayOfObject';
    default:
      return 'unsupported';
  }
}

/** Yeni bir alan/öğe eklenirken (örn. dizi öğesi, varsayılan) o türün "boş" değeri. */
export function emptyValueForField(field: BlockTypeFieldDescriptor): unknown {
  const kind = classifyField(field);
  switch (kind) {
    case 'text':
    case 'richText':
    case 'arrayOfString':
      return '';
    case 'int':
    case 'decimal':
      return 0;
    case 'boolean':
      return false;
    case 'dateTime':
      return null;
    case 'media':
    case 'content':
    case 'contentType':
    case 'video':
    case 'slider':
    case 'category':
      return null;
    case 'mediaList':
    case 'contentList':
    case 'metricList':
      return [];
    case 'linkTarget':
      return null;
    case 'object': {
      const result: Record<string, unknown> = {};
      for (const nested of field.fields ?? []) result[nested.name] = emptyValueForField(nested);
      return result;
    }
    case 'arrayOfObject':
      return [];
    case 'unsupported':
      return null;
  }
}
