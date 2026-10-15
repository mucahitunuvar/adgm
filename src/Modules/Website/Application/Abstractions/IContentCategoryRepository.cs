using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IContentCategoryRepository
{
    Task<ContentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentCategory>> GetByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default);

    // ADR-024 §17 (Faz 1b Görev 7): the public list endpoint's `category` filter resolves a slug (not
    // an id) within the requested type and language.
    Task<ContentCategory?> GetByTypeAndSlugAsync(
        Guid contentTypeId, LanguageCode languageCode, string slug, CancellationToken cancellationToken = default);

    // Cross-aggregate uniqueness check (ADR-024 §4.1): a language's slug must be unique within its
    // ContentType, excludeId lets an update check against every OTHER category of the same type
    // without a query for "does this category collide with itself" always trivially succeeding.
    Task<bool> SlugExistsAsync(
        Guid contentTypeId, LanguageCode languageCode, string slug, Guid? excludeId, CancellationToken cancellationToken = default);

    // DeleteContentCategoryCommandHandler's guard (ADR-024 §4.1: "alt kategorisi varsa 409" half - the
    // "içerik atanmışsa" half is IContentItemRepository.CountByCategoryIdAsync).
    Task<int> CountChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default);

    // GetContentItemsQueryHandler's `categoryId` filter expansion - categoryId itself plus its direct
    // children, the same "include the category's own subtree" rule GetPublicContentsQueryHandler
    // already applies to the public list's `category` filter. Safe to stop at one level: ADR-024
    // §4.1 caps categories at exactly 2 levels (CreateContentCategoryCommandHandler rejects a parent
    // that already has a parent of its own), so a category's children can never have children of
    // their own. Empty when categoryId does not exist - the caller then filters on an empty set
    // rather than treating a bad id as "no filter" or a 404 (no other admin list filter here
    // validates id existence either, e.g. contentTypeId/parentId).
    Task<IReadOnlyList<Guid>> GetSelfAndChildrenIdsAsync(Guid categoryId, CancellationToken cancellationToken = default);

    void Add(ContentCategory category);

    void Remove(ContentCategory category);
}
