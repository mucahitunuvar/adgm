import {
  getApiErrorMessage,
  updateSiteSettingsCookieConsent,
  type SiteSettingsResponse,
  type UpdateSiteSettingsCookieConsentTranslationInput,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  FormSection,
  TextAreaField,
  TextField,
  TranslationTabs,
  UnsavedChangesGuard,
  useToast,
  type TranslationLanguage,
} from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { mergeTranslations } from '../mergeTranslations';

type Translation = UpdateSiteSettingsCookieConsentTranslationInput;

const EMPTY_TRANSLATION: Translation = {
  languageCode: '',
  cookieBannerTitle: '',
  cookieBannerText: '',
  cookieCategoryNecessaryDescription: '',
  cookieCategoryAnalyticsDescription: '',
  cookieCategoryMarketingDescription: '',
};

export function CookieConsentSettingsForm({
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

  const [policyKey, setPolicyKey] = useState(settings.cookiePolicyKey ?? '');
  const [translations, setTranslations] = useState<Record<string, Translation>>(() =>
    mergeTranslations(settings.translations, languageCodes, EMPTY_TRANSLATION),
  );
  const [activeLanguage, setActiveLanguage] = useState(languageCodes[0] ?? '');

  const initialPolicyKey = settings.cookiePolicyKey ?? '';
  const initialTranslations = mergeTranslations(settings.translations, languageCodes, EMPTY_TRANSLATION);
  const isDirty = policyKey !== initialPolicyKey || JSON.stringify(translations) !== JSON.stringify(initialTranslations);

  const mutation = useMutation({
    mutationFn: () =>
      updateSiteSettingsCookieConsent(client, {
        rowVersion: settings.rowVersion,
        cookiePolicyKey: policyKey || null,
        translations: languageCodes.map((code) => ({ ...translations[code]!, languageCode: code })),
      }),
    onSuccess: () => {
      showToast('Çerez ayarları güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  function updateField(code: string, field: keyof Translation, value: string) {
    setTranslations((prev) => ({ ...prev, [code]: { ...prev[code]!, [field]: value } }));
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <FormSection title="Çerez onayı" description="Çerez banner'ının metni ve kategori açıklamaları.">
        <TextField id="cookie-policy-key" label="Çerez politikası belge anahtarı" value={policyKey} onChange={(event) => setPolicyKey(event.target.value)} />
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(code) => {
            const translation = translations[code] ?? EMPTY_TRANSLATION;
            return (
              <div>
                <TextField
                  id={`cookie-banner-title-${code}`}
                  label="Banner başlığı"
                  value={translation.cookieBannerTitle ?? ''}
                  onChange={(event) => updateField(code, 'cookieBannerTitle', event.target.value)}
                />
                <TextAreaField
                  id={`cookie-banner-text-${code}`}
                  label="Banner metni"
                  rows={3}
                  value={translation.cookieBannerText ?? ''}
                  onChange={(event) => updateField(code, 'cookieBannerText', event.target.value)}
                />
                <TextAreaField
                  id={`cookie-necessary-${code}`}
                  label="Gerekli çerezler açıklaması"
                  rows={2}
                  value={translation.cookieCategoryNecessaryDescription ?? ''}
                  onChange={(event) => updateField(code, 'cookieCategoryNecessaryDescription', event.target.value)}
                />
                <TextAreaField
                  id={`cookie-analytics-${code}`}
                  label="Analitik çerezler açıklaması"
                  rows={2}
                  value={translation.cookieCategoryAnalyticsDescription ?? ''}
                  onChange={(event) => updateField(code, 'cookieCategoryAnalyticsDescription', event.target.value)}
                />
                <TextAreaField
                  id={`cookie-marketing-${code}`}
                  label="Pazarlama çerezleri açıklaması"
                  rows={2}
                  value={translation.cookieCategoryMarketingDescription ?? ''}
                  onChange={(event) => updateField(code, 'cookieCategoryMarketingDescription', event.target.value)}
                />
              </div>
            );
          }}
        />
      </FormSection>
      <Button
        type="button"
        tone="quiet"
        onClick={() => {
          setPolicyKey(initialPolicyKey);
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
