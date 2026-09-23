namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssets;

public sealed record MediaAssetSummaryResponse(
    Guid Id,
    string Kind,
    string OriginalFileName,
    string ThumbnailUrl,
    int? Width,
    int? Height,
    string Folder,
    bool HasAltText,
    DateTime CreatedAtUtc);
