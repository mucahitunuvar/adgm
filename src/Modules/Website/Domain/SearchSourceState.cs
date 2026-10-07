using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §10 (Faz 5 Görev 1): one row per registered search source (the external
// IExternalSearchSource implementations Görev 4 syncs, plus "website" for Görev 2's own content
// reconciliation job), tracking that source's last run so an admin can see sync health (Görev 4's
// GET .../search/sources). Not written to by Görev 1 itself - only the shape is defined here.
public sealed class SearchSourceState : Entity
{
    public const int MaxSourceKeyLength = 50;
    public const int MaxLastErrorLength = 500;

    public string SourceKey { get; private set; } = string.Empty;

    public DateTime? LastStartedAtUtc { get; private set; }

    public DateTime? LastSucceededAtUtc { get; private set; }

    // Truncated exception message, no stack trace (ADR-024 §10 / SECURITY.md: an admin-only
    // diagnostics field, but still not a dumping ground for arbitrary exception detail).
    public string? LastError { get; private set; }

    public int DocumentCount { get; private set; }

    private SearchSourceState(Guid id, string sourceKey)
        : base(id)
    {
        SourceKey = sourceKey;
    }

    private SearchSourceState()
    {
    }

    public static Result<SearchSourceState> Create(string sourceKey)
    {
        if (string.IsNullOrWhiteSpace(sourceKey) || sourceKey.Length > MaxSourceKeyLength)
        {
            return Result.Failure<SearchSourceState>(Error.Validation(
                "SearchSourceState.SourceKeyInvalid",
                $"Source key is required and must be at most {MaxSourceKeyLength} characters."));
        }

        return Result.Success(new SearchSourceState(Guid.NewGuid(), sourceKey.Trim()));
    }

    public void MarkStarted(DateTime startedAtUtc)
    {
        LastStartedAtUtc = startedAtUtc;
    }

    public void MarkSucceeded(DateTime succeededAtUtc, int documentCount)
    {
        LastSucceededAtUtc = succeededAtUtc;
        LastError = null;
        DocumentCount = documentCount;
    }

    public void MarkFailed(string error)
    {
        LastError = error.Length > MaxLastErrorLength ? error[..MaxLastErrorLength] : error;
    }
}
