using System.Linq.Expressions;
using System.Reflection;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class SearchDocumentRepository(WebsiteDbContext dbContext) : ISearchDocumentRepository
{
    // The 4-arg EF.Functions.Like(matchExpression, pattern, escapeCharacter) overload - resolved once
    // by reflection so BuildAllLikeExpression (Görev 3) can build an N-pattern AND entirely as an
    // expression tree (needed for the title-priority ORDER BY key, where a plain foreach of .Where()
    // calls - used for the NormalizedText filter below - cannot be used since it would filter rows
    // instead of producing a boolean sort key).
    private static readonly MethodInfo LikeMethod = typeof(DbFunctionsExtensions).GetMethod(
        nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!;

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

    public async Task<PublicSearchQueryResult> SearchAsync(
        LanguageCode languageCode,
        IReadOnlyList<string> likePatterns,
        IReadOnlyList<string>? typeKeys,
        string? sourceKey,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.SearchDocuments.AsNoTracking().Where(d => d.LanguageCode == languageCode);

        foreach (var pattern in likePatterns)
        {
            var capturedPattern = pattern;
            baseQuery = baseQuery.Where(d => EF.Functions.Like(d.NormalizedText, capturedPattern, SearchQueryTokenizer.LikeEscapeCharacter));
        }

        if (!string.IsNullOrWhiteSpace(sourceKey))
        {
            baseQuery = baseQuery.Where(d => d.SourceKey == sourceKey);
        }

        // Faceted type counts for the current language/source/query, deliberately computed without the
        // type filter itself (single grouped query, Görev 3).
        var typeCounts = await baseQuery
            .GroupBy(d => d.TypeKey)
            .Select(g => new { TypeKey = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TypeKey, x => x.Count, cancellationToken);

        var filteredQuery = typeKeys is { Count: > 0 }
            ? baseQuery.Where(d => typeKeys.Contains(d.TypeKey))
            : baseQuery;

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var titleMatchesAllTokens = BuildAllLikeExpression(d => d.Title, likePatterns);
        var items = await filteredQuery
            .OrderByDescending(titleMatchesAllTokens)
            .ThenByDescending(d => d.PublishedAtUtc)
            .ThenBy(d => d.Id)
            .Skip((pagedRequest.Page - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .Select(d => new PublicSearchResultCandidate(d.SourceKey, d.TypeKey, d.Title, d.Summary, d.Url, d.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        return new PublicSearchQueryResult(
            new PagedResult<PublicSearchResultCandidate>(items, totalCount, pagedRequest.Page, pagedRequest.PageSize), typeCounts);
    }

    public async Task<IReadOnlyList<SearchDocument>> GetSitemapEligibleExternalAsync(
        string excludedSourceKey, CancellationToken cancellationToken = default) =>
        await dbContext.SearchDocuments.AsNoTracking()
            .Where(d => d.SourceKey != excludedSourceKey && d.IncludeInSitemap)
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

    private static Expression<Func<SearchDocument, bool>> BuildAllLikeExpression(
        Expression<Func<SearchDocument, string>> fieldSelector, IReadOnlyList<string> likePatterns)
    {
        var parameter = fieldSelector.Parameters[0];
        var dbFunctions = Expression.Constant(EF.Functions);
        Expression body = Expression.Constant(true);

        foreach (var pattern in likePatterns)
        {
            var likeCall = Expression.Call(
                LikeMethod, dbFunctions, fieldSelector.Body, Expression.Constant(pattern),
                Expression.Constant(SearchQueryTokenizer.LikeEscapeCharacter));
            body = Expression.AndAlso(body, likeCall);
        }

        return Expression.Lambda<Func<SearchDocument, bool>>(body, parameter);
    }
}
