import {
  getApiErrorMessage,
  updateSiteSettingsIdentity,
  type SiteSettingsResponse,
  type UpdateSiteSettingsIdentityTranslationInput,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  FieldRow,
  FormSection,
  TextField,
  TranslationTabs,
  UnsavedChangesGuard,
  useToast,
  type TranslationLanguage,
} from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { MediaPickerField } from '../../media/MediaPickerField';
import { mergeTranslations } from '../mergeTranslations';

type Translation = UpdateSiteSettingsIdentityTranslationInput;

const EMPTY_TRANSLATION: Translation = {
  languageCode: '',
  siteName: '',
  tagline: '',
  defaultMetaTitle: '',
  defaultMetaDescription: '',
  footerText: '',
};

interface MediaIds {
  logoLightMediaAssetId: string;
  logoDarkMediaAssetId: string;
  faviconMediaAssetId: string;
  defaultOgImageMediaId: string;
}

function toMediaIds(settings: SiteSettingsResponse): MediaIds {
  return {
    logoLightMediaAssetId: settings.logoLightMediaAssetId ?? '',
    logoDarkMediaAssetId: settings.logoDarkMediaAssetId ?? '',
    faviconMediaAssetId: settings.faviconMediaAssetId ?? '',
    defaultOgImageMediaId: settings.defaultOgImageMediaId ?? '',
  };
}

export function IdentitySettingsForm({
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

  const [mediaIds, setMediaIds] = useState<MediaIds>(() => toMediaIds(settings));
  const [translations, setTranslations] = useState<Record<string, Translation>>(() =>
    mergeTranslations(settings.translations, languageCodes, EMPTY_TRANSLATION),
  );
  const [activeLanguage, setActiveLanguage] = useState(languageCodes[0] ?? '');

  const initialMediaIds = toMediaIds(settings);
  const initialTranslations = mergeTranslations(settings.translations, languageCodes, EMPTY_TRANSLATION);
  const isDirty =
    JSON.stringify(mediaIds) !== JSON.stringify(initialMediaIds) ||
    JSON.stringify(translations) !== JSON.stringify(initialTranslations);

  const mutation = useMutation({
    mutationFn: () =>
      updateSiteSettingsIdentity(client, {
        rowVersion: settings.rowVersion,
        logoLightMediaAssetId: mediaIds.logoLightMediaAssetId || null,
        logoDarkMediaAssetId: mediaIds.logoDarkMediaAssetId || null,
        faviconMediaAssetId: mediaIds.faviconMediaAssetId || null,
        defaultOgImageMediaId: mediaIds.defaultOgImageMediaId || null,
        translations: languageCodes.map((code) => ({ ...translations[code]!, languageCode: code })),
      }),
    onSuccess: () => {
      showToast('Kimlik ayarları güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  function updateTranslationField(code: string, field: keyof Translation, value: string) {
    setTranslations((prev) => ({ ...prev, [code]: { ...prev[code]!, [field]: value } }));
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <FormSection title="Logo ve görseller" description="Medya kütüphanesinden seçilir; serbest URL girilmez.">
        <FieldRow>
          <MediaPickerField
            label="Açık tema logosu"
            value={mediaIds.logoLightMediaAssetId}
            previewUrl={settings.logoLightUrl}
            onChange={(id) => setMediaIds((prev) => ({ ...prev, logoLightMediaAssetId: id }))}
          />
          <MediaPickerField
            label="Koyu tema logosu"
            value={mediaIds.logoDarkMediaAssetId}
            previewUrl={settings.logoDarkUrl}
            onChange={(id) => setMediaIds((prev) => ({ ...prev, logoDarkMediaAssetId: id }))}
          />
        </FieldRow>
        <FieldRow>
          <MediaPickerField
            label="Favicon"
            value={mediaIds.faviconMediaAssetId}
            previewUrl={settings.faviconUrl}
            onChange={(id) => setMediaIds((prev) => ({ ...prev, faviconMediaAssetId: id }))}
          />
          <MediaPickerField
            label="Varsayılan paylaşım görseli"
            value={mediaIds.defaultOgImageMediaId}
            previewUrl={settings.defaultOgImageUrl}
            onChange={(id) => setMediaIds((prev) => ({ ...prev, defaultOgImageMediaId: id }))}
          />
        </FieldRow>
      </FormSection>
      <FormSection title="Dile göre kimlik bilgileri" description="Site adı, slogan ve varsayılan SEO metinleri.">
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(code) => {
            const translation = translations[code] ?? EMPTY_TRANSLATION;
            return (
              <div>
                <TextField
                  id={`identity-site-name-${code}`}
                  label="Site adı"
                  required
                  value={translation.siteName ?? ''}
                  onChange={(event) => updateTranslationField(code, 'siteName', event.target.value)}
                />
                <TextField
                  id={`identity-tagline-${code}`}
                  label="Slogan"
                  value={translation.tagline ?? ''}
                  onChange={(event) => updateTranslationField(code, 'tagline', event.target.value)}
                />
                <TextField
                  id={`identity-meta-title-${code}`}
                  label="Varsayılan meta başlık"
                  value={translation.defaultMetaTitle ?? ''}
                  onChange={(event) => updateTranslationField(code, 'defaultMetaTitle', event.target.value)}
                />
                <TextField
                  id={`identity-meta-description-${code}`}
                  label="Varsayılan meta açıklama"
                  value={translation.defaultMetaDescription ?? ''}
                  onChange={(event) => updateTranslationField(code, 'defaultMetaDescription', event.target.value)}
                />
                <TextField
                  id={`identity-footer-${code}`}
                  label="Alt bilgi (footer) metni"
                  value={translation.footerText ?? ''}
                  onChange={(event) => updateTranslationField(code, 'footerText', event.target.value)}
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
          setMediaIds(initialMediaIds);
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
