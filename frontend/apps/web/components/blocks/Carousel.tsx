'use client';

import { useCallback, useEffect, useState, type ReactNode } from 'react';
import useEmblaCarousel from 'embla-carousel-react';
import { ChevronLeft, ChevronRight, Pause, Play } from 'lucide-react';

/**
 * Hero slider ve başarı hikayeleri bloklarının paylaştığı Embla sarmalayıcısı (MIT, K-11 onaylı
 * bağımlılık). Ok/nokta/duraklat davranışı demo'nun `HomePage.tsx`'inden taşındı. Otomatik oynatma
 * yalnız `autoplayMs` verilmişse ve `prefers-reduced-motion` kapalıyken çalışır, odak/hover'da durur.
 */
export function Carousel({ slides, autoplayMs, ariaLabel }: { slides: ReactNode[]; autoplayMs?: number; ariaLabel: string }) {
  const [emblaRef, emblaApi] = useEmblaCarousel({ loop: slides.length > 1 });
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [isPaused, setIsPaused] = useState(false);

  const scrollPrev = useCallback(() => emblaApi?.scrollPrev(), [emblaApi]);
  const scrollNext = useCallback(() => emblaApi?.scrollNext(), [emblaApi]);
  const scrollTo = useCallback((index: number) => emblaApi?.scrollTo(index), [emblaApi]);

  useEffect(() => {
    if (!emblaApi) return;
    const onSelect = () => setSelectedIndex(emblaApi.selectedScrollSnap());
    emblaApi.on('select', onSelect);
    onSelect();
    return () => {
      emblaApi.off('select', onSelect);
    };
  }, [emblaApi]);

  useEffect(() => {
    if (!emblaApi || !autoplayMs || slides.length < 2 || isPaused) return;
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const interval = setInterval(() => emblaApi.scrollNext(), autoplayMs);
    return () => clearInterval(interval);
  }, [emblaApi, autoplayMs, isPaused, slides.length]);

  if (slides.length === 0) return null;

  return (
    <div className="carousel" onMouseEnter={() => setIsPaused(true)} onMouseLeave={() => setIsPaused(false)} onFocus={() => setIsPaused(true)} onBlur={() => setIsPaused(false)}>
      <div className="carousel__viewport" ref={emblaRef} role="region" aria-label={ariaLabel}>
        <div className="carousel__container">
          {slides.map((slide, index) => (
            <div className="carousel__slide" key={index}>
              {slide}
            </div>
          ))}
        </div>
      </div>
      {slides.length > 1 && (
        <div className="carousel__controls">
          <button type="button" className="icon-button" onClick={scrollPrev} aria-label="Önceki">
            <ChevronLeft aria-hidden="true" />
          </button>
          <div className="carousel__dots">
            {slides.map((_, index) => (
              <button
                key={index}
                type="button"
                className={`carousel__dot${index === selectedIndex ? ' carousel__dot--active' : ''}`}
                aria-label={`${index + 1}. slayta geç`}
                aria-current={index === selectedIndex}
                onClick={() => scrollTo(index)}
              />
            ))}
          </div>
          <button type="button" className="icon-button" onClick={scrollNext} aria-label="Sonraki">
            <ChevronRight aria-hidden="true" />
          </button>
          {autoplayMs && (
            <button type="button" className="icon-button" onClick={() => setIsPaused((v) => !v)} aria-label={isPaused ? 'Oynat' : 'Duraklat'}>
              {isPaused ? <Play aria-hidden="true" /> : <Pause aria-hidden="true" />}
            </button>
          )}
        </div>
      )}
    </div>
  );
}
