using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 1a Görev 4 built the aggregate for automatic redirects; Görev 5 adds manual
// creation/editing on the same shape). (LanguageCode, FromPath) is unique - enforced by
// IRedirectRepository callers and a DB index, the same split every other cross-aggregate uniqueness
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

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

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

    // The only redirect shape Faz 1a Görev 4 creates: always MovedPermanently, always pointing at a
    // ContentItem's live (recomputed at resolution time, not stored) FullPath.
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

    // ADR-024 §15 (Faz 1a Görev 5): an admin-created redirect - either kind, either status code. Cross-
    // aggregate checks this cannot do itself (FromPath colliding with a live ContentItem's FullPath,
    // or with another Redirect's FromPath - a chain) are the Application-layer command handler's job.
    public static Result<Redirect> Create(
        LanguageCode languageCode, string? fromPath, RedirectTargetKind targetKind, Guid? targetContentItemId,
        string? targetPath, RedirectStatusCode statusCode, Guid createdByUserId, DateTime createdAtUtc)
    {
        var fromPathResult = NormalizeFromPath(fromPath);
        if (fromPathResult.IsFailure)
        {
            return Result.Failure<Redirect>(fromPathResult.Error);
        }

        var targetResult = ValidateTarget(targetKind, targetContentItemId, targetPath, fromPathResult.Value);
        if (targetResult.IsFailure)
        {
            return Result.Failure<Redirect>(targetResult.Error);
        }

        return Result.Success(new Redirect(
            Guid.NewGuid(), languageCode, fromPathResult.Value, targetKind,
            targetKind == RedirectTargetKind.ContentItem ? targetContentItemId : null,
            targetKind == RedirectTargetKind.Path ? targetResult.Value : null,
            statusCode, isAutomatic: false, createdByUserId, createdAtUtc));
    }

    // FromPath itself never changes after creation (it is effectively this redirect's identity - Görev
    // 5's PUT endpoint only ever edits where it points, never what it catches).
    public Result Update(
        RedirectTargetKind targetKind, Guid? targetContentItemId, string? targetPath, RedirectStatusCode statusCode,
        Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (IsAutomatic)
        {
            return Result.Failure(Error.Conflict(
                "Redirect.CannotEditAutomaticRedirect", "Automatic redirects cannot be edited; delete it instead."));
        }

        var targetResult = ValidateTarget(targetKind, targetContentItemId, targetPath, FromPath);
        if (targetResult.IsFailure)
        {
            return targetResult;
        }

        TargetKind = targetKind;
        TargetContentItemId = targetKind == RedirectTargetKind.ContentItem ? targetContentItemId : null;
        TargetPath = targetKind == RedirectTargetKind.Path ? targetResult.Value : null;
        StatusCode = statusCode;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public void RecordHit(DateTime now)
    {
        HitCount++;
        LastHitAtUtc = now;
    }

    private static Result<string> ValidateTarget(
        RedirectTargetKind targetKind, Guid? targetContentItemId, string? targetPath, string fromPath)
    {
        if (targetKind == RedirectTargetKind.ContentItem)
        {
            return targetContentItemId is null
                ? Result.Failure<string>(Error.Validation("Redirect.TargetContentItemIdRequired", "A target content item is required."))
                : Result.Success(string.Empty);
        }

        var normalizedResult = NormalizeTargetPath(targetPath);
        if (normalizedResult.IsFailure)
        {
            return normalizedResult;
        }

        if (string.Equals(normalizedResult.Value, fromPath, StringComparison.Ordinal))
        {
            return Result.Failure<string>(Error.Validation(
                "Redirect.TargetCannotBeFromPath", "The target path cannot be the same as FromPath."));
        }

        return normalizedResult;
    }

    // A relative target is normalized exactly like FromPath (no leading/trailing slash) so Görev 6's
    // route resolution can treat both the same way; an absolute target must be https (ADR-024 §15 -
    // never http, javascript:, or any other scheme).
    private static Result<string> NormalizeTargetPath(string? targetPath)
    {
        var trimmed = (targetPath ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return Result.Failure<string>(Error.Validation("Redirect.TargetPathRequired", "TargetPath is required."));
        }

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return uri.Scheme == Uri.UriSchemeHttps
                ? Result.Success(trimmed)
                : Result.Failure<string>(Error.Validation("Redirect.TargetPathMustBeHttps", "An absolute target URL must use https."));
        }

        return Result.Success(trimmed.Trim('/'));
    }

    private static Result<string> NormalizeFromPath(string? fromPath)
    {
        var normalized = (fromPath ?? string.Empty).Trim('/');
        return normalized.Length == 0
            ? Result.Failure<string>(Error.Validation("Redirect.FromPathRequired", "FromPath is required."))
            : Result.Success(normalized);
    }
}
