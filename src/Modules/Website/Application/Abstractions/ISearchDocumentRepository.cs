using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface ISearchDocumentRepository
{
    Task<SearchDocument?> GetAsync(
        string sourceKey, string sourceId, LanguageCode languageCode, CancellationToken cancellationToken = default);

    // Finds the existing (SourceKey, SourceId, LanguageCode) row and refreshes it in place, or adds
    // `document` as a new row - the caller never needs to know which happened.
    Task UpsertAsync(SearchDocument document, CancellationToken cancellationToken = default);

    Task<int> DeleteAsync(
        string sourceKey, string sourceId, LanguageCode languageCode, CancellationToken cancellationToken = default);

    // All languages of the given source ids at once - e.g. a content item (every translation) leaving
    // the searchable set (Görev 2: archived, trashed, type turned non-searchable, permanently deleted).
    Task<int> DeleteBySourceAndIdsAsync(
        string sourceKey, IReadOnlyCollection<string> sourceIds, CancellationToken cancellationToken = default);

    // Full-sync cleanup (Görev 4): removes every document of `sourceKey` whose SourceId was not seen
    // in this sync pass - including an empty seen set, which correctly clears the whole source (e.g.
    // PublicJobListingsEnabled = false).
    Task<int> DeleteUnseenBySourceAsync(
        string sourceKey, IReadOnlyCollection<string> seenSourceIds, CancellationToken cancellationToken = default);

    Task<int> CountBySourceAsync(string sourceKey, CancellationToken cancellationToken = default);
}
