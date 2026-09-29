namespace GenclikMerkezi.Modules.Website.Features.GetPublicVideos;

public sealed record PublicVideoResponse(
    Guid Id,
    string YouTubeVideoId,
    string EmbedUrl,
    string YouTubeThumbnailUrl,
    VideoCoverImageResponse? CoverImage,
    string Title,
    string? Description,
    int SortOrder);
