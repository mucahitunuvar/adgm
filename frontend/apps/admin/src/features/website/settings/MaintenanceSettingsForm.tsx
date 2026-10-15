import {
  getApiErrorMessage,
  updateSiteSettingsMaintenance,
  type SiteSettingsResponse,
  type UpdateSiteSettingsMaintenanceTranslationInput,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  CheckboxField,
  FormSection,
  TextAreaField,
  TranslationTabs,
  UnsavedChangesGuard,
  useToast,
  type TranslationLanguage,
} from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { mergeTranslations } from '../mergeTranslations';

type Translation = UpdateSiteSettingsMaintenanceTranslationInput;

const EMPTY_TRANSLATION: Translation = { languageCode: '', maintenanceMessage: '' };

export function MaintenanceSettingsForm({
  settings,
  languages,
  onSaved,
}: {
  settings: SiteSettingsResponse;
  languages: readonly TranslationLanguage[];
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const languageCodes = languages.map((language) => language.code);

  const [enabled, setEnabled] = useState(settings.maintenanceModeEnabled);
  const [translations, setTranslations] = useState<Record<string, Translation>>(() =>
    mergeTranslations(settings.translations, languageCodes, EMPTY_TRANSLATION),
  );
  const [activeLanguage, setActiveLanguage] = useState(languageCodes[0] ?? '');

  const initialTranslations = mergeTranslations(settings.translations, languageCodes, EMPTY_TRANSLATION);
  const isDirty = enabled !== settings.maintenanceModeEnabled || JSON.stringify(translations) !== JSON.stringify(initialTranslations);

  const mutation = useMutation({
    mutationFn: () =>
      updateSiteSettingsMaintenance(client, {
        rowVersion: settings.rowVersion,
        maintenanceModeEnabled: enabled,
        translations: languageCodes.map((code) => ({ ...translations[code]!, languageCode: code })),
      }),
    onSuccess: () => {
      showToast('Bakım ayarları güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <FormSection title="Bakım modu" description="Etkinleştirildiğinde public site 503 (Hizmet kullanılamıyor) döner.">
        <CheckboxField id="maintenance-enabled" label="Bakım modu etkin" checked={enabled} onChange={(event) => setEnabled(event.target.checked)} />
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(code) => {
            const translation = translations[code] ?? EMPTY_TRANSLATION;
            return (
              <TextAreaField
                id={`maintenance-message-${code}`}
                label="Bakım mesajı"
                rows={3}
                value={translation.maintenanceMessage ?? ''}
                onChange={(event) =>
                  setTranslations((prev) => ({ ...prev, [code]: { ...prev[code]!, maintenanceMessage: event.target.value } }))
                }
              />
            );
          }}
        />
      </FormSection>
      <Button
        type="button"
        tone="quiet"
        onClick={() => {
          setEnabled(settings.maintenanceModeEnabled);
          setTranslations(initialTranslations);
        }}
        disabled={!isDirty || mutation.isPending}
      >
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || mutation.isPending}>
        {mutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
