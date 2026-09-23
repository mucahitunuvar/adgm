namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;

public sealed record MediaAssetDetailResponse(
    Guid Id,
    string Kind,
    string OriginalFileName,
    string OriginalUrl,
    long OriginalSizeInBytes,
    string ContentType,
    int? Width,
    int? Height,
    string Folder,
    string Source,
    string UsagePermissionNote,
    bool ContainsPersonalData,
    IReadOnlyList<MediaAssetVariantDetailResponse> Variants,
    IReadOnlyList<MediaAssetTranslationDetailResponse> Translations,
    IReadOnlyList<MediaUsageResponse> Usages,
    DateTime CreatedAtUtc);
