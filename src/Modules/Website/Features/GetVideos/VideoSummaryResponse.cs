namespace GenclikMerkezi.Modules.Website.Features.GetVideos;

public sealed record VideoSummaryResponse(
    Guid Id,
    string YouTubeVideoId,
    string EmbedUrl,
    string YouTubeThumbnailUrl,
    VideoCoverImageResponse? CoverImage,
    string Title,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion);
