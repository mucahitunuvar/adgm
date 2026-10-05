using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopup;

public sealed class UpdatePopupCommandHandler(
    IPopupRepository popupRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePopupCommand, Result>
{
    public async Task<Result> Handle(UpdatePopupCommand request, CancellationToken cancellationToken)
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

        if (!Enum.TryParse<PopupDisplayMode>(request.DisplayMode, ignoreCase: true, out var displayMode))
        {
            return Result.Failure(Error.Validation("Popup.DisplayModeInvalid", $"Unknown display mode '{request.DisplayMode}'."));
        }

        if (!Enum.TryParse<PopupDeviceTarget>(request.DeviceTarget, ignoreCase: true, out var deviceTarget))
        {
            return Result.Failure(Error.Validation("Popup.DeviceTargetInvalid", $"Unknown device target '{request.DeviceTarget}'."));
        }

        if (!Enum.TryParse<PopupFrequency>(request.Frequency, ignoreCase: true, out var frequency))
        {
            return Result.Failure(Error.Validation("Popup.FrequencyInvalid", $"Unknown frequency '{request.Frequency}'."));
        }

        var linkResult = LinkTargetDtoMapper.ToLinkTarget(request.Link);
        if (linkResult.IsFailure)
        {
            return linkResult;
        }

        var targetingResult = PopupTargetingInputMapper.ToTargeting(request.Targeting);
        if (targetingResult.IsFailure)
        {
            return targetingResult;
        }

        var targetingValidation = await PopupTargetingReferenceValidator.ValidateAsync(
            targetingResult.Value, siteLanguageRepository, contentItemRepository, cancellationToken);
        if (targetingValidation.IsFailure)
        {
            return targetingValidation;
        }

        if (request.ImageMediaId is { } imageMediaId)
        {
            var imageCheck = await MediaImageReferenceGuard.CheckAsync(imageMediaId, "Popup", "Image", mediaAssetRepository, cancellationToken);
            if (imageCheck.IsFailure)
            {
                return imageCheck;
            }
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeLimitCheck = await PopupActiveLimitGuard.CheckAsync(
            popupRepository, popup.IsActive, request.UnpublishAtUtc, now, popup.Id, cancellationToken);
        if (activeLimitCheck.IsFailure)
        {
            return activeLimitCheck;
        }

        var updateResult = popup.Update(
            displayMode, request.ImageMediaId, linkResult.Value, targetingResult.Value, deviceTarget, request.PublishAtUtc,
            request.UnpublishAtUtc, request.DelaySeconds, frequency, request.FrequencyDays, request.Dismissible, request.Priority,
            currentUserContext.UserId!.Value, now);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
