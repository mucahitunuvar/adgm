namespace GenclikMerkezi.Modules.Website.Features.UpdateVideo;

public sealed record UpdateVideoRequest(byte[] RowVersion, string? YouTubeUrl, Guid? CoverImageMediaId, int SortOrder);
