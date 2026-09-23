using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssets;

public sealed class GetMediaAssetsQueryHandler(IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetMediaAssetsQuery, Result<PagedResult<MediaAssetSummaryResponse>>>
{
    public async Task<Result<PagedResult<MediaAssetSummaryResponse>>> Handle(GetMediaAssetsQuery request, CancellationToken cancellationToken)
    {
        MediaAssetKind? kind = null;
        if (!string.IsNullOrWhiteSpace(request.Kind))
        {
            if (!Enum.TryParse<MediaAssetKind>(request.Kind, ignoreCase: true, out var parsedKind))
            {
                return Result.Failure<PagedResult<MediaAssetSummaryResponse>>(Error.Validation(
                    "MediaAsset.InvalidKindFilter", $"'{request.Kind}' is not a recognized media kind."));
            }

            kind = parsedKind;
        }

        var pagedMediaAssets = await mediaAssetRepository.SearchAsync(
            kind, request.Folder, request.Search, request.MissingAltText, request, cancellationToken);

        var items = new List<MediaAssetSummaryResponse>();
        foreach (var mediaAsset in pagedMediaAssets.Items)
        {
            items.Add(await ToSummaryAsync(mediaAsset, cancellationToken));
        }

        return Result.Success(new PagedResult<MediaAssetSummaryResponse>(
            items, pagedMediaAssets.TotalCount, pagedMediaAssets.Page, pagedMediaAssets.PageSize));
    }

    private async Task<MediaAssetSummaryResponse> ToSummaryAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var smallVariant = mediaAsset.Variants.FirstOrDefault(v => v.VariantName == MediaAssetVariantNames.Small);
        var thumbnailFileKey = smallVariant?.File.FileKey ?? mediaAsset.Original.FileKey;
        var thumbnailUrl = await fileStorageService.GetUrlAsync(thumbnailFileKey, cancellationToken);

        var hasAltText = mediaAsset.Translations.Any(t => t.AltText.Length > 0);

        return new MediaAssetSummaryResponse(
            mediaAsset.Id, mediaAsset.Kind.ToString(), mediaAsset.Original.OriginalFileName, thumbnailUrl,
            mediaAsset.Width, mediaAsset.Height, mediaAsset.Folder.Value, hasAltText, mediaAsset.CreatedAtUtc);
    }
}
