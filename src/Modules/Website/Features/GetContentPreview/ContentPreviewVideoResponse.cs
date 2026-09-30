namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

public sealed record ContentPreviewVideoResponse(
    Guid VideoId, string YouTubeVideoId, string EmbedUrl, string YouTubeThumbnailUrl, string Title, string? Description);
