using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: a LegalDocumentVersion's per-language Body - the sanitized rich text an admin edits
// while the owning version is a Draft (Infrastructure.Sanitization.HtmlSanitizerContentSanitizer runs
// in the handler before this ever sees the string, the same split ContentItemTranslation.Body uses).
// A translation only exists once an admin has written that language's body, so Body is always required
// and non-empty here - there is no "empty but present" state.
public sealed class LegalDocumentVersionTranslation : Entity
{
    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Body { get; private set; } = string.Empty;

    private LegalDocumentVersionTranslation(Guid id, LanguageCode languageCode, string body)
        : base(id)
    {
        LanguageCode = languageCode;
        Body = body;
    }

    private LegalDocumentVersionTranslation()
    {
    }

    public static Result<LegalDocumentVersionTranslation> Create(LanguageCode languageCode, string? body)
    {
        var bodyResult = NormalizeBody(body);
        if (bodyResult.IsFailure)
        {
            return Result.Failure<LegalDocumentVersionTranslation>(bodyResult.Error);
        }

        return Result.Success(new LegalDocumentVersionTranslation(Guid.NewGuid(), languageCode, bodyResult.Value));
    }

    internal Result Update(string? body)
    {
        var bodyResult = NormalizeBody(body);
        if (bodyResult.IsFailure)
        {
            return bodyResult;
        }

        Body = bodyResult.Value;

        return Result.Success();
    }

    private static Result<string> NormalizeBody(string? body)
    {
        var trimmed = (body ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return Result.Failure<string>(Error.Validation("LegalDocumentVersionTranslation.BodyRequired", "Body is required."));
        }

        return Result.Success(trimmed);
    }
}
