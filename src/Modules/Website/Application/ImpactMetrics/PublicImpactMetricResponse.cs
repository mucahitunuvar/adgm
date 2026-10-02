namespace GenclikMerkezi.Modules.Website.Application.ImpactMetrics;

// Period/Source are nullable: ImpactMetric.Activate only guarantees the DEFAULT language's Period and
// Source are filled (ADR-024 §8.2) - a non-default translation's own Period/Source can still be empty
// even on an active metric, so no non-null guarantee holds once languageCode is anything else.
public sealed record PublicImpactMetricResponse(
    Guid Id,
    decimal Value,
    string? Unit,
    string Label,
    string? Period,
    string? Source,
    string? IconKey,
    int SortOrder);
