import { getApiErrorMessage, updateSiteSettingsFeatures, type SiteSettingsResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, CheckboxField, FormSection, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

interface FeaturesForm {
  globalSearchEnabled: boolean;
  newsletterEnabled: boolean;
  publicJobListingsEnabled: boolean;
  donationPageEnabled: boolean;
  allowSearchEngineIndexing: boolean;
}

function toForm(settings: SiteSettingsResponse): FeaturesForm {
  return {
    globalSearchEnabled: settings.globalSearchEnabled,
    newsletterEnabled: settings.newsletterEnabled,
    publicJobListingsEnabled: settings.publicJobListingsEnabled,
    donationPageEnabled: settings.donationPageEnabled,
    allowSearchEngineIndexing: settings.allowSearchEngineIndexing,
  };
}

export function FeaturesSettingsForm({ settings, onSaved }: { settings: SiteSettingsResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState<FeaturesForm>(() => toForm(settings));
  const initial = toForm(settings);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const mutation = useMutation({
    mutationFn: () => updateSiteSettingsFeatures(client, { rowVersion: settings.rowVersion, ...form }),
    onSuccess: () => {
      showToast('Özellik bayrakları güncellendi.', 'success');
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
      <FormSection title="Özellik bayrakları" description="Bazı bayraklar public siteyi etkiler; kaydedince hemen yansır.">
        <CheckboxField
          id="feature-global-search"
          label="Genel arama etkin"
          checked={form.globalSearchEnabled}
          onChange={(e) => setForm((p) => ({ ...p, globalSearchEnabled: e.target.checked }))}
        />
        <CheckboxField
          id="feature-newsletter"
          label="Bülten aboneliği etkin"
          checked={form.newsletterEnabled}
          onChange={(e) => setForm((p) => ({ ...p, newsletterEnabled: e.target.checked }))}
        />
        <CheckboxField
          id="feature-job-listings"
          label="Açık ilan listeleme etkin"
          checked={form.publicJobListingsEnabled}
          onChange={(e) => setForm((p) => ({ ...p, publicJobListingsEnabled: e.target.checked }))}
        />
        <CheckboxField
          id="feature-donation"
          label="Bağış sayfası etkin"
          checked={form.donationPageEnabled}
          onChange={(e) => setForm((p) => ({ ...p, donationPageEnabled: e.target.checked }))}
        />
        <CheckboxField
          id="feature-indexing"
          label="Arama motorlarının siteyi dizinlemesine izin ver"
          hint="Kapatılırsa public sayfalarda noindex uygulanır."
          checked={form.allowSearchEngineIndexing}
          onChange={(e) => setForm((p) => ({ ...p, allowSearchEngineIndexing: e.target.checked }))}
        />
      </FormSection>
      <Button type="button" tone="quiet" onClick={() => setForm(initial)} disabled={!isDirty || mutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || mutation.isPending}>
        {mutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
