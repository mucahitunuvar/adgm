using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class SearchDocumentRepository(WebsiteDbContext dbContext) : ISearchDocumentRepository
{
    public Task<SearchDocument?> GetAsync(
        string sourceKey, string sourceId, LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        dbContext.SearchDocuments.FirstOrDefaultAsync(
            d => d.SourceKey == sourceKey && d.SourceId == sourceId && d.LanguageCode == languageCode,
            cancellationToken);

    public async Task UpsertAsync(SearchDocument document, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.SearchDocuments.FirstOrDefaultAsync(
            d => d.SourceKey == document.SourceKey && d.SourceId == document.SourceId && d.LanguageCode == document.LanguageCode,
            cancellationToken);

        if (existing is null)
        {
            dbContext.SearchDocuments.Add(document);
            return;
        }

        if (ReferenceEquals(existing, document))
        {
            return;
        }

        existing.Refresh(
            document.TypeKey,
            document.Title,
            document.Summary,
            document.Url,
            document.NormalizedText,
            document.PublishedAtUtc,
            document.IndexedAtUtc,
            document.IncludeInSitemap);
    }

    public Task<int> DeleteAsync(
        string sourceKey, string sourceId, LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        dbContext.SearchDocuments
            .Where(d => d.SourceKey == sourceKey && d.SourceId == sourceId && d.LanguageCode == languageCode)
            .ExecuteDeleteAsync(cancellationToken);

    public Task<int> DeleteBySourceAndIdsAsync(
        string sourceKey, IReadOnlyCollection<string> sourceIds, CancellationToken cancellationToken = default)
    {
        if (sourceIds.Count == 0)
        {
            return Task.FromResult(0);
        }

        return dbContext.SearchDocuments
            .Where(d => d.SourceKey == sourceKey && sourceIds.Contains(d.SourceId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task<int> DeleteUnseenBySourceAsync(
        string sourceKey, IReadOnlyCollection<string> seenSourceIds, CancellationToken cancellationToken = default) =>
        dbContext.SearchDocuments
            .Where(d => d.SourceKey == sourceKey && !seenSourceIds.Contains(d.SourceId))
            .ExecuteDeleteAsync(cancellationToken);

    public Task<int> CountBySourceAsync(string sourceKey, CancellationToken cancellationToken = default) =>
        dbContext.SearchDocuments.CountAsync(d => d.SourceKey == sourceKey, cancellationToken);
}
