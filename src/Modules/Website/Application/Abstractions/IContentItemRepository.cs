using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IContentItemRepository
{
    Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // languageCode both filters (when requireLanguage is true, only items that have a translation in
    // that language are returned) and picks which language's title is projected. When requireLanguage
    // is false, languageCode is the resolved default site language - every item structurally has a
    // translation in it (ContentItem.Create/RemoveTranslation guarantee this), so no filter is needed.
    Task<PagedResult<ContentItemListItem>> SearchAsync(
        Guid? contentTypeId,
        ContentItemStatus? status,
        LanguageCode languageCode,
        bool requireLanguage,
        string? search,
        bool? isFeatured,
        Guid? parentId,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default);

    // Cross-aggregate uniqueness check (ADR-024 §4.3): FullPath is unique per language across the
    // whole module, not just within a content type.
    Task<bool> FullPathExistsAsync(LanguageCode languageCode, string fullPath, Guid? excludeId, CancellationToken cancellationToken = default);

    // Every ContentItem referencing mediaAssetId as its cover, detail or (in any language) OG image -
    // used by ContentItemMediaUsageProvider to block deleting a MediaAsset still in use.
    Task<IReadOnlyList<ContentItem>> GetByMediaAssetIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);

    void Add(ContentItem contentItem);
}
