using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetVideos;

public sealed class GetVideosQueryHandler(
    IVideoRepository videoRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<GetVideosQuery, Result<PagedResult<VideoSummaryResponse>>>
{
    public async Task<Result<PagedResult<VideoSummaryResponse>>> Handle(GetVideosQuery request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<PagedResult<VideoSummaryResponse>>(
                Error.Failure("Video.NoDefaultLanguage", "No default site language is configured."));
        }

        var paged = await videoRepository.SearchAsync(request.IsActive, request.Search, defaultLanguage.Code, request, cancellationToken);

        var items = new List<VideoSummaryResponse>();
        foreach (var video in paged.Items)
        {
            items.Add(await ToSummaryAsync(video, defaultLanguage.Code, cancellationToken));
        }

        return Result.Success(new PagedResult<VideoSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }

    private async Task<VideoSummaryResponse> ToSummaryAsync(Video video, LanguageCode defaultLanguageCode, CancellationToken cancellationToken)
    {
        var title = video.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode)?.Title ?? string.Empty;
        var coverImage = await BuildCoverImageAsync(video.CoverImageMediaId, cancellationToken);

        return new VideoSummaryResponse(
            video.Id, video.YouTubeVideoId.Value, video.YouTubeVideoId.BuildEmbedUrl(), video.YouTubeVideoId.BuildThumbnailUrl(),
            coverImage, title, video.SortOrder, video.IsActive, video.RowVersion);
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
