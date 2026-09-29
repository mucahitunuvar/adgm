namespace GenclikMerkezi.Modules.Website.Features.CreateVideo;

public sealed record CreateVideoRequest(
    string? YouTubeUrl, Guid? CoverImageMediaId, int SortOrder, string? DefaultLanguageTitle, string? DefaultLanguageDescription);
