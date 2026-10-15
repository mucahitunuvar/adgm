import { getApiErrorMessage, getSiteLanguages, getSiteSettings } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { ErrorState, LoadingState, PageHeader, Tabs, type TabItem } from '@gm/ui';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { BankAccountsSettingsForm } from './BankAccountsSettingsForm';
import { BotProtectionSettingsForm } from './BotProtectionSettingsForm';
import { ContactSettingsForm } from './ContactSettingsForm';
import { CookieConsentSettingsForm } from './CookieConsentSettingsForm';
import { EventsSettingsForm } from './EventsSettingsForm';
import { FeaturesSettingsForm } from './FeaturesSettingsForm';
import { IdentitySettingsForm } from './IdentitySettingsForm';
import { MaintenanceSettingsForm } from './MaintenanceSettingsForm';
import { NewsletterSettingsForm } from './NewsletterSettingsForm';
import { SubmissionsSettingsForm } from './SubmissionsSettingsForm';
import { ThemeSettingsForm } from './ThemeSettingsForm';

export const SETTINGS_QUERY_KEY = ['website-settings'] as const;

export function SettingsPage() {
  const { client } = useAuth();
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState('identity');

  const { data, isLoading, error } = useQuery({
    queryKey: SETTINGS_QUERY_KEY,
    queryFn: () => getSiteSettings(client),
  });
  const { data: languagesData } = useQuery({
    queryKey: ['website-languages'],
    queryFn: () => getSiteLanguages(client),
  });

  function onSaved() {
    void queryClient.invalidateQueries({ queryKey: SETTINGS_QUERY_KEY });
  }

  if (isLoading) return <LoadingState />;
  if (error || !data) return <ErrorState description={error ? getApiErrorMessage(error) : 'Ayarlar yüklenemedi.'} />;

  const languages = (languagesData?.items ?? []).map((language) => ({ code: language.code, name: language.name }));

  const tabs: TabItem[] = [
    { key: 'identity', label: 'Kimlik', content: <IdentitySettingsForm settings={data} languages={languages} onSaved={onSaved} /> },
    { key: 'theme', label: 'Tema', content: <ThemeSettingsForm theme={data.theme} rowVersion={data.rowVersion} onSaved={onSaved} /> },
    { key: 'contact', label: 'İletişim', content: <ContactSettingsForm settings={data} onSaved={onSaved} /> },
    { key: 'features', label: 'Özellikler', content: <FeaturesSettingsForm settings={data} onSaved={onSaved} /> },
    { key: 'maintenance', label: 'Bakım', content: <MaintenanceSettingsForm settings={data} languages={languages} onSaved={onSaved} /> },
    { key: 'bot-protection', label: 'Bot koruması', content: <BotProtectionSettingsForm settings={data} onSaved={onSaved} /> },
    { key: 'bank-accounts', label: 'Banka hesapları', content: <BankAccountsSettingsForm settings={data} onSaved={onSaved} /> },
    { key: 'newsletter', label: 'Bülten', content: <NewsletterSettingsForm settings={data} onSaved={onSaved} /> },
    { key: 'events', label: 'Etkinlik', content: <EventsSettingsForm settings={data} onSaved={onSaved} /> },
    { key: 'cookie-consent', label: 'Çerez', content: <CookieConsentSettingsForm settings={data} languages={languages} onSaved={onSaved} /> },
    { key: 'submissions', label: 'Gönderim', content: <SubmissionsSettingsForm settings={data} onSaved={onSaved} /> },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title="Site ayarları" description="Her bölüm kendi kaydet düğmesine sahiptir." />
      <Tabs items={tabs} activeKey={activeTab} onChange={setActiveTab} />
    </div>
  );
}
