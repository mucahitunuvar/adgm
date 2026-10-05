using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IPopupRepository
{
    Task<Popup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin list (ADR-024 §9 Faz 2 Görev 6).
    Task<PagedResult<Popup>> SearchAsync(
        PopupDisplayMode? displayMode, bool? isActive, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // Public site response (ADR-024 §9/§15): currently visible (IsActive + publish window) popups with
    // a translation in languageCode, Priority descending - the frontend then picks the highest-priority
    // Modal and Banner whose targeting matches the current page.
    Task<IReadOnlyList<Popup>> SearchVisibleAsync(LanguageCode languageCode, DateTime now, CancellationToken cancellationToken = default);

    // PopupMediaUsageProvider's deletion guard: which popups currently use this media asset as their
    // (Modal-only) image.
    Task<IReadOnlyList<Popup>> SearchByImageMediaIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);

    // §6 "aynı anda en fazla 20 aktif ve süresi dolmamış pop-up" - active and not-yet-expired popups,
    // excluding excludeId (the popup being created/updated/activated, so it does not count against
    // itself). The caller adds 1 for the popup it is about to leave active+non-expired, if any.
    Task<int> CountActiveAndNotExpiredAsync(DateTime now, Guid? excludeId, CancellationToken cancellationToken = default);

    // Faz 2 Görev 6 master prompt §6: the public site cache's TTL-shortening query - the earliest
    // PublishAtUtc/UnpublishAtUtc among active popups that is still in the future, mirroring
    // ISliderRepository.GetEarliestUpcomingSlideTransitionAsync.
    Task<DateTime?> GetEarliestUpcomingTransitionAsync(DateTime now, CancellationToken cancellationToken = default);

    void Add(Popup popup);

    void Remove(Popup popup);
}
