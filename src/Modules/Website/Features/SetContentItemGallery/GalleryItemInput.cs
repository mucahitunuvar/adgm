namespace GenclikMerkezi.Modules.Website.Features.SetContentItemGallery;

public sealed record GalleryItemInput(Guid MediaAssetId, int SortOrder, IReadOnlyList<GalleryItemTranslationInput> Translations);
