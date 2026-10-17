import { PageHeader, Tabs, type TabItem } from '@gm/ui';
import { useState } from 'react';
import { NotFoundPathsTab } from './NotFoundPathsTab';
import { RedirectsListTab } from './RedirectsListTab';

export function RedirectsPage() {
  const [activeTab, setActiveTab] = useState('redirects');

  const tabs: TabItem[] = [
    { key: 'redirects', label: 'Yönlendirmeler', content: <RedirectsListTab /> },
    { key: 'not-found', label: '404 yolları', content: <NotFoundPathsTab /> },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="Yönlendirmeler ve 404 yolları"
        description="Kaynak yoldan hedefe 301/302 yönlendirmeler ve en çok bulunamayan yollar."
      />
      <Tabs items={tabs} activeKey={activeTab} onChange={setActiveTab} />
    </div>
  );
}
