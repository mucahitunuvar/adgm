'use client';

import { useState } from 'react';
import { ProcessStepsList } from './ProcessStepsBlock';

interface Step {
  title: string;
  text: string;
}

interface Tab {
  label: string;
  steps: Step[];
}

/**
 * Karar (BLOK-ESLEMESI.md tablo #6, madde 2): aynı `PageLayout`'ta ARDIŞIK iki `process-steps`
 * bloğu = sekmeli görünüm; her bloğun `title`'ı sekme etiketi olur. Bu, admin blok düzeni
 * editöründe görünmeyen/belgelenmemiş bir frontend render kuralıdır (backend şema değişikliği yok).
 */
export function HowItWorksTabs({ tabs }: { tabs: Tab[] }) {
  const [activeIndex, setActiveIndex] = useState(0);
  const active = tabs[activeIndex];
  if (!active) return null;

  return (
    <section className="container section block-process-steps block-process-steps--tabs">
      <div className="tabs__list" role="tablist">
        {tabs.map((tab, index) => (
          <button
            key={tab.label}
            type="button"
            role="tab"
            aria-selected={index === activeIndex}
            className={`tabs__tab${index === activeIndex ? ' tabs__tab--active' : ''}`}
            onClick={() => setActiveIndex(index)}
          >
            {tab.label}
          </button>
        ))}
      </div>
      <div role="tabpanel">
        <ProcessStepsList steps={active.steps} />
      </div>
    </section>
  );
}
