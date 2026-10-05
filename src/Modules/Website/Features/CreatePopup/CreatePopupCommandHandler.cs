using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreatePopup;

public sealed class CreatePopupCommandHandler(
    IPopupRepository popupRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IContentItemRepository contentItemRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePopupCommand, Result<CreatePopupResponse>>
{
    public async Task<Result<CreatePopupResponse>> Handle(CreatePopupCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PopupDisplayMode>(request.DisplayMode, ignoreCase: true, out var displayMode))
        {
            return Result.Failure<CreatePopupResponse>(Error.Validation("Popup.DisplayModeInvalid", $"Unknown display mode '{request.DisplayMode}'."));
        }

        if (!Enum.TryParse<PopupDeviceTarget>(request.DeviceTarget, ignoreCase: true, out var deviceTarget))
        {
            return Result.Failure<CreatePopupResponse>(Error.Validation("Popup.DeviceTargetInvalid", $"Unknown device target '{request.DeviceTarget}'."));
        }

        if (!Enum.TryParse<PopupFrequency>(request.Frequency, ignoreCase: true, out var frequency))
        {
            return Result.Failure<CreatePopupResponse>(Error.Validation("Popup.FrequencyInvalid", $"Unknown frequency '{request.Frequency}'."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreatePopupResponse>(Error.Failure("Popup.NoDefaultLanguage", "No default site language is configured."));
        }

        var linkResult = LinkTargetDtoMapper.ToLinkTarget(request.Link);
        if (linkResult.IsFailure)
        {
            return Result.Failure<CreatePopupResponse>(linkResult.Error);
        }

        var targetingResult = PopupTargetingInputMapper.ToTargeting(request.Targeting);
        if (targetingResult.IsFailure)
        {
            return Result.Failure<CreatePopupResponse>(targetingResult.Error);
        }

        var targetingValidation = await PopupTargetingReferenceValidator.ValidateAsync(
            targetingResult.Value, siteLanguageRepository, contentItemRepository, cancellationToken);
        if (targetingValidation.IsFailure)
        {
            return Result.Failure<CreatePopupResponse>(targetingValidation.Error);
        }

        if (request.ImageMediaId is { } imageMediaId)
        {
            var imageCheck = await MediaImageReferenceGuard.CheckAsync(imageMediaId, "Popup", "Image", mediaAssetRepository, cancellationToken);
            if (imageCheck.IsFailure)
            {
                return Result.Failure<CreatePopupResponse>(imageCheck.Error);
            }
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var activeLimitCheck = await PopupActiveLimitGuard.CheckAsync(
            popupRepository, wouldBeActive: true, request.UnpublishAtUtc, now, excludeId: null, cancellationToken);
        if (activeLimitCheck.IsFailure)
        {
            return Result.Failure<CreatePopupResponse>(activeLimitCheck.Error);
        }

        var body = SanitizeBody(displayMode, request.DefaultLanguageBody);

        var popupResult = Popup.Create(
            displayMode, request.ImageMediaId, linkResult.Value, targetingResult.Value, deviceTarget, request.PublishAtUtc,
            request.UnpublishAtUtc, request.DelaySeconds, frequency, request.FrequencyDays, request.Dismissible, request.Priority,
            defaultLanguage.Code, request.DefaultLanguageTitle, body, request.DefaultLanguageButtonLabel,
            currentUserContext.UserId!.Value, now);
        if (popupResult.IsFailure)
        {
            return Result.Failure<CreatePopupResponse>(popupResult.Error);
        }

        popupRepository.Add(popupResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new CreatePopupResponse(popupResult.Value.Id, defaultLanguage.Code.Value));
    }

    private string? SanitizeBody(PopupDisplayMode displayMode, string? body) =>
        displayMode == PopupDisplayMode.Modal ? htmlContentSanitizer.Sanitize(body ?? string.Empty) : body;
}
