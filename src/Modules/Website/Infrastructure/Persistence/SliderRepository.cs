using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class SliderRepository(WebsiteDbContext dbContext) : ISliderRepository
{
    public Task<Slider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Sliders.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Slider?> GetByKeyAsync(SliderKey key, CancellationToken cancellationToken = default) =>
        dbContext.Sliders.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

    public async Task<IReadOnlyList<Slider>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Sliders.AsNoTracking().OrderBy(s => s.Key).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Slider>> SearchByImageIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        await dbContext.Sliders
            .Where(s => s.Slides.Any(sl => sl.DesktopImageMediaId == mediaAssetId || sl.MobileImageMediaId == mediaAssetId))
            .ToListAsync(cancellationToken);

    // ADR-024 §17 (Faz 2 Görev 2): mirrors ContentItemRepository.GetEarliestUpcomingTransitionAsync's
    // own single-UNION-query shape. Only currently-active slides count - an inactive slide's schedule
    // can never make it visible (SlideVisibility requires IsActive regardless of the time window), so
    // its PublishAtUtc/UnpublishAtUtc are irrelevant to when the public response could next change.
    public Task<DateTime?> GetEarliestUpcomingSlideTransitionAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var activeSlides = dbContext.Sliders.AsNoTracking().SelectMany(s => s.Slides).Where(sl => sl.IsActive);

        var upcomingTransitions = activeSlides.Where(sl => sl.PublishAtUtc > now).Select(sl => sl.PublishAtUtc!.Value)
            .Union(activeSlides.Where(sl => sl.UnpublishAtUtc > now).Select(sl => sl.UnpublishAtUtc!.Value));

        return upcomingTransitions.OrderBy(t => t).Select(t => (DateTime?)t).FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Slider slider) => dbContext.Sliders.Add(slider);

    public void Remove(Slider slider) => dbContext.Sliders.Remove(slider);
}
