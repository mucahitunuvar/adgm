using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPopupById;

public sealed class GetPopupByIdQueryHandler(
    IPopupRepository popupRepository, IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetPopupByIdQuery, Result<PopupDetailResponse>>
{
    public async Task<Result<PopupDetailResponse>> Handle(GetPopupByIdQuery request, CancellationToken cancellationToken)
    {
        var popup = await popupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (popup is null)
        {
            return Result.Failure<PopupDetailResponse>(Error.NotFound("Popup.NotFound", $"Popup '{request.Id}' could not be found."));
        }

        var image = popup.ImageMediaId is { } imageMediaId ? await BuildImageAsync(imageMediaId, cancellationToken) : null;

        PopupLinkResponse? link = popup.LinkTarget.IsEmpty
            ? null
            : new PopupLinkResponse(
                popup.LinkTarget.Kind.ToString(), popup.LinkTarget.ContentItemId, popup.LinkTarget.ContentTypeId,
                popup.LinkTarget.InternalPath, popup.LinkTarget.ExternalUrl);

        var targeting = new PopupTargetingResponse(popup.Targeting.Kind.ToString(), popup.Targeting.ContentItemIds, popup.Targeting.Paths);

        var translations = popup.Translations
            .Select(t => new PopupTranslationResponse(t.LanguageCode.Value, t.Title, t.Body, t.ButtonLabel))
            .ToList();

        var response = new PopupDetailResponse(
            popup.Id, popup.DisplayMode.ToString(), popup.ImageMediaId, image, link, targeting, popup.DeviceTarget.ToString(),
            popup.PublishAtUtc, popup.UnpublishAtUtc, popup.IsActive, popup.DelaySeconds, popup.Frequency.ToString(), popup.FrequencyDays,
            popup.Dismissible, popup.Priority, popup.RowVersion, translations, popup.CreatedAtUtc);

        return Result.Success(response);
    }

    private async Task<PopupImageResponse?> BuildImageAsync(Guid imageMediaId, CancellationToken cancellationToken)
    {
        var mediaAsset = await mediaAssetRepository.GetByIdAsync(imageMediaId, cancellationToken);
        if (mediaAsset is null)
        {
            return null;
        }

        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
        string? small = null;
        string? medium = null;
        string? large = null;

        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            if (variant.VariantName == MediaAssetVariantNames.Small)
            {
                small = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Medium)
            {
                medium = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Large)
            {
                large = url;
            }
        }

        return new PopupImageResponse(small, medium, large, originalUrl);
    }
}
