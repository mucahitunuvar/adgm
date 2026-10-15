import { getApiErrorMessage, updateSiteSettingsTheme, type SiteSettingsResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FormSection, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

type Theme = SiteSettingsResponse['theme'];

export function ThemeSettingsForm({
  theme,
  rowVersion,
  onSaved,
}: {
  theme: Theme;
  rowVersion: string;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState<Theme>(theme);
  const isDirty = JSON.stringify(form) !== JSON.stringify(theme);

  const mutation = useMutation({
    mutationFn: () => updateSiteSettingsTheme(client, { rowVersion, ...form }),
    onSuccess: () => {
      showToast('Tema güncellendi.', 'success');
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
      <FormSection title="Tema" description="Marka rengi ve yazı tipi; genel tasarım sistemi kararları FRONTEND-TASARIM-SISTEMI.md'dedir.">
        <TextField
          id="theme-primary"
          label="Birincil renk (hex)"
          required
          value={form.primaryColorHex}
          onChange={(event) => setForm((prev) => ({ ...prev, primaryColorHex: event.target.value }))}
        />
        <TextField
          id="theme-secondary"
          label="İkincil renk (hex)"
          required
          value={form.secondaryColorHex}
          onChange={(event) => setForm((prev) => ({ ...prev, secondaryColorHex: event.target.value }))}
        />
        <TextField
          id="theme-font"
          label="Yazı tipi ailesi"
          required
          value={form.fontFamily}
          onChange={(event) => setForm((prev) => ({ ...prev, fontFamily: event.target.value }))}
        />
      </FormSection>
      <Button type="button" tone="quiet" onClick={() => setForm(theme)} disabled={!isDirty || mutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || mutation.isPending}>
        {mutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
