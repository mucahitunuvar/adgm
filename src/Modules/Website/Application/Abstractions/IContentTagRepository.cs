using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IContentTagRepository
{
    Task<ContentTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContentTag?> GetBySlugAsync(LanguageCode languageCode, string slug, CancellationToken cancellationToken = default);

    // Default sort is usage count descending is the caller's job (usage isn't stored on ContentTag
    // itself - see CountUsagesAsync) - this just returns the raw page for the requested filters.
    Task<PagedResult<ContentTag>> SearchAsync(
        LanguageCode? languageCode, string? search, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // How many ContentItemTranslations currently reference this tag (ADR-024 §4.1: shown in GET /tags,
    // and used by the 30-day cleanup job to find genuinely unused tags).
    Task<int> CountUsagesAsync(Guid tagId, CancellationToken cancellationToken = default);

    // The daily cleanup job's candidate set: tags older than the threshold with zero current usages.
    Task<IReadOnlyList<ContentTag>> GetUnusedOlderThanAsync(DateTime olderThanUtc, CancellationToken cancellationToken = default);

    void Add(ContentTag tag);

    void Remove(ContentTag tag);
}
