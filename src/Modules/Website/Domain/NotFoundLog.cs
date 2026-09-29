using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 1a Görev 5). Deliberately minimal - no IP, User-Agent or referrer (SECURITY.md /
// ADR-024 §15: an anonymous visitor's 404 is counted, not tracked). No audit user fields either: this
// is written by the public route-resolution endpoint itself (Görev 6), not an admin action.
public sealed class NotFoundLog : AggregateRoot
{
    public const int MaxPathLength = 500;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Path { get; private set; } = string.Empty;

    public int HitCount { get; private set; }

    public DateTime FirstSeenAtUtc { get; private set; }

    public DateTime LastSeenAtUtc { get; private set; }

    private NotFoundLog(Guid id, LanguageCode languageCode, string path, DateTime seenAtUtc)
        : base(id)
    {
        LanguageCode = languageCode;
        Path = path;
        HitCount = 1;
        FirstSeenAtUtc = seenAtUtc;
        LastSeenAtUtc = seenAtUtc;
    }

    private NotFoundLog()
    {
    }

    public static Result<NotFoundLog> Create(LanguageCode languageCode, string? path, DateTime seenAtUtc)
    {
        var normalizedResult = NormalizePath(path);
        return normalizedResult.IsFailure
            ? Result.Failure<NotFoundLog>(normalizedResult.Error)
            : Result.Success(new NotFoundLog(Guid.NewGuid(), languageCode, normalizedResult.Value, seenAtUtc));
    }

    public void RecordHit(DateTime seenAtUtc)
    {
        HitCount++;
        LastSeenAtUtc = seenAtUtc;
    }

    // Shares RoutePathFormat.Normalize with Redirect.NormalizeFromPath (fix: a single path
    // normalization rule module-wide, instead of two diverging ones) - a NotFoundLog's Path is stored
    // in exactly the shape Redirect.FromPath expects, so converting a log to a redirect (see
    // ConvertNotFoundPathToRedirectCommandHandler) never needs its own re-normalization.
    private static Result<string> NormalizePath(string? path)
    {
        var normalized = RoutePathFormat.Normalize(path);
        if (normalized.Length == 0 || normalized.Length > MaxPathLength)
        {
            return Result.Failure<string>(Error.Validation(
                "NotFoundLog.PathInvalid", $"Path is required and must be at most {MaxPathLength} characters."));
        }

        return Result.Success(normalized);
    }
}
