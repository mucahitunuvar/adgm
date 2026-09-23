namespace GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;

public sealed record UploadMediaAssetResponse(
    Guid Id,
    string Kind,
    string OriginalUrl,
    int? Width,
    int? Height,
    string Folder,
    IReadOnlyList<UploadMediaAssetVariantResponse> Variants);
