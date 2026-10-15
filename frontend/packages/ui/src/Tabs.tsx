import type { KeyboardEvent, ReactNode } from 'react';

export interface TabItem {
  key: string;
  label: string;
  content: ReactNode;
}

export interface TabsProps {
  items: TabItem[];
  activeKey: string;
  onChange: (key: string) => void;
}

/** Standart ARIA `tablist`/`tab`/`tabpanel` deseni; sol/sağ ok tuşlarıyla gezinme. */
export function Tabs({ items, activeKey, onChange }: TabsProps) {
  function onKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    if (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft') return;
    const index = items.findIndex((item) => item.key === activeKey);
    if (index === -1) return;
    const nextIndex = event.key === 'ArrowRight'
      ? (index + 1) % items.length
      : (index - 1 + items.length) % items.length;
    const next = items[nextIndex];
    if (next) onChange(next.key);
  }

  return (
    <div className="tabs">
      <div className="tabs__list" role="tablist" onKeyDown={onKeyDown}>
        {items.map((item) => {
          const isActive = item.key === activeKey;
          return (
            <button
              key={item.key}
              type="button"
              role="tab"
              id={`tab-${item.key}`}
              aria-controls={`tabpanel-${item.key}`}
              aria-selected={isActive}
              tabIndex={isActive ? 0 : -1}
              className={`tabs__tab${isActive ? ' tabs__tab--active' : ''}`}
              onClick={() => onChange(item.key)}
            >
              {item.label}
            </button>
          );
        })}
      </div>
      {items.map((item) => (
        <div
          key={item.key}
          role="tabpanel"
          id={`tabpanel-${item.key}`}
          aria-labelledby={`tab-${item.key}`}
          hidden={item.key !== activeKey}
        >
          {item.key === activeKey ? item.content : null}
        </div>
      ))}
    </div>
  );
}
