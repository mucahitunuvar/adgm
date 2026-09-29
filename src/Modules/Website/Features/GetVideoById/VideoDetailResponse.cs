namespace GenclikMerkezi.Modules.Website.Features.GetVideoById;

public sealed record VideoDetailResponse(
    Guid Id,
    string YouTubeVideoId,
    string EmbedUrl,
    string YouTubeThumbnailUrl,
    Guid? CoverImageMediaId,
    VideoCoverImageResponse? CoverImage,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion,
    IReadOnlyList<VideoTranslationResponse> Translations,
    DateTime CreatedAtUtc);
