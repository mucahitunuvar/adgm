namespace GenclikMerkezi.Modules.Website.Features.GetSearchSources;

public sealed record SearchSourceSummaryResponse(
    string SourceKey,
    DateTime? LastStartedAtUtc,
    DateTime? LastSucceededAtUtc,
    string? LastError,
    int DocumentCount);
