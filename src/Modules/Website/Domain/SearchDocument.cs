using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §10 (Faz 5 Görev 1): denormalized search read-model. Fed by Website's own content, same
// transaction as the content mutation (Görev 2), and by external sources through
// IExternalSearchSource's pull model (Görev 4, full-sync job) - SourceKey distinguishes "website"
// from e.g. "employer.job". Deliberately a plain Entity, not an AggregateRoot: nothing downstream
// reacts to a document being indexed/removed, so it carries no domain events and no audit fields of
// its own (IndexedAtUtc is the only "when" this needs).
public sealed class SearchDocument : Entity
{
    public const int MaxSourceKeyLength = 50;
    public const int MaxSourceIdLength = 100;
    public const int MaxTypeKeyLength = 50;
    public const int MaxTitleLength = 300;
    public const int MaxSummaryLength = 500;
    public const int MaxUrlLength = 500;
    public const int MaxNormalizedTextLength = 4000;

    public string SourceKey { get; private set; } = string.Empty;

    public string SourceId { get; private set; } = string.Empty;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string TypeKey { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    // SearchTextBuilder.BuildNormalizedText output (title + summary + short body excerpt, folded
    // through TurkishTextNormalizer) - never displayed, only matched against (Görev 3).
    public string NormalizedText { get; private set; } = string.Empty;

    public DateTime PublishedAtUtc { get; private set; }

    public DateTime IndexedAtUtc { get; private set; }

    // False for NoIndex content (still searchable, ADR-024 §4.4) - Görev 5's sitemap excludes it.
    public bool IncludeInSitemap { get; private set; }

    private SearchDocument(Guid id, string sourceKey, string sourceId, LanguageCode languageCode)
        : base(id)
    {
        SourceKey = sourceKey;
        SourceId = sourceId;
        LanguageCode = languageCode;
    }

    private SearchDocument()
    {
    }

    public static Result<SearchDocument> Create(
        string sourceKey,
        string sourceId,
        LanguageCode languageCode,
        string typeKey,
        string title,
        string summary,
        string url,
        string normalizedText,
        DateTime publishedAtUtc,
        DateTime indexedAtUtc,
        bool includeInSitemap)
    {
        var keyValidation = ValidateKey(sourceKey, sourceId);
        if (keyValidation.IsFailure)
        {
            return Result.Failure<SearchDocument>(keyValidation.Error);
        }

        var document = new SearchDocument(Guid.NewGuid(), sourceKey.Trim(), sourceId.Trim(), languageCode);
        var refreshResult = document.Refresh(
            typeKey, title, summary, url, normalizedText, publishedAtUtc, indexedAtUtc, includeInSitemap);

        return refreshResult.IsFailure
            ? Result.Failure<SearchDocument>(refreshResult.Error)
            : Result.Success(document);
    }

    // SourceKey/SourceId/LanguageCode are the natural key (ISearchDocumentRepository.UpsertAsync) and
    // never change here - only the content-derived fields are refreshed in place.
    public Result Refresh(
        string typeKey,
        string title,
        string summary,
        string url,
        string normalizedText,
        DateTime publishedAtUtc,
        DateTime indexedAtUtc,
        bool includeInSitemap)
    {
        if (string.IsNullOrWhiteSpace(typeKey) || typeKey.Length > MaxTypeKeyLength)
        {
            return Result.Failure(Error.Validation(
                "SearchDocument.TypeKeyInvalid", $"Type key is required and must be at most {MaxTypeKeyLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(title) || title.Length > MaxTitleLength)
        {
            return Result.Failure(Error.Validation(
                "SearchDocument.TitleInvalid", $"Title is required and must be at most {MaxTitleLength} characters."));
        }

        if (summary.Length > MaxSummaryLength)
        {
            return Result.Failure(Error.Validation(
                "SearchDocument.SummaryTooLong", $"Summary must be at most {MaxSummaryLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(url) || url.Length > MaxUrlLength)
        {
            return Result.Failure(Error.Validation(
                "SearchDocument.UrlInvalid", $"Url is required and must be at most {MaxUrlLength} characters."));
        }

        if (normalizedText.Length > MaxNormalizedTextLength)
        {
            return Result.Failure(Error.Validation(
                "SearchDocument.NormalizedTextTooLong",
                $"Normalized text must be at most {MaxNormalizedTextLength} characters."));
        }

        TypeKey = typeKey.Trim();
        Title = title.Trim();
        Summary = summary.Trim();
        Url = url.Trim();
        NormalizedText = normalizedText;
        PublishedAtUtc = publishedAtUtc;
        IndexedAtUtc = indexedAtUtc;
        IncludeInSitemap = includeInSitemap;

        return Result.Success();
    }

    private static Result ValidateKey(string sourceKey, string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceKey) || sourceKey.Length > MaxSourceKeyLength)
        {
            return Result.Failure(Error.Validation(
                "SearchDocument.SourceKeyInvalid", $"Source key is required and must be at most {MaxSourceKeyLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(sourceId) || sourceId.Length > MaxSourceIdLength)
        {
            return Result.Failure(Error.Validation(
                "SearchDocument.SourceIdInvalid", $"Source id is required and must be at most {MaxSourceIdLength} characters."));
        }

        return Result.Success();
    }
}
