using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSearch;

// ADR-024 §10 (Faz 5 Görev 3). Deliberately uncached (Görev 3's own decision - the key space is
// unbounded, same reasoning GetPublicContentsQueryHandler already applies to its own `search` param)
// and the raw query text `request.Q` is never logged or persisted anywhere in this handler (SECURITY.md:
// no popular-search/analytics feature exists, and `q` may incidentally contain personal data).
public sealed class GetPublicSearchQueryHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ISearchDocumentRepository searchDocumentRepository)
    : IRequestHandler<GetPublicSearchQuery, Result<PublicSearchResponse>>
{
    public const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    // Mirrors GetPublicContentsQueryHandler's own MinSearchLength/MaxSearchLength (2-100).
    private const int MinQueryLength = 2;
    private const int MaxQueryLength = 100;

    private static readonly Error SearchNotAvailableError = Error.NotFound(
        "Website.Search.NotAvailable", "Global search is not available.");

    private static readonly Error LanguageNotFoundError = Error.NotFound(
        "SiteLanguage.NotFound", "The specified site language could not be found.");

    public async Task<Result<PublicSearchResponse>> Handle(GetPublicSearchQuery request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        if (!settings.GlobalSearchEnabled)
        {
            return Result.Failure<PublicSearchResponse>(SearchNotAvailableError);
        }

        var trimmedQuery = (request.Q ?? string.Empty).Trim();
        if (trimmedQuery.Length < MinQueryLength || trimmedQuery.Length > MaxQueryLength)
        {
            return Result.Failure<PublicSearchResponse>(Error.Validation(
                "Search.QueryLengthInvalid", $"Search query must be between {MinQueryLength} and {MaxQueryLength} characters."));
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        SiteLanguage resolvedLanguage;
        if (!string.IsNullOrWhiteSpace(request.Lang))
        {
            var matchedLanguage = activeLanguages.FirstOrDefault(
                l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase));
            if (matchedLanguage is null)
            {
                return Result.Failure<PublicSearchResponse>(LanguageNotFoundError);
            }

            resolvedLanguage = matchedLanguage;
        }
        else
        {
            resolvedLanguage = activeLanguages.First(l => l.IsDefault);
        }

        var likePatterns = SearchQueryTokenizer.BuildLikePatterns(trimmedQuery);

        var typeKeys = string.IsNullOrWhiteSpace(request.Type)
            ? null
            : request.Type
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .ToList();

        var sourceKey = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim().ToLowerInvariant();

        var pagedRequest = new PagedRequest { Page = request.Page, PageSize = Math.Min(request.PageSize, MaxPageSize) };

        var queryResult = await searchDocumentRepository.SearchAsync(
            resolvedLanguage.Code, likePatterns, typeKeys, sourceKey, pagedRequest, cancellationToken);

        var items = queryResult.Page.Items
            .Select(d => new PublicSearchResultItemResponse(d.SourceKey, d.TypeKey, d.Title, d.Summary, d.Url, d.PublishedAtUtc))
            .ToList();

        return Result.Success(new PublicSearchResponse(
            new PagedResult<PublicSearchResultItemResponse>(items, queryResult.Page.TotalCount, queryResult.Page.Page, queryResult.Page.PageSize),
            queryResult.TypeCounts));
    }
}
