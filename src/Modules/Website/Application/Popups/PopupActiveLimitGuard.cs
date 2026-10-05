using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Popups;

// Faz 2 Görev 6 master prompt §6 "aynı anda en fazla 20 aktif ve süresi dolmamış pop-up" - shared by
// CreatePopup/UpdatePopup/ActivatePopup's command handlers (the three transitions that can leave a
// popup active and not-yet-expired), the same "small static Application guard used by multiple
// handlers" shape MediaImageReferenceGuard already uses.
public static class PopupActiveLimitGuard
{
    private const int MaxActiveAndNotExpired = 20;

    public static async Task<Result> CheckAsync(
        IPopupRepository popupRepository, bool wouldBeActive, DateTime? unpublishAtUtc, DateTime now, Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var wouldBeActiveAndNotExpired = wouldBeActive && (unpublishAtUtc is null || unpublishAtUtc > now);
        if (!wouldBeActiveAndNotExpired)
        {
            return Result.Success();
        }

        var activeCount = await popupRepository.CountActiveAndNotExpiredAsync(now, excludeId, cancellationToken);
        return activeCount >= MaxActiveAndNotExpired
            ? Result.Failure(Error.Conflict(
                "Popup.TooManyActivePopups", $"At most {MaxActiveAndNotExpired} popups can be active and not expired at the same time."))
            : Result.Success();
    }
}
