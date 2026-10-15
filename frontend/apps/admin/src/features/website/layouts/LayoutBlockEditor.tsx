import type { BlockTypeResponse } from '@gm/api-client';
import { Button, CheckboxField, TranslationTabs, type TranslationLanguage } from '@gm/ui';
import { useState } from 'react';
import { BlockFieldRenderer } from './BlockFieldRenderer';
import type { DraftBlock } from './layoutTypes';

/** Taslak bloklardan BİRİNİN tüm düzenleme arayüzü - `settings` (dil-bağımsız) ve `texts` (dil bazlı). */
export function LayoutBlockEditor({
  block,
  blockType,
  languages,
  idPrefix,
  onChange,
  onRemove,
}: {
  block: DraftBlock;
  blockType: BlockTypeResponse | undefined;
  languages: readonly TranslationLanguage[];
  idPrefix: string;
  onChange: (next: DraftBlock) => void;
  onRemove: () => void;
}) {
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');

  if (!blockType) {
    return (
      <div className="field-row">
        <p className="field-hint" role="note">
          {`"${block.blockTypeKey}" - bu blok türü artık kod tabanlı katalogda yok (kaldırılmış olabilir). Düzenlenemiyor.`}
        </p>
        <Button type="button" tone="danger" onClick={onRemove}>
          Kaldır
        </Button>
      </div>
    );
  }

  const settings = (block.settings as Record<string, unknown>) ?? {};

  return (
    <div>
      <div className="field-row" style={{ alignItems: 'center' }}>
        <strong>{blockType.key}</strong>
        <CheckboxField
          id={`${idPrefix}-active`}
          label="Etkin"
          checked={block.isActive}
          onChange={(e) => onChange({ ...block, isActive: e.target.checked })}
        />
        <Button type="button" tone="danger" onClick={onRemove}>
          Bloğu kaldır
        </Button>
      </div>

      {blockType.settingsFields.length > 0 && (
        <fieldset className="form-section">
          <legend>Ayarlar</legend>
          {blockType.settingsFields.map((field) => (
            <BlockFieldRenderer
              key={field.name}
              idPrefix={`${idPrefix}-settings-${field.name}`}
              blockTypeKey={blockType.key}
              field={field}
              value={settings[field.name]}
              siblingValues={settings}
              onChange={(nextValue) => onChange({ ...block, settings: { ...settings, [field.name]: nextValue } })}
            />
          ))}
        </fieldset>
      )}

      {blockType.textsFields.length > 0 && (
        <fieldset className="form-section">
          <legend>Metinler (dil bazlı)</legend>
          <TranslationTabs
            languages={languages}
            activeCode={activeLanguage}
            onChange={setActiveLanguage}
            renderPanel={(languageCode) => {
              const texts = (block.texts[languageCode] as Record<string, unknown>) ?? {};
              return (
                <div>
                  {blockType.textsFields.map((field) => (
                    <BlockFieldRenderer
                      key={field.name}
                      idPrefix={`${idPrefix}-texts-${languageCode}-${field.name}`}
                      blockTypeKey={blockType.key}
                      field={field}
                      value={texts[field.name]}
                      siblingValues={texts}
                      onChange={(nextValue) =>
                        onChange({ ...block, texts: { ...block.texts, [languageCode]: { ...texts, [field.name]: nextValue } } })
                      }
                    />
                  ))}
                </div>
              );
            }}
          />
        </fieldset>
      )}
    </div>
  );
}
