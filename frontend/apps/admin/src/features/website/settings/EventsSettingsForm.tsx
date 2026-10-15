import { getApiErrorMessage, updateSiteSettingsEvents, type SiteSettingsResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FormSection, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

export function EventsSettingsForm({ settings, onSaved }: { settings: SiteSettingsResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [key, setKey] = useState(settings.eventPrivacyNoticeKey ?? '');
  const initial = settings.eventPrivacyNoticeKey ?? '';
  const isDirty = key !== initial;

  const mutation = useMutation({
    mutationFn: () => updateSiteSettingsEvents(client, { rowVersion: settings.rowVersion, eventPrivacyNoticeKey: key || null }),
    onSuccess: () => {
      showToast('Etkinlik ayarları güncellendi.', 'success');
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
      <FormSection title="Etkinlik" description="Etkinlik kaydı formunda gösterilecek gizlilik bildirimi anahtarı.">
        <TextField id="events-privacy-key" label="Gizlilik bildirimi anahtarı" value={key} onChange={(event) => setKey(event.target.value)} />
      </FormSection>
      <Button type="button" tone="quiet" onClick={() => setKey(initial)} disabled={!isDirty || mutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || mutation.isPending}>
        {mutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
