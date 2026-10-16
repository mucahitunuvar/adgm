import { iconKeyToLucide } from './iconKeyToLucide';

interface ImpactMetric {
  id: string;
  value: number;
  unit?: string | null;
  label: string;
  period?: string | null;
  source?: string | null;
  iconKey?: string | null;
}

export function ImpactStatsBlock({ texts, data }: { texts: { title?: string } | null; data: ImpactMetric[] | null }) {
  if (!data || data.length === 0) return null;

  return (
    <section className="container section block-impact-stats">
      {texts?.title && <h2>{texts.title}</h2>}
      <dl className="block-impact-stats__grid">
        {data.map((metric) => {
          const Icon = iconKeyToLucide(metric.iconKey);
          return (
            <div key={metric.id} className="block-impact-stats__item">
              <Icon aria-hidden="true" />
              <dd>
                {metric.value.toLocaleString('tr-TR')}
                {metric.unit ? ` ${metric.unit}` : ''}
              </dd>
              <dt>{metric.label}</dt>
            </div>
          );
        })}
      </dl>
    </section>
  );
}
