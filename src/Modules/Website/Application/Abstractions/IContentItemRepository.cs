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

    // ADR-024 §15 (Faz 1a Görev 6): the single-row (LanguageCode, FullPath) lookup public route
    // resolution's Detail step uses - backed by the same unique index FullPathExistsAsync's
    // uniqueness rule relies on.
    Task<ContentItem?> GetByFullPathAsync(LanguageCode languageCode, string fullPath, CancellationToken cancellationToken = default);

    // Every ContentItem referencing mediaAssetId as its cover, detail or (in any language) OG image -
    // used by ContentItemMediaUsageProvider to block deleting a MediaAsset still in use.
    Task<IReadOnlyList<ContentItem>> GetByMediaAssetIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);

    // Direct children only (ADR-024 §4.3 Görev 4) - ContentPathCascadeService walks these to compute
    // hierarchy depth/cycles and to cascade a path change down a subtree.
    Task<IReadOnlyList<ContentItem>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<int> CountPublishedChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    // Every root-level (ParentId == null) item of this content type - used when a ContentType's
    // RoutePrefix changes and every item's FullPath in that language must be recomputed (Görev 4).
    Task<IReadOnlyList<ContentItem>> GetRootItemsByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default);

    // ADR-024 §4.1 (Faz 1b Görev 3): every item with at least one translation whose TagIds references
    // tagId - loaded as full aggregates so DeleteTag/MergeTagInto can update them through
    // ContentItem.SetTranslationTags (one aggregate root mutates and persists itself, not a raw
    // JSON-column rewrite).
    Task<IReadOnlyList<ContentItem>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);

    // ADR-024 §4.1 (Faz 1b Görev 3): DeleteContentCategoryCommandHandler's "içerik atanmışsa 409" guard.
    Task<int> CountByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    // ADR-024 §5 (Faz 1b Görev 4): VideoUsageChecker's real implementation - every ContentItem whose
    // VideoIds references videoId, used by DeleteVideoCommandHandler's "kullanımdaysa 409" guard.
    Task<IReadOnlyList<ContentItem>> GetByVideoIdAsync(Guid videoId, CancellationToken cancellationToken = default);

    void Add(ContentItem contentItem);
}
