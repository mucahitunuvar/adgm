'use client';

import { useState } from 'react';
import { Play } from 'lucide-react';
import { safeUrl } from '@gm/ui';

interface VideoFeatureData {
  embedUrl: string;
  thumbnailUrl?: string | null;
  href: string | null;
}

/** Tıklayınca yükle (SECURITY.md §23.3): `youtube-nocookie` iframe'i yalnız kullanıcı oynat'a basınca eklenir. */
export function VideoFeatureBlock({
  texts,
  data,
}: {
  texts: { eyebrow?: string; title?: string; text?: string; link?: string; linkLabel?: string } | null;
  data: VideoFeatureData | null;
}) {
  const [loaded, setLoaded] = useState(false);
  if (!data) return null;
  const thumbnail = safeUrl(data.thumbnailUrl);
  const linkHref = safeUrl(data.href ?? texts?.link);

  return (
    <section className="container section block-video-feature">
      <div className="block-video-feature__grid">
        <div className="block-video-feature__player">
          {loaded ? (
            <iframe
              src={data.embedUrl}
              title={texts?.title ?? 'Video'}
              allow="accelerometer; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
            />
          ) : (
            <button type="button" className="block-video-feature__play" onClick={() => setLoaded(true)} aria-label={`${texts?.title ?? 'Video'} - oynat`}>
              {thumbnail && <img src={thumbnail} alt="" loading="lazy" />}
              <Play aria-hidden="true" />
            </button>
          )}
        </div>
        <div>
          {texts?.eyebrow && <p className="eyebrow">{texts.eyebrow}</p>}
          {texts?.title && <h2>{texts.title}</h2>}
          {texts?.text && <p>{texts.text}</p>}
          {linkHref && texts?.linkLabel && (
            <a className="button button--secondary" href={linkHref}>
              {texts.linkLabel}
            </a>
          )}
        </div>
      </div>
    </section>
  );
}
