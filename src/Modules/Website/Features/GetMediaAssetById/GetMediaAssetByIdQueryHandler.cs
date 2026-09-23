using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;

public sealed class GetMediaAssetByIdQueryHandler(
    IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService, IMediaUsageChecker mediaUsageChecker)
    : IRequestHandler<GetMediaAssetByIdQuery, Result<MediaAssetDetailResponse>>
{
    public async Task<Result<MediaAssetDetailResponse>> Handle(GetMediaAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var mediaAsset = await mediaAssetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (mediaAsset is null)
        {
            return Result.Failure<MediaAssetDetailResponse>(Error.NotFound(
                "MediaAsset.NotFound", "The specified media asset could not be found."));
        }

        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);

        var variants = new List<MediaAssetVariantDetailResponse>();
        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            variants.Add(new MediaAssetVariantDetailResponse(variant.VariantName, url, variant.Width, variant.Height));
        }

        var translations = mediaAsset.Translations
            .Select(t => new MediaAssetTranslationDetailResponse(t.LanguageCode.Value, t.AltText, t.Caption))
            .ToList();

        var usages = await mediaUsageChecker.GetUsagesAsync(mediaAsset.Id, cancellationToken);
        var usageResponses = usages.Select(u => new MediaUsageResponse(u.SourceKey, u.SourceId, u.Description, u.Url)).ToList();

        return Result.Success(new MediaAssetDetailResponse(
            mediaAsset.Id, mediaAsset.Kind.ToString(), mediaAsset.Original.OriginalFileName, originalUrl,
            mediaAsset.Original.SizeInBytes, mediaAsset.Original.ContentType, mediaAsset.Width, mediaAsset.Height,
            mediaAsset.Folder.Value, mediaAsset.Source, mediaAsset.UsagePermissionNote, mediaAsset.ContainsPersonalData,
            variants, translations, usageResponses, mediaAsset.CreatedAtUtc));
    }
}
