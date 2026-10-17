import { safeUrl } from '@gm/ui';
import { BlockImage, type BlockImageVariants } from './BlockImage';
import { Carousel } from './Carousel';

interface Slide {
  id: string;
  eyebrow?: string | null;
  title: string;
  text?: string | null;
  desktop: BlockImageVariants;
  mobile?: BlockImageVariants | null;
  altText: string;
  buttonLabel?: string | null;
  buttonHref?: string | null;
}

export function HeroSliderBlock({ data }: { data: Slide[] | null }) {
  if (!data || data.length === 0) return null;

  const slides = data.map((slide, index) => {
    const href = safeUrl(slide.buttonHref);
    // Yalnız ilk slayt sayfa yüklenir yüklenmez görünür (katlama üstü); diğerleri Carousel'in
    // transform'uyla görünüme girer, geç yüklenmeleri sorun değildir.
    const isFirstSlide = index === 0;
    return (
      <div className="hero-slide" key={slide.id}>
        <BlockImage
          image={slide.mobile ?? slide.desktop}
          alt={slide.altText}
          className="hero-slide__image hero-slide__image--mobile"
          sizes="100vw"
          priority={isFirstSlide}
        />
        <BlockImage
          image={slide.desktop}
          alt={slide.altText}
          className="hero-slide__image hero-slide__image--desktop"
          sizes="100vw"
          priority={isFirstSlide}
        />
        <div className="hero-slide__content">
          {slide.eyebrow && <p className="eyebrow">{slide.eyebrow}</p>}
          <h2>{slide.title}</h2>
          {slide.text && <p className="page-lead">{slide.text}</p>}
          {href && slide.buttonLabel && (
            <a className="button button--primary" href={href}>
              {slide.buttonLabel}
            </a>
          )}
        </div>
      </div>
    );
  });

  return (
    <section className="block-hero-slider">
      <Carousel slides={slides} autoplayMs={6000} ariaLabel="Öne çıkan duyurular" />
    </section>
  );
}
