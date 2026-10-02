using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// §4.3: Home has exactly one row (seeded by migration, like Menu's three rows - never created/deleted
// through an admin action), while a Content layout is created lazily, on its first draft save.
public interface IPageLayoutRepository
{
    Task<PageLayout?> GetHomeAsync(CancellationToken cancellationToken = default);

    Task<PageLayout?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    // PageLayoutReferenceScanner's deletion-guard sweep (SliderUsageChecker/VideoUsageChecker/
    // LayoutMediaUsageProvider) - the number of layouts is bounded by one Home row plus one per
    // SupportsBlockLayout content item, small enough to load in full rather than add a per-reference-
    // kind query.
    Task<IReadOnlyList<PageLayout>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(PageLayout pageLayout);

    void Remove(PageLayout pageLayout);
}
