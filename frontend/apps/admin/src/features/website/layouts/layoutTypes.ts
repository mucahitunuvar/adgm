import type { BlockTypeResponse, LayoutBlockInput, LayoutBlockResponse } from '@gm/api-client';
import { emptyValueForField } from './blockFields';

export interface DraftBlock {
  /** Yalnız React listesi/`ReorderList` anahtarı için - backend'e gitmez. İki blok aynı türde VE aynı
      ayarlarla olabilir (ör. iki boş "rich-text"), bu yüzden `blockTypeKey + JSON.stringify(settings)`
      güvenilir bir anahtar değil - her taslak bloğun kendi sabit, rastgele üretilmiş kimliği var. */
  tempKey: string;
  blockTypeKey: string;
  isActive: boolean;
  settings: unknown;
  /** Dil koduna göre - her dilin kendi metin nesnesi. */
  texts: Record<string, unknown>;
}

export function draftBlocksFromResponse(blocks: readonly LayoutBlockResponse[]): DraftBlock[] {
  return blocks.map((block) => ({
    tempKey: crypto.randomUUID(),
    blockTypeKey: block.blockTypeKey,
    isActive: block.isActive,
    settings: block.settings,
    texts: (block.texts as Record<string, unknown>) ?? {},
  }));
}

export function draftBlocksToInput(blocks: readonly DraftBlock[]): LayoutBlockInput[] {
  // `texts`'in üretilen tipi (`Record<string, never>`) openapi-typescript'in `IReadOnlyDictionary<string,
  // JsonElement>`'e somut bir şema veremediği için ortaya çıkan bir yapaylık - gerçek şekli
  // `Record<string, unknown>`, backend JSON'u aynen kabul ediyor.
  return blocks.map((block, index) => ({
    blockTypeKey: block.blockTypeKey,
    sortOrder: index,
    isActive: block.isActive,
    settings: block.settings,
    texts: block.texts as LayoutBlockInput['texts'],
  }));
}

export function newDraftBlock(blockType: BlockTypeResponse, languageCodes: readonly string[]): DraftBlock {
  const settings: Record<string, unknown> = {};
  for (const field of blockType.settingsFields) settings[field.name] = emptyValueForField(field);

  const texts: Record<string, unknown> = {};
  for (const languageCode of languageCodes) {
    const languageTexts: Record<string, unknown> = {};
    for (const field of blockType.textsFields) languageTexts[field.name] = emptyValueForField(field);
    texts[languageCode] = languageTexts;
  }

  return { tempKey: crypto.randomUUID(), blockTypeKey: blockType.key, isActive: true, settings, texts };
}
