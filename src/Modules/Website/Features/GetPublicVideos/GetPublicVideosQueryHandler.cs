using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicVideos;

// ADR-024 §5 (Faz 1b Görev 2): the videos page is the public listing of the video library - active
// videos only, in SortOrder, and only those with a translation in the resolved language (ADR-024 §3:
// a missing translation means the item is not listed in that language).
public sealed class GetPublicVideosQueryHandler(
    IVideoRepository videoRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<GetPublicVideosQuery, Result<PagedResult<PublicVideoResponse>>>
{
    public async Task<Result<PagedResult<PublicVideoResponse>>> Handle(GetPublicVideosQuery request, CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);

        // Invariant guaranteed by SiteLanguage's own domain rules: the default language can never be
        // deactivated, so there is always exactly one active default to fall back to.
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var paged = await videoRepository.SearchPublicAsync(resolvedLanguage.Code, request, cancellationToken);

        var items = new List<PublicVideoResponse>();
        foreach (var video in paged.Items)
        {
            items.Add(await ToPublicResponseAsync(video, resolvedLanguage.Code, cancellationToken));
        }

        return Result.Success(new PagedResult<PublicVideoResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }

    private async Task<PublicVideoResponse> ToPublicResponseAsync(Video video, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        // Guaranteed to exist: SearchPublicAsync only returns videos with a translation in languageCode.
        var translation = video.Translations.First(t => t.LanguageCode == languageCode);
        var coverImage = await BuildCoverImageAsync(video.CoverImageMediaId, cancellationToken);

        return new PublicVideoResponse(
            video.Id, video.YouTubeVideoId.Value, video.YouTubeVideoId.BuildEmbedUrl(), video.YouTubeVideoId.BuildThumbnailUrl(),
            coverImage, translation.Title, translation.Description, video.SortOrder);
    }

    private async Task<VideoCoverImageResponse?> BuildCoverImageAsync(Guid? coverImageMediaId, CancellationToken cancellationToken)
    {
        if (coverImageMediaId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(coverImageMediaId.Value, cancellationToken);
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

        return new VideoCoverImageResponse(small, medium, large, originalUrl);
    }
}
