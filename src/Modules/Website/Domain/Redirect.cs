using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 1a Görev 4 builds the aggregate for automatic redirects only; Görev 5 adds manual
// creation/editing endpoints on top of the same shape). (LanguageCode, FromPath) is unique - enforced
// by IRedirectRepository callers and a DB index, the same split every other cross-aggregate uniqueness
// rule in this module uses (ContentType.Key, ContentItem's FullPath).
public sealed class Redirect : AggregateRoot
{
    public LanguageCode LanguageCode { get; private set; } = null!;

    public string FromPath { get; private set; } = string.Empty;

    public RedirectTargetKind TargetKind { get; private set; }

    public Guid? TargetContentItemId { get; private set; }

    public string? TargetPath { get; private set; }

    public RedirectStatusCode StatusCode { get; private set; }

    public bool IsAutomatic { get; private set; }

    public int HitCount { get; private set; }

    public DateTime? LastHitAtUtc { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Redirect(
        Guid id, LanguageCode languageCode, string fromPath, RedirectTargetKind targetKind, Guid? targetContentItemId,
        string? targetPath, RedirectStatusCode statusCode, bool isAutomatic, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        LanguageCode = languageCode;
        FromPath = fromPath;
        TargetKind = targetKind;
        TargetContentItemId = targetContentItemId;
        TargetPath = targetPath;
        StatusCode = statusCode;
        IsAutomatic = isAutomatic;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private Redirect()
    {
    }

    // The only redirect shape Faz 1a Görev 4 ever creates: always MovedPermanently, always pointing at
    // a ContentItem's live (recomputed at resolution time, not stored) FullPath. Manual redirects -
    // TargetKind.Path, an admin-chosen StatusCode - are Görev 5's CreateRedirect command.
    public static Result<Redirect> CreateAutomatic(
        LanguageCode languageCode, string? fromPath, Guid targetContentItemId, Guid createdByUserId, DateTime createdAtUtc)
    {
        var normalizedResult = NormalizeFromPath(fromPath);
        if (normalizedResult.IsFailure)
        {
            return Result.Failure<Redirect>(normalizedResult.Error);
        }

        return Result.Success(new Redirect(
            Guid.NewGuid(), languageCode, normalizedResult.Value, RedirectTargetKind.ContentItem, targetContentItemId,
            null, RedirectStatusCode.MovedPermanently, isAutomatic: true, createdByUserId, createdAtUtc));
    }

    private static Result<string> NormalizeFromPath(string? fromPath)
    {
        var normalized = (fromPath ?? string.Empty).Trim('/');
        return normalized.Length == 0
            ? Result.Failure<string>(Error.Validation("Redirect.FromPathRequired", "FromPath is required."))
            : Result.Success(normalized);
    }
}
