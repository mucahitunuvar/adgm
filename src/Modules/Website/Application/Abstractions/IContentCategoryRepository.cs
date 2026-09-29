using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IContentCategoryRepository
{
    Task<ContentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentCategory>> GetByContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default);

    // Cross-aggregate uniqueness check (ADR-024 §4.1): a language's slug must be unique within its
    // ContentType, excludeId lets an update check against every OTHER category of the same type
    // without a query for "does this category collide with itself" always trivially succeeding.
    Task<bool> SlugExistsAsync(
        Guid contentTypeId, LanguageCode languageCode, string slug, Guid? excludeId, CancellationToken cancellationToken = default);

    // DeleteContentCategoryCommandHandler's guard (ADR-024 §4.1: "alt kategorisi varsa 409" half - the
    // "içerik atanmışsa" half is IContentItemRepository.CountByCategoryIdAsync).
    Task<int> CountChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default);

    void Add(ContentCategory category);

    void Remove(ContentCategory category);
}
