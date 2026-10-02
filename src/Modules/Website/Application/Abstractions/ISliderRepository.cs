using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface ISliderRepository
{
    Task<Slider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Slider?> GetByKeyAsync(SliderKey key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Slider>> GetAllAsync(CancellationToken cancellationToken = default);

    // SliderMediaUsageProvider's guard against deleting a MediaAsset some slide still references as its
    // desktop or mobile image.
    Task<IReadOnlyList<Slider>> SearchByImageIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);

    // §1.3/§2 "Cache": feeds GetPublicSiteQueryHandler's site-wide earliest-upcoming-transition
    // calculation alongside ContentItemRepository.GetEarliestUpcomingTransitionAsync - a scheduled
    // slide publish/unpublish must shorten the public site cache's TTL the same way a scheduled
    // content transition already does.
    Task<DateTime?> GetEarliestUpcomingSlideTransitionAsync(DateTime now, CancellationToken cancellationToken = default);

    void Add(Slider slider);

    void Remove(Slider slider);
}
