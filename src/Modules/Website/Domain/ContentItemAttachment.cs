using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1b Görev 4): one downloadable file, with an optional per-language display name
// override. Whole-list replace only - the caller (Application layer) has already verified
// MediaAssetId exists and is Kind == Document before constructing this; ContentItem.SetAttachments
// enforces the max-count and no-duplicate-media invariants across the whole list.
public sealed class ContentItemAttachment : Entity
{
    private readonly List<ContentItemAttachmentTranslation> _translations = [];

    public Guid MediaAssetId { get; private set; }

    public int SortOrder { get; private set; }

    public IReadOnlyList<ContentItemAttachmentTranslation> Translations => _translations.AsReadOnly();

    private ContentItemAttachment(Guid id, Guid mediaAssetId, int sortOrder)
        : base(id)
    {
        MediaAssetId = mediaAssetId;
        SortOrder = sortOrder;
    }

    private ContentItemAttachment()
    {
    }

    public static ContentItemAttachment Create(Guid mediaAssetId, int sortOrder, IReadOnlyList<ContentItemAttachmentTranslation> translations)
    {
        var attachment = new ContentItemAttachment(Guid.NewGuid(), mediaAssetId, sortOrder);
        attachment._translations.AddRange(translations);

        return attachment;
    }
}
