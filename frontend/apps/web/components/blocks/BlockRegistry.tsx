import type { PublicLayoutBlockResponse } from '@gm/api-client';
import { CtaBlock } from './CtaBlock';
import { FaqBlock } from './FaqBlock';
import { GalleryBlock } from './GalleryBlock';
import { HeroSliderBlock } from './HeroSliderBlock';
import { HowItWorksTabs } from './HowItWorksTabs';
import { ImageTextBlock } from './ImageTextBlock';
import { ImpactStatsBlock } from './ImpactStatsBlock';
import { JobListBlock } from './JobListBlock';
import { LogoStripBlock } from './LogoStripBlock';
import { ContentListBlock } from './ContentListBlock';
import { ProcessStepsBlock } from './ProcessStepsBlock';
import { QuickLinksBlock } from './QuickLinksBlock';
import { RichTextBlock } from './RichTextBlock';
import { UpcomingEventsBlock } from './UpcomingEventsBlock';
import { VideoFeatureBlock } from './VideoFeatureBlock';

/**
 * Karar (madde 5): registry 15 blok tipinin TAMAMINI render eder, 11 satırlık akışla sınırlı değil.
 * Bilinmeyen tip güvenle atlanır (ADR-024 §8.1). Ardışık iki `process-steps` = sekmeli görünüm
 * (karar madde 2) - bu TEK istisna, burada "önden bakma" ile ele alınır; başka hiçbir blok
 * ikilisi birleştirilmez. Blok şekli `Home` ve `Content` hedefleri arasında aynıdır (ADR-024
 * §8.1); bu yüzden aynı registry Faz 3 Koşu 3'te içerik detay sayfasının `blocks` alanı için de
 * (`SupportsBlockLayout` türler) kullanılır - isim artık yalnız ana sayfaya özgü değildir.
 */
export function renderLayoutBlocks(
  blocks: readonly PublicLayoutBlockResponse[],
  { publicJobListingsEnabled }: { publicJobListingsEnabled: boolean },
): React.ReactNode[] {
  const nodes: React.ReactNode[] = [];

  for (let index = 0; index < blocks.length; index++) {
    const block = blocks[index]!;
    const settings = block.settings as Record<string, unknown> | null;
    const texts = block.texts as Record<string, unknown> | null;

    if (block.type === 'job-list') {
      if (publicJobListingsEnabled) {
        nodes.push(<JobListBlock key={index} texts={texts as never} settings={settings as never} />);
      }
      continue;
    }

    if (block.type === 'process-steps') {
      const next = blocks[index + 1];
      if (next?.type === 'process-steps') {
        const firstTexts = texts as { title?: string; steps?: { title: string; text: string }[] } | null;
        const nextTexts = next.texts as { title?: string; steps?: { title: string; text: string }[] } | null;
        if (firstTexts?.steps && nextTexts?.steps) {
          nodes.push(
            <HowItWorksTabs
              key={index}
              tabs={[
                { label: firstTexts.title ?? '', steps: firstTexts.steps },
                { label: nextTexts.title ?? '', steps: nextTexts.steps },
              ]}
            />,
          );
          index++;
          continue;
        }
      }
      nodes.push(<ProcessStepsBlock key={index} texts={texts as never} />);
      continue;
    }

    switch (block.type) {
      case 'hero-slider':
        nodes.push(<HeroSliderBlock key={index} data={block.data as never} />);
        break;
      case 'image-text':
        nodes.push(<ImageTextBlock key={index} settings={settings as never} texts={texts as never} data={block.data as never} />);
        break;
      case 'impact-stats':
        nodes.push(<ImpactStatsBlock key={index} texts={texts as never} data={block.data as never} />);
        break;
      case 'video-feature':
        nodes.push(<VideoFeatureBlock key={index} texts={texts as never} data={block.data as never} />);
        break;
      case 'upcoming-events':
        nodes.push(<UpcomingEventsBlock key={index} texts={texts as never} data={block.data as never} />);
        break;
      case 'content-list':
        nodes.push(<ContentListBlock key={index} settings={settings as never} texts={texts as never} data={block.data as never} />);
        break;
      case 'logo-strip':
        nodes.push(<LogoStripBlock key={index} texts={texts as never} data={block.data as never} />);
        break;
      case 'quick-links':
        nodes.push(<QuickLinksBlock key={index} data={block.data as never} />);
        break;
      case 'gallery':
        nodes.push(<GalleryBlock key={index} texts={texts as never} data={block.data as never} />);
        break;
      case 'faq':
        nodes.push(<FaqBlock key={index} texts={texts as never} data={block.data as never} />);
        break;
      case 'cta':
        nodes.push(<CtaBlock key={index} data={block.data as never} />);
        break;
      case 'rich-text':
        nodes.push(<RichTextBlock key={index} texts={texts as never} />);
        break;
      default:
        // Tanınmayan blok tipi (ileride eklenecek) - güvenle atlanır, sayfa çökmez.
        break;
    }
  }

  return nodes;
}
