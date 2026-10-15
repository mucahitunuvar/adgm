import {
  BankAccountCurrency,
  getApiErrorMessage,
  updateSiteSettingsBankAccounts,
  type SiteSettingsResponse,
  type UpdateSiteSettingsBankAccountInput,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, CheckboxField, FieldRow, FormSection, SelectField, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { Trash2 } from 'lucide-react';
import { useState, type FormEvent } from 'react';
import { enumOptions } from '../../../i18n/enumOptions';

const currencyOptions = enumOptions(BankAccountCurrency, 'BankAccountCurrency');

function toForm(settings: SiteSettingsResponse): UpdateSiteSettingsBankAccountInput[] {
  return settings.bankAccounts.map((account) => ({
    iban: account.iban,
    bankName: account.bankName,
    accountHolder: account.accountHolder,
    currency: account.currency,
    description: account.description,
    sortOrder: account.sortOrder,
    isActive: account.isActive,
  }));
}

export function BankAccountsSettingsForm({ settings, onSaved }: { settings: SiteSettingsResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [accounts, setAccounts] = useState<UpdateSiteSettingsBankAccountInput[]>(() => toForm(settings));
  const initial = toForm(settings);
  const isDirty = JSON.stringify(accounts) !== JSON.stringify(initial);

  const mutation = useMutation({
    mutationFn: () => updateSiteSettingsBankAccounts(client, { rowVersion: settings.rowVersion, bankAccounts: accounts }),
    onSuccess: () => {
      showToast('Banka hesapları güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  function updateAccount(index: number, patch: Partial<UpdateSiteSettingsBankAccountInput>) {
    setAccounts((prev) => prev.map((account, i) => (i === index ? { ...account, ...patch } : account)));
  }

  function addAccount() {
    setAccounts((prev) => [
      ...prev,
      { iban: '', bankName: '', accountHolder: '', currency: BankAccountCurrency.TRY, description: null, sortOrder: prev.length, isActive: true },
    ]);
  }

  function removeAccount(index: number) {
    setAccounts((prev) => prev.filter((_, i) => i !== index));
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <FormSection
        title="Banka hesapları"
        description="Bağış/ödeme sayfasında gösterilen hesaplar. Pasif hesaplar public'te görünmez."
        actions={
          <Button type="button" tone="secondary" onClick={addAccount}>
            Hesap ekle
          </Button>
        }
      >
        {accounts.length === 0 && <p>Henüz banka hesabı yok.</p>}
        {accounts.map((account, index) => (
          <div key={index} className="form-section" style={{ paddingTop: 0 }}>
            <FieldRow>
              <TextField id={`bank-iban-${index}`} label="IBAN" value={account.iban} onChange={(e) => updateAccount(index, { iban: e.target.value })} />
              <TextField
                id={`bank-name-${index}`}
                label="Banka adı"
                value={account.bankName}
                onChange={(e) => updateAccount(index, { bankName: e.target.value })}
              />
            </FieldRow>
            <FieldRow>
              <TextField
                id={`bank-holder-${index}`}
                label="Hesap sahibi"
                value={account.accountHolder}
                onChange={(e) => updateAccount(index, { accountHolder: e.target.value })}
              />
              <SelectField
                id={`bank-currency-${index}`}
                label="Para birimi"
                value={account.currency}
                onChange={(e) => updateAccount(index, { currency: e.target.value })}
              >
                {currencyOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </SelectField>
              <TextField
                id={`bank-sort-${index}`}
                label="Sıra"
                type="number"
                value={account.sortOrder}
                onChange={(e) => updateAccount(index, { sortOrder: Number(e.target.value) })}
              />
            </FieldRow>
            <TextField
              id={`bank-description-${index}`}
              label="Açıklama"
              value={account.description ?? ''}
              onChange={(e) => updateAccount(index, { description: e.target.value || null })}
            />
            <CheckboxField
              id={`bank-active-${index}`}
              label="Etkin"
              checked={account.isActive}
              onChange={(e) => updateAccount(index, { isActive: e.target.checked })}
            />
            <Button type="button" tone="quiet" onClick={() => removeAccount(index)}>
              <Trash2 aria-hidden="true" /> Hesabı sil
            </Button>
          </div>
        ))}
      </FormSection>
      <Button type="button" tone="quiet" onClick={() => setAccounts(initial)} disabled={!isDirty || mutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || mutation.isPending}>
        {mutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
