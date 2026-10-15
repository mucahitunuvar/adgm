import { getApiErrorMessage, updateSiteSettingsSubmissions, type SiteSettingsResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FormSection, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

export function SubmissionsSettingsForm({ settings, onSaved }: { settings: SiteSettingsResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [prefix, setPrefix] = useState(settings.submissionReferencePrefix);
  const initial = settings.submissionReferencePrefix;
  const isDirty = prefix !== initial;

  const mutation = useMutation({
    mutationFn: () => updateSiteSettingsSubmissions(client, { rowVersion: settings.rowVersion, submissionReferencePrefix: prefix }),
    onSuccess: () => {
      showToast('Gönderim ayarları güncellendi.', 'success');
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
      <FormSection title="Gönderim referansı" description="Form gönderimlerine atanan referans numarasının öneki (ör. GM-2026-0001).">
        <TextField id="submissions-prefix" label="Referans öneki" required value={prefix} onChange={(event) => setPrefix(event.target.value)} />
      </FormSection>
      <Button type="button" tone="quiet" onClick={() => setPrefix(initial)} disabled={!isDirty || mutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || mutation.isPending}>
        {mutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
