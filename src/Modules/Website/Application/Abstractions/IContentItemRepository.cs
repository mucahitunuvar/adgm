using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IContentItemRepository
{
    Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Faz 2 Görev 1: LinkTargetResolver's bulk lookup - every requested id (and, across further calls,
    // every ancestor id still missing) fetched in one query, so resolving every Content link target in
    // a whole menu costs O(hierarchy depth) round trips, not O(menu item count) (§1.1 "N+1 yok").
    Task<IReadOnlyList<ContentItem>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

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

    // ADR-024 §12.2 (Faz 3 Görev 3): FormDefinitionUsageChecker's real implementation - every
    // ContentItem linked to formDefinitionId, used by DeleteFormDefinitionCommandHandler's "kullanımdaysa
    // silinemez" guard.
    Task<IReadOnlyList<ContentItem>> GetByFormDefinitionIdAsync(Guid formDefinitionId, CancellationToken cancellationToken = default);

    // ADR-024 §4.1 (Faz 1b Görev 5): RelatedContentResolutionService's manual step - projects the
    // requested language's display fields for visible, translated items among `ids`, in no
    // particular order (the caller re-sorts to `ids`' own order, since RelatedContentItemIds' order
    // is the manually curated display order).
    Task<IReadOnlyList<RelatedContentCandidate>> GetVisibleRelatedCandidatesByIdsAsync(
        IReadOnlyList<Guid> ids, LanguageCode languageCode, DateTime now, CancellationToken cancellationToken = default);

    // ADR-024 §4.1 (Faz 1b Görev 5): RelatedContentResolutionService's automatic fallback steps -
    // same ContentType, visible, translated, excludes contentItemId itself; when categoryIds is
    // non-empty, further restricted to items sharing at least one category (step 2); null/empty
    // applies no category filter (step 3). Newest effective publish date first, capped at `take`.
    Task<IReadOnlyList<RelatedContentCandidate>> SearchRelatedCandidatesAsync(
        Guid contentTypeId, Guid excludeId, IReadOnlyList<Guid>? categoryIds, LanguageCode languageCode, DateTime now, int take,
        CancellationToken cancellationToken = default);

    // ADR-024 §4.5 (Faz 1b Görev 6): admin trash listing - trashed items only, newest DeletedAtUtc
    // first.
    Task<PagedResult<ContentItemTrashListItem>> SearchTrashedAsync(
        LanguageCode languageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // ADR-024 §4.5 (Faz 1b Görev 6): PermanentlyDeleteExpiredTrashJob's daily candidate set - trashed
    // items whose DeletedAtUtc is older than the retention threshold. Whether each one currently has
    // any child (in any status) still decides eligibility, checked per-candidate via GetChildrenAsync
    // by the job itself (this is a low-volume nightly batch, not a hot path).
    Task<IReadOnlyList<ContentItem>> GetTrashedOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default);

    // ADR-024 §4.5 (Faz 1b Görev 6): PermanentlyDeleteContentItemCommandHandler's cleanup of every
    // ContentItem whose RelatedContentItemIds references the item being hard-deleted ("bu içeriği
    // hedefleyen ContentItemRelation satırları silinir") - loaded as full aggregates so each one can
    // remove the reference through its own ContentItem.SetRelatedContent, not a raw JSON-column rewrite.
    Task<IReadOnlyList<ContentItem>> GetByRelatedContentItemIdAsync(Guid relatedContentItemId, CancellationToken cancellationToken = default);

    // ADR-024 §17 (Faz 1b Görev 7): the public list endpoint's query - same ContentType, visible
    // (own status/schedule, ContentItemVisibility.IsVisibleAt(now) - a scheduled or expired item is
    // excluded exactly like the detail endpoint and related-content resolution already do), translated
    // in languageCode; categoryIds (already expanded to include a selected parent category's children)
    // and tagId narrow further when given. Sort mirrors ContentType.SortMode: Manual -> SortOrder then
    // Title; PublishDateDesc -> effective publish date descending; EventDateAsc (Faz 4 Görev 2) ->
    // linked EventSchedule.StartsAtUtc ascending, items with no schedule yet last.
    Task<PagedResult<PublicContentListItemCandidate>> SearchPublicListAsync(
        Guid contentTypeId,
        LanguageCode languageCode,
        IReadOnlyList<Guid>? categoryIds,
        Guid? tagId,
        string? search,
        DateTime? from,
        DateTime? to,
        bool? featured,
        ContentTypeSortMode sortMode,
        DateTime now,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default);

    // ADR-024 §17 (Faz 1b bugfix): the public list cache's TTL-shortening query - the earliest
    // PublishAtUtc/UnpublishAtUtc among this ContentType's currently-Published, non-deleted items that
    // is still in the future (a schedule on a Draft/Unpublished/Archived item cannot make it visible on
    // its own, so those are excluded the same way ContentItemVisibility does). Null when no such
    // transition exists, in which case the caller falls back to ContentCacheTtlCalculator.DefaultTtl.
    Task<DateTime?> GetEarliestUpcomingTransitionAsync(Guid contentTypeId, DateTime now, CancellationToken cancellationToken = default);

    // Faz 2 Görev 1 master prompt §1.3: the public site cache's own TTL-shortening query - the same
    // rule as the type-scoped overload above, across every content type at once (a menu link can point
    // at any type, so the site-wide bootstrap response's TTL cannot be scoped to just one).
    Task<DateTime?> GetEarliestUpcomingTransitionAsync(DateTime now, CancellationToken cancellationToken = default);

    void Add(ContentItem contentItem);

    void Remove(ContentItem contentItem);
}
