using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePopup;

public sealed class ActivatePopupCommandHandler(
    IPopupRepository popupRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivatePopupCommand, Result>
{
    public async Task<Result> Handle(ActivatePopupCommand request, CancellationToken cancellationToken)
    {
        var popup = await popupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (popup is null)
        {
            return Result.Failure(Error.NotFound("Popup.NotFound", $"Popup '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(popup.RowVersion))
        {
            return Result.Failure(Error.Conflict("Popup.ConcurrencyConflict", "The popup was changed by someone else. Reload and try again."));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeLimitCheck = await PopupActiveLimitGuard.CheckAsync(
            popupRepository, wouldBeActive: true, popup.UnpublishAtUtc, now, popup.Id, cancellationToken);
        if (activeLimitCheck.IsFailure)
        {
            return activeLimitCheck;
        }

        var activateResult = popup.Activate(currentUserContext.UserId!.Value, now);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
