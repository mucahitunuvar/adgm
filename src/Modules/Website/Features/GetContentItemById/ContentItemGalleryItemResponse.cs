namespace GenclikMerkezi.Modules.Website.Features.GetContentItemById;

public sealed record ContentItemGalleryItemResponse(
    Guid MediaAssetId, int SortOrder, IReadOnlyList<ContentItemGalleryItemTranslationResponse> Translations);
