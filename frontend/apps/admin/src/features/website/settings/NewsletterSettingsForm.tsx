import { getApiErrorMessage, LegalDocumentKind, updateSiteSettingsNewsletter, type SiteSettingsResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FormSection, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { LegalDocumentSelector } from '../shared/LegalDocumentSelector';

export function NewsletterSettingsForm({ settings, onSaved }: { settings: SiteSettingsResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [key, setKey] = useState(settings.newsletterPrivacyNoticeKey ?? '');
  const initial = settings.newsletterPrivacyNoticeKey ?? '';
  const isDirty = key !== initial;

  const mutation = useMutation({
    mutationFn: () => updateSiteSettingsNewsletter(client, { rowVersion: settings.rowVersion, newsletterPrivacyNoticeKey: key || null }),
    onSuccess: () => {
      showToast('Bülten ayarları güncellendi.', 'success');
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
        title="Bülten"
        description="Bülten aboneliği formunda gösterilecek gizlilik bildirimi. Yayınlı bir sürümü olmayan bir anahtar seçilirse bülten formu public sitede kapalı kalır."
      >
        <LegalDocumentSelector
          idPrefix="newsletter"
          label="Gizlilik bildirimi"
          kind={LegalDocumentKind.PrivacyNotice}
          value={key || null}
          onChange={(next) => setKey(next ?? '')}
        />
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
