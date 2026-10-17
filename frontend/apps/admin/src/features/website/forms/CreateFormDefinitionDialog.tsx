import { createFormDefinition, getApiErrorMessage, LegalDocumentKind, type CreateFormDefinitionRequest } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, FieldRow, FormSection, TextAreaField, TextField, useToast } from '@gm/ui';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../routes';
import { LegalDocumentSelector } from '../shared/LegalDocumentSelector';

const EMPTY_FORM: CreateFormDefinitionRequest = {
  key: '',
  retentionDays: null,
  notificationEmails: [],
  privacyNoticeKey: null,
  explicitConsents: [],
  defaultLanguageTitle: '',
  defaultLanguageDescription: '',
  defaultLanguageSuccessMessage: '',
  defaultLanguageSubmitButtonLabel: '',
};

export function CreateFormDefinitionDialog({ open, onCancel }: { open: boolean; onCancel: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [form, setForm] = useState<CreateFormDefinitionRequest>(EMPTY_FORM);
  const [emailsText, setEmailsText] = useState('');

  const mutation = useMutation({
    mutationFn: () => createFormDefinition(client, { ...form, notificationEmails: parseEmailLines(emailsText) }),
    onSuccess: (response) => {
      showToast('Form oluşturuldu.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-forms'] });
      setForm(EMPTY_FORM);
      setEmailsText('');
      navigate(ROUTES.websiteFormDetail(response.id));
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  if (!open) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Yeni form" className="accessible-dialog--wide">
      <h2>Yeni form</h2>
      <form onSubmit={handleSubmit}>
        <FormSection title="Temel bilgiler" description="Anahtar oluşturulduktan sonra değiştirilemez.">
          <FieldRow>
            <TextField
              id="form-key"
              label="Anahtar"
              hint="ör. iletisim, is-basvurusu — küçük harf, boşluksuz."
              required
              value={form.key ?? ''}
              onChange={(event) => setForm((prev) => ({ ...prev, key: event.target.value }))}
            />
            <TextField
              id="form-default-title"
              label="Başlık (varsayılan dil)"
              required
              value={form.defaultLanguageTitle ?? ''}
              onChange={(event) => setForm((prev) => ({ ...prev, defaultLanguageTitle: event.target.value }))}
            />
          </FieldRow>
          <TextAreaField
            id="form-default-description"
            label="Açıklama (varsayılan dil)"
            value={form.defaultLanguageDescription ?? ''}
            onChange={(event) => setForm((prev) => ({ ...prev, defaultLanguageDescription: event.target.value }))}
          />
          <FieldRow>
            <TextField
              id="form-default-success"
              label="Başarı mesajı (varsayılan dil)"
              required
              value={form.defaultLanguageSuccessMessage ?? ''}
              onChange={(event) => setForm((prev) => ({ ...prev, defaultLanguageSuccessMessage: event.target.value }))}
            />
            <TextField
              id="form-default-submit-label"
              label="Gönder düğmesi metni (varsayılan dil)"
              required
              value={form.defaultLanguageSubmitButtonLabel ?? ''}
              onChange={(event) => setForm((prev) => ({ ...prev, defaultLanguageSubmitButtonLabel: event.target.value }))}
            />
          </FieldRow>
        </FormSection>
        <FormSection title="Aydınlatma metni" description="Her formun bir aydınlatma metni olmak zorundadır (backend invariant'ı) - yayınlı sürümü olmayan bir anahtar seçilirse form public'te kapalı kalır.">
          <LegalDocumentSelector
            idPrefix="form-privacy-notice"
            label="Aydınlatma metni"
            kind={LegalDocumentKind.PrivacyNotice}
            allowEmpty={false}
            value={form.privacyNoticeKey ?? null}
            onChange={(next) => setForm((prev) => ({ ...prev, privacyNoticeKey: next }))}
          />
        </FormSection>
        <FormSection title="Bildirim ve saklama" description="Yeni başvuru geldiğinde bilgilendirilecek e-posta adresleri (başvuru içeriği e-posta gövdesine yazılmaz).">
          <TextAreaField
            id="form-notification-emails"
            label="Bildirim e-postaları"
            hint="Her satıra bir e-posta."
            value={emailsText}
            onChange={(event) => setEmailsText(event.target.value)}
          />
          <TextField
            id="form-retention-days"
            label="Saklama süresi (gün)"
            type="number"
            min={1}
            hint="Boş bırakılırsa backend'in varsayımı kullanılır."
            value={form.retentionDays ?? ''}
            onChange={(event) => setForm((prev) => ({ ...prev, retentionDays: event.target.value === '' ? null : Number(event.target.value) }))}
          />
        </FormSection>
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={mutation.isPending}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Oluşturuluyor…' : 'Oluştur'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}

function parseEmailLines(text: string): string[] {
  return text
    .split(/[\n,]/)
    .map((line) => line.trim())
    .filter((line) => line !== '');
}
