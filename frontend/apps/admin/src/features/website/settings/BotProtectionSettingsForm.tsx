import { getApiErrorMessage, updateSiteSettingsBotProtection, type SiteSettingsResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, CheckboxField, FormSection, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

interface BotProtectionForm {
  botProtectionEnabled: boolean;
  turnstileSiteKey: string;
}

function toForm(settings: SiteSettingsResponse): BotProtectionForm {
  return { botProtectionEnabled: settings.botProtectionEnabled, turnstileSiteKey: settings.turnstileSiteKey };
}

export function BotProtectionSettingsForm({ settings, onSaved }: { settings: SiteSettingsResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState<BotProtectionForm>(() => toForm(settings));
  const initial = toForm(settings);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const mutation = useMutation({
    mutationFn: () =>
      updateSiteSettingsBotProtection(client, {
        rowVersion: settings.rowVersion,
        botProtectionEnabled: form.botProtectionEnabled,
        turnstileSiteKey: form.turnstileSiteKey || null,
      }),
    onSuccess: () => {
      showToast('Bot koruması güncellendi.', 'success');
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
      <FormSection
        title="Bot koruması"
        description="Yalnız Turnstile SITE anahtarı buradadır; gizli (secret) anahtar backend yapılandırmasında kalır (FRONTEND-ACIK-MADDELER.md A7)."
      >
        <CheckboxField
          id="bot-protection-enabled"
          label="Bot koruması etkin"
          checked={form.botProtectionEnabled}
          onChange={(event) => setForm((prev) => ({ ...prev, botProtectionEnabled: event.target.checked }))}
        />
        <TextField
          id="bot-protection-site-key"
          label="Turnstile site anahtarı"
          value={form.turnstileSiteKey}
          onChange={(event) => setForm((prev) => ({ ...prev, turnstileSiteKey: event.target.value }))}
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
