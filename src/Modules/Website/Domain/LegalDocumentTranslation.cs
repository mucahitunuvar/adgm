using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: a LegalDocument's per-language Title - used both as the admin-facing name in the
// panel and as the public page's title. Mirrors VideoTranslation's shape: Update is internal (only
// LegalDocument, the aggregate root, calls it).
public sealed class LegalDocumentTranslation : Entity
{
    public const int MaxTitleLength = 200;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    private LegalDocumentTranslation(Guid id, LanguageCode languageCode, string title)
        : base(id)
    {
        LanguageCode = languageCode;
        Title = title;
    }

    private LegalDocumentTranslation()
    {
    }

    public static Result<LegalDocumentTranslation> Create(LanguageCode languageCode, string? title)
    {
        var titleResult = NormalizeTitle(title);
        if (titleResult.IsFailure)
        {
            return Result.Failure<LegalDocumentTranslation>(titleResult.Error);
        }

        return Result.Success(new LegalDocumentTranslation(Guid.NewGuid(), languageCode, titleResult.Value));
    }

    internal Result Update(string? title)
    {
        var titleResult = NormalizeTitle(title);
        if (titleResult.IsFailure)
        {
            return titleResult;
        }

        Title = titleResult.Value;

        return Result.Success();
    }

    private static Result<string> NormalizeTitle(string? title)
    {
        var normalized = (title ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxTitleLength)
        {
            return Result.Failure<string>(Error.Validation(
                "LegalDocumentTranslation.TitleInvalid", $"Title is required and must be at most {MaxTitleLength} characters."));
        }

        return Result.Success(normalized);
    }
}
