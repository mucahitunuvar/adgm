import { AccessibleDialog, Button, CheckboxField, TextField, TranslationTabs, type TranslationLanguage } from '@gm/ui';
import { useState } from 'react';
import { LinkTargetField } from '../shared/LinkTargetField';
import type { DraftMenuItem } from './menuTree';

export function MenuItemEditDialog({
  open,
  item,
  languages,
  onSave,
  onCancel,
}: {
  open: boolean;
  item: DraftMenuItem | null;
  languages: readonly TranslationLanguage[];
  onSave: (next: DraftMenuItem) => void;
  onCancel: () => void;
}) {
  const [draft, setDraft] = useState<DraftMenuItem | null>(item);
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');

  if (!open || !draft) return null;

  function labelFor(languageCode: string): string {
    return draft!.translations.find((t) => t.languageCode === languageCode)?.label ?? '';
  }

  function setLabel(languageCode: string, label: string) {
    const existing = draft!.translations.filter((t) => t.languageCode !== languageCode);
    setDraft({ ...draft!, translations: [...existing, { languageCode, label }] });
  }

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Menü öğesi">
      <h2>Menü öğesi</h2>
      <TranslationTabs
        languages={languages}
        activeCode={activeLanguage}
        onChange={setActiveLanguage}
        renderPanel={(languageCode) => (
          <TextField
            id={`menu-item-label-${languageCode}`}
            label="Etiket"
            required
            value={labelFor(languageCode)}
            onChange={(event) => setLabel(languageCode, event.target.value)}
          />
        )}
      />
      <LinkTargetField idPrefix="menu-item" value={draft.link} onChange={(link) => setDraft({ ...draft, link })} />
      <TextField
        id="menu-item-icon"
        label="İkon anahtarı"
        value={draft.iconKey ?? ''}
        onChange={(event) => setDraft({ ...draft, iconKey: event.target.value || null })}
      />
      <CheckboxField
        id="menu-item-new-tab"
        label="Yeni sekmede aç"
        checked={draft.openInNewTab}
        onChange={(event) => setDraft({ ...draft, openInNewTab: event.target.checked })}
      />
      <CheckboxField
        id="menu-item-active"
        label="Etkin"
        checked={draft.isActive}
        onChange={(event) => setDraft({ ...draft, isActive: event.target.checked })}
      />
      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onCancel}>
          Vazgeç
        </Button>
        <Button type="button" onClick={() => onSave(draft)} disabled={draft.translations.every((t) => !t.label.trim())}>
          Tamam
        </Button>
      </div>
    </AccessibleDialog>
  );
}
