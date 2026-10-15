import {
  getApiErrorMessage,
  updateSiteSettingsContact,
  type SiteSettingsResponse,
  type UpdateSiteSettingsSocialLinkInput,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FieldRow, FormSection, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { Trash2 } from 'lucide-react';
import { useState, type FormEvent } from 'react';

interface ContactForm {
  address: string;
  phone: string;
  email: string;
  whatsApp: string;
  mapEmbedUrl: string;
  socialLinks: UpdateSiteSettingsSocialLinkInput[];
}

function toForm(settings: SiteSettingsResponse): ContactForm {
  return {
    address: settings.contact.address,
    phone: settings.contact.phone,
    email: settings.contact.email,
    whatsApp: settings.contact.whatsApp,
    mapEmbedUrl: settings.contact.mapEmbedUrl,
    socialLinks: settings.socialLinks.map((link) => ({ platform: link.platform, url: link.url, sortOrder: link.sortOrder })),
  };
}

export function ContactSettingsForm({ settings, onSaved }: { settings: SiteSettingsResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState<ContactForm>(() => toForm(settings));
  const initial = toForm(settings);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const mutation = useMutation({
    mutationFn: () => updateSiteSettingsContact(client, { rowVersion: settings.rowVersion, ...form }),
    onSuccess: () => {
      showToast('İletişim bilgileri güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  function updateSocialLink(index: number, patch: Partial<UpdateSiteSettingsSocialLinkInput>) {
    setForm((prev) => ({
      ...prev,
      socialLinks: prev.socialLinks.map((link, i) => (i === index ? { ...link, ...patch } : link)),
    }));
  }

  function addSocialLink() {
    setForm((prev) => ({
      ...prev,
      socialLinks: [...prev.socialLinks, { platform: '', url: '', sortOrder: prev.socialLinks.length }],
    }));
  }

  function removeSocialLink(index: number) {
    setForm((prev) => ({ ...prev, socialLinks: prev.socialLinks.filter((_, i) => i !== index) }));
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <FormSection title="İletişim bilgileri" description="Bu bilgiler tüm diller için ortaktır (dile göre çevrilmez).">
        <FieldRow>
          <TextField id="contact-address" label="Adres" value={form.address} onChange={(e) => setForm((p) => ({ ...p, address: e.target.value }))} />
          <TextField id="contact-phone" label="Telefon" value={form.phone} onChange={(e) => setForm((p) => ({ ...p, phone: e.target.value }))} />
        </FieldRow>
        <FieldRow>
          <TextField id="contact-email" label="E-posta" type="email" value={form.email} onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))} />
          <TextField id="contact-whatsapp" label="WhatsApp" value={form.whatsApp} onChange={(e) => setForm((p) => ({ ...p, whatsApp: e.target.value }))} />
        </FieldRow>
        <TextField
          id="contact-map"
          label="Harita gömme adresi (iframe src)"
          value={form.mapEmbedUrl}
          onChange={(e) => setForm((p) => ({ ...p, mapEmbedUrl: e.target.value }))}
        />
      </FormSection>
      <FormSection
        title="Sosyal medya bağlantıları"
        description="Sıra, listelenme önceliğidir (küçük sayı önce gösterilir)."
        actions={
          <Button type="button" tone="secondary" onClick={addSocialLink}>
            Bağlantı ekle
          </Button>
        }
      >
        {form.socialLinks.length === 0 && <p>Henüz sosyal medya bağlantısı yok.</p>}
        {form.socialLinks.map((link, index) => (
          <FieldRow key={index}>
            <TextField
              id={`social-platform-${index}`}
              label="Platform"
              value={link.platform}
              onChange={(e) => updateSocialLink(index, { platform: e.target.value })}
            />
            <TextField id={`social-url-${index}`} label="URL" value={link.url} onChange={(e) => updateSocialLink(index, { url: e.target.value })} />
            <TextField
              id={`social-sort-${index}`}
              label="Sıra"
              type="number"
              value={link.sortOrder}
              onChange={(e) => updateSocialLink(index, { sortOrder: Number(e.target.value) })}
            />
            <Button type="button" tone="quiet" onClick={() => removeSocialLink(index)} aria-label={`${link.platform || 'Bağlantıyı'} sil`}>
              <Trash2 aria-hidden="true" />
            </Button>
          </FieldRow>
        ))}
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
