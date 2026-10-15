import { AccessibleDialog, Button, CheckboxField, DateTimeField, TextField, TranslationTabs, type TranslationLanguage } from '@gm/ui';
import { useState } from 'react';
import { useMediaPicker } from '../../media/useMediaPicker';
import { LinkTargetField } from '../shared/LinkTargetField';
import type { DraftSlide } from './slideTypes';

export function SlideEditDialog({
  open,
  slide,
  languages,
  onSave,
  onCancel,
}: {
  open: boolean;
  slide: DraftSlide | null;
  languages: readonly TranslationLanguage[];
  onSave: (next: DraftSlide) => void;
  onCancel: () => void;
}) {
  const [draft, setDraft] = useState<DraftSlide | null>(slide);
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');
  const { pickMedia: pickDesktopMedia, pickerElement: desktopPickerElement } = useMediaPicker();
  const { pickMedia: pickMobileMedia, pickerElement: mobilePickerElement } = useMediaPicker();

  if (!open || !draft) return null;

  function translationFor(languageCode: string) {
    return (
      draft!.translations.find((t) => t.languageCode === languageCode) ??
      { languageCode, eyebrow: null, title: '', text: null, buttonLabel: null, altTextOverride: null }
    );
  }

  function setTranslation(languageCode: string, patch: Partial<DraftSlide['translations'][number]>) {
    const existing = draft!.translations.filter((t) => t.languageCode !== languageCode);
    setDraft({ ...draft!, translations: [...existing, { ...translationFor(languageCode), ...patch }] });
  }

  async function handlePickDesktop() {
    const asset = await pickDesktopMedia();
    if (asset) setDraft({ ...draft!, desktopImageMediaId: asset.id });
  }

  async function handlePickMobile() {
    const asset = await pickMobileMedia();
    if (asset) setDraft({ ...draft!, mobileImageMediaId: asset.id });
  }

  const canSave = Boolean(draft.desktopImageMediaId) && draft.translations.some((t) => t.title.trim());

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Slide">
      <h2>Slide</h2>
      <div className="field-row">
        <Button type="button" tone="quiet" onClick={() => void handlePickDesktop()}>
          {draft.desktopImageMediaId ? 'Masaüstü görseli değiştir' : 'Masaüstü görseli seç (zorunlu)'}
        </Button>
        <Button type="button" tone="quiet" onClick={() => void handlePickMobile()}>
          {draft.mobileImageMediaId ? 'Mobil görseli değiştir' : 'Mobil görseli seç (isteğe bağlı)'}
        </Button>
      </div>
      {desktopPickerElement}
      {mobilePickerElement}

      <LinkTargetField idPrefix="slide" value={draft.link} onChange={(link) => setDraft({ ...draft, link })} />

      <div className="field-row">
        <DateTimeField
          id="slide-publish-at"
          label="Yayın başlangıcı"
          value={draft.publishAtUtc}
          onChange={(value) => setDraft({ ...draft, publishAtUtc: value })}
        />
        <DateTimeField
          id="slide-unpublish-at"
          label="Yayın bitişi"
          value={draft.unpublishAtUtc}
          onChange={(value) => setDraft({ ...draft, unpublishAtUtc: value })}
        />
      </div>
      <CheckboxField id="slide-active" label="Etkin" checked={draft.isActive} onChange={(e) => setDraft({ ...draft, isActive: e.target.checked })} />

      <TranslationTabs
        languages={languages}
        activeCode={activeLanguage}
        onChange={setActiveLanguage}
        renderPanel={(languageCode) => {
          const translation = translationFor(languageCode);
          return (
            <div>
              <TextField
                id={`slide-eyebrow-${languageCode}`}
                label="Üst başlık"
                value={translation.eyebrow ?? ''}
                onChange={(e) => setTranslation(languageCode, { eyebrow: e.target.value || null })}
              />
              <TextField
                id={`slide-title-${languageCode}`}
                label="Başlık"
                required
                value={translation.title}
                onChange={(e) => setTranslation(languageCode, { title: e.target.value })}
              />
              <TextField
                id={`slide-text-${languageCode}`}
                label="Metin"
                value={translation.text ?? ''}
                onChange={(e) => setTranslation(languageCode, { text: e.target.value || null })}
              />
              <TextField
                id={`slide-button-label-${languageCode}`}
                label="Buton etiketi"
                value={translation.buttonLabel ?? ''}
                onChange={(e) => setTranslation(languageCode, { buttonLabel: e.target.value || null })}
              />
              <TextField
                id={`slide-alt-text-${languageCode}`}
                label="Görsel alternatif metni"
                value={translation.altTextOverride ?? ''}
                onChange={(e) => setTranslation(languageCode, { altTextOverride: e.target.value || null })}
              />
            </div>
          );
        }}
      />

      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onCancel}>
          Vazgeç
        </Button>
        <Button type="button" onClick={() => onSave(draft)} disabled={!canSave}>
          Tamam
        </Button>
      </div>
    </AccessibleDialog>
  );
}
