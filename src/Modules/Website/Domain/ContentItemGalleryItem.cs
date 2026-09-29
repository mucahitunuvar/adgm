using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1b Görev 4): one gallery image, with an optional per-language alt text/caption
// override. Whole-list replace only (ADR-024 Faz 1b "Koleksiyon güncelleme şekli") - the caller
// (Application layer) has already verified MediaAssetId exists and is Kind == Image (MediaImageReferenceGuard)
// before constructing this; ContentItem.SetGallery enforces the max-count and no-duplicate-media
// invariants across the whole list.
public sealed class ContentItemGalleryItem : Entity
{
    private readonly List<ContentItemGalleryItemTranslation> _translations = [];

    public Guid MediaAssetId { get; private set; }

    public int SortOrder { get; private set; }

    public IReadOnlyList<ContentItemGalleryItemTranslation> Translations => _translations.AsReadOnly();

    private ContentItemGalleryItem(Guid id, Guid mediaAssetId, int sortOrder)
        : base(id)
    {
        MediaAssetId = mediaAssetId;
        SortOrder = sortOrder;
    }

    private ContentItemGalleryItem()
    {
    }

    public static ContentItemGalleryItem Create(Guid mediaAssetId, int sortOrder, IReadOnlyList<ContentItemGalleryItemTranslation> translations)
    {
        var item = new ContentItemGalleryItem(Guid.NewGuid(), mediaAssetId, sortOrder);
        item._translations.AddRange(translations);

        return item;
    }
}
