using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeactivatePopup;

public sealed class DeactivatePopupCommandHandler(
    IPopupRepository popupRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivatePopupCommand, Result>
{
    public async Task<Result> Handle(DeactivatePopupCommand request, CancellationToken cancellationToken)
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

        var deactivateResult = popup.Deactivate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
