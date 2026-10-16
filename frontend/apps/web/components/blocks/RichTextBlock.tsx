import { SafeHtml } from '@gm/ui';

export function RichTextBlock({ texts }: { texts: { body?: string } | null }) {
  if (!texts?.body) return null;
  return (
    <section className="container section block-rich-text">
      <SafeHtml html={texts.body} className="block-rich-text__body" />
    </section>
  );
}
