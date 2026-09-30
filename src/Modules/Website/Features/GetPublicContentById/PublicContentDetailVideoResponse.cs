namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

public sealed record PublicContentDetailVideoResponse(
    Guid VideoId, string YouTubeVideoId, string EmbedUrl, string YouTubeThumbnailUrl, string Title, string? Description);
