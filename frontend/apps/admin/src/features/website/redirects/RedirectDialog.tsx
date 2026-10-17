import { RedirectStatusCode, RedirectTargetKind, type RedirectResponse } from '@gm/api-client';
import { AccessibleDialog, Button, SelectField, TextField, type TranslationLanguage } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';
import { RedirectTargetFields, validateRedirectTargetPath, type RedirectTargetValue } from '../shared/RedirectTargetFields';

export interface RedirectDraft {
  languageCode: string;
  fromPath: string;
  target: RedirectTargetValue;
}

const EMPTY_TARGET: RedirectTargetValue = {
  targetKind: RedirectTargetKind.Path,
  targetContentItemId: null,
  targetPath: null,
  statusCode: RedirectStatusCode.MovedPermanently,
};

function toDraft(redirect: RedirectResponse | null, defaultLanguageCode: string): RedirectDraft {
  if (!redirect) return { languageCode: defaultLanguageCode, fromPath: '', target: EMPTY_TARGET };
  return {
    languageCode: redirect.languageCode,
    fromPath: redirect.fromPath,
    target: {
      targetKind: redirect.targetKind,
      targetContentItemId: redirect.targetContentItemId,
      targetPath: redirect.targetPath,
      statusCode: redirect.statusCode,
    },
  };
}

/** Çağıran taraf `key={redirect?.id ?? 'new'}` vermelidir: hedefe göre tazelenmiş durumla yeniden kurulur. */
export function RedirectDialog({
  open,
  redirect,
  languages,
  isSubmitting,
  onConfirm,
  onCancel,
}: {
  open: boolean;
  /** `null` = yeni yönlendirme oluşturma. */
  redirect: RedirectResponse | null;
  languages: readonly TranslationLanguage[];
  isSubmitting: boolean;
  onConfirm: (draft: RedirectDraft) => void;
  onCancel: () => void;
}) {
  const isEditing = Boolean(redirect);
  const [draft, setDraft] = useState<RedirectDraft>(() => toDraft(redirect, languages[0]?.code ?? ''));
  const fromPathId = useId();
  const languageId = useId();

  const targetPathError =
    draft.target.targetKind === RedirectTargetKind.Path && draft.target.targetPath
      ? validateRedirectTargetPath(draft.target.targetPath)
      : null;
  const isValid =
    draft.fromPath.trim().length > 0 &&
    (draft.target.targetKind === RedirectTargetKind.ContentItem ? Boolean(draft.target.targetContentItemId) : Boolean(draft.target.targetPath) && !targetPathError);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onConfirm(draft);
  }

  return (
    <AccessibleDialog open={open} onClose={onCancel} title={isEditing ? 'Yönlendirmeyi düzenle' : 'Yeni yönlendirme'}>
      <h2>{isEditing ? 'Yönlendirmeyi düzenle' : 'Yeni yönlendirme'}</h2>
      <form onSubmit={handleSubmit}>
        {!isEditing && (
          <>
            <SelectField
              id={languageId}
              label="Dil"
              required
              value={draft.languageCode}
              onChange={(event) => setDraft((prev) => ({ ...prev, languageCode: event.target.value }))}
            >
              {languages.map((language) => (
                <option key={language.code} value={language.code}>
                  {language.name}
                </option>
              ))}
            </SelectField>
            <TextField
              id={fromPathId}
              label="Kaynak yol"
              hint='Örn. "eski-sayfa" (başında "/" olmadan). Dil içinde benzersiz olmalıdır.'
              required
              value={draft.fromPath}
              onChange={(event) => setDraft((prev) => ({ ...prev, fromPath: event.target.value }))}
            />
          </>
        )}
        {isEditing && (
          <p className="field-hint">
            Kaynak yol: <strong>{draft.fromPath}</strong> ({draft.languageCode}) - oluşturulduktan sonra değiştirilemez.
          </p>
        )}
        <RedirectTargetFields idPrefix="redirect-dialog" value={draft.target} onChange={(target) => setDraft((prev) => ({ ...prev, target }))} />
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={isSubmitting || !isValid}>
            {isSubmitting ? 'Kaydediliyor…' : 'Kaydet'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
