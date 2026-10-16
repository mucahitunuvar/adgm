'use client';

import { useState } from 'react';
import { Play } from 'lucide-react';
import { safeUrl } from '@gm/ui';
import type { PublicContentDetailResponse } from '@gm/api-client';

/** Tıklayınca yükle (SECURITY.md §23.3): `youtube-nocookie` iframe'i yalnız oynat'a basınca eklenir. */
function VideoListItem({ video }: { video: PublicContentDetailResponse['videos'][number] }) {
  const [loaded, setLoaded] = useState(false);
  const thumbnail = safeUrl(video.youTubeThumbnailUrl);

  return (
    <li className="content-detail__video">
      {loaded ? (
        <iframe
          src={video.embedUrl}
          title={video.title}
          allow="accelerometer; encrypted-media; gyroscope; picture-in-picture"
          allowFullScreen
        />
      ) : (
        <button type="button" className="block-video-feature__play" onClick={() => setLoaded(true)} aria-label={`${video.title} - oynat`}>
          {thumbnail && <img src={thumbnail} alt="" loading="lazy" />}
          <Play aria-hidden="true" />
        </button>
      )}
      <p>{video.title}</p>
    </li>
  );
}

export function VideoListSection({ videos }: { videos: PublicContentDetailResponse['videos'] }) {
  if (videos.length === 0) return null;
  return (
    <section className="content-detail__section">
      <h2>Videolar</h2>
      <ul className="content-detail__video-list">
        {videos.map((video) => (
          <VideoListItem key={video.videoId} video={video} />
        ))}
      </ul>
    </section>
  );
}
