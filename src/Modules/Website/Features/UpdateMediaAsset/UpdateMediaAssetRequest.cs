namespace GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;

public sealed record UpdateMediaAssetRequest(
    string? Folder,
    string? Source,
    string? UsagePermissionNote,
    bool ContainsPersonalData,
    IReadOnlyList<UpdateMediaAssetTranslationInput>? Translations);
