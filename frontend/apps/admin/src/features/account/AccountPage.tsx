import { useCurrentUser } from '@gm/auth';
import { PageHeader, Tabs, type TabItem } from '@gm/ui';
import { useState } from 'react';
import { ChangePasswordForm } from './ChangePasswordForm';
import { ProfileSummary } from './ProfileSummary';

export function AccountPage() {
  const user = useCurrentUser();
  const [activeTab, setActiveTab] = useState('profile');

  if (!user) return null;

  const items: TabItem[] = [
    { key: 'profile', label: 'Profil', content: <ProfileSummary user={user} /> },
    { key: 'password', label: 'Parola', content: <ChangePasswordForm /> },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Hesabım" title="Hesap bilgileri" />
      <Tabs items={items} activeKey={activeTab} onChange={setActiveTab} />
    </div>
  );
}
