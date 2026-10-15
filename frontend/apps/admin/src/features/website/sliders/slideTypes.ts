import type { SlideInput, SlideResponse } from '@gm/api-client';
import type { LinkTargetValue } from '../shared/LinkTargetField';

/**
 * `ReplaceSlides`'ın `SlideInput`'u hiçbir kimlik taşımaz (tüm liste sırayla değiştirilir) - `tempKey`
 * yalnız React listesi/`ReorderList` için istemci tarafında tutulan bir anahtar, backend'e gitmez.
 */
export interface DraftSlide {
  tempKey: string;
  desktopImageMediaId: string | null;
  mobileImageMediaId: string | null;
  link: LinkTargetValue | null;
  isActive: boolean;
  publishAtUtc: string | null;
  unpublishAtUtc: string | null;
  translations: Array<{ languageCode: string; eyebrow: string | null; title: string; text: string | null; buttonLabel: string | null; altTextOverride: string | null }>;
}

export function draftSlidesFromResponse(slides: readonly SlideResponse[]): DraftSlide[] {
  return slides.map((slide) => ({
    tempKey: slide.id,
    desktopImageMediaId: slide.desktopImageMediaId,
    mobileImageMediaId: slide.mobileImageMediaId,
    link: slide.link,
    isActive: slide.isActive,
    publishAtUtc: slide.publishAtUtc,
    unpublishAtUtc: slide.unpublishAtUtc,
    translations: slide.translations.map((t) => ({
      languageCode: t.languageCode,
      eyebrow: t.eyebrow,
      title: t.title,
      text: t.text,
      buttonLabel: t.buttonLabel,
      altTextOverride: t.altTextOverride,
    })),
  }));
}

export function draftSlidesToInput(slides: readonly DraftSlide[]): SlideInput[] {
  return slides.map((slide, index) => ({
    desktopImageMediaId: slide.desktopImageMediaId ?? '',
    mobileImageMediaId: slide.mobileImageMediaId,
    link: slide.link,
    sortOrder: index,
    isActive: slide.isActive,
    publishAtUtc: slide.publishAtUtc,
    unpublishAtUtc: slide.unpublishAtUtc,
    translations: slide.translations.map((t) => ({
      languageCode: t.languageCode,
      eyebrow: t.eyebrow,
      title: t.title,
      text: t.text,
      buttonLabel: t.buttonLabel,
      altTextOverride: t.altTextOverride,
    })),
  }));
}

export function newDraftSlide(): DraftSlide {
  return {
    tempKey: crypto.randomUUID(),
    desktopImageMediaId: null,
    mobileImageMediaId: null,
    link: null,
    isActive: true,
    publishAtUtc: null,
    unpublishAtUtc: null,
    translations: [],
  };
}
