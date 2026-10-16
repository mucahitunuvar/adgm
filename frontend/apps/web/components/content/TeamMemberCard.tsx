import { BlockImage } from '../blocks/BlockImage';
import type { PublicContentListItemResponse } from '@gm/api-client';

/** `team-grid` şablonu: yalnız kart (foto, ad, unvan), detay yok (`HasDetailPage=false`, karar özeti §4). */
export function TeamMemberCard({ item }: { item: PublicContentListItemResponse }) {
  const image = item.coverImage ?? item.detailImage;
  return (
    <div className="team-card">
      {image ? (
        <BlockImage image={image} alt={item.title} sizes="(min-width: 900px) 25vw, 50vw" className="team-card__photo" />
      ) : (
        <div className="team-card__photo team-card__photo--placeholder" aria-hidden="true" />
      )}
      <h3>{item.title}</h3>
      {item.summary && <p className="team-card__role">{item.summary}</p>}
    </div>
  );
}
