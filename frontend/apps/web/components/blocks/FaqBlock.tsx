import { SafeHtml } from '@gm/ui';

interface FaqItem {
  id: string;
  title: string;
  body: string;
}

export function FaqBlock({ texts, data }: { texts: { title?: string } | null; data: { items: FaqItem[] } | null }) {
  if (!data || data.items.length === 0) return null;

  return (
    <section className="container section block-faq">
      {texts?.title && <h2>{texts.title}</h2>}
      <div className="block-faq__list">
        {data.items.map((item) => (
          <details key={item.id} className="block-faq__item">
            <summary>{item.title}</summary>
            <SafeHtml html={item.body} />
          </details>
        ))}
      </div>
    </section>
  );
}
