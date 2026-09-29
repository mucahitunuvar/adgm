using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetVideoById;

public sealed class GetVideoByIdQueryHandler(
    IVideoRepository videoRepository, IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetVideoByIdQuery, Result<VideoDetailResponse>>
{
    public async Task<Result<VideoDetailResponse>> Handle(GetVideoByIdQuery request, CancellationToken cancellationToken)
    {
        var video = await videoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (video is null)
        {
            return Result.Failure<VideoDetailResponse>(Error.NotFound("Video.NotFound", $"Video '{request.Id}' could not be found."));
        }

        var coverImage = await BuildCoverImageAsync(video.CoverImageMediaId, cancellationToken);

        var translations = video.Translations
            .Select(t => new VideoTranslationResponse(t.LanguageCode.Value, t.Title, t.Description))
            .ToList();

        var response = new VideoDetailResponse(
            video.Id, video.YouTubeVideoId.Value, video.YouTubeVideoId.BuildEmbedUrl(), video.YouTubeVideoId.BuildThumbnailUrl(),
            video.CoverImageMediaId, coverImage, video.SortOrder, video.IsActive, video.RowVersion, translations, video.CreatedAtUtc);

        return Result.Success(response);
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
