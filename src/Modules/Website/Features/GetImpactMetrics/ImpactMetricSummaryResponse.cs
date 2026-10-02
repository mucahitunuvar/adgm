namespace GenclikMerkezi.Modules.Website.Features.GetImpactMetrics;

public sealed record ImpactMetricSummaryResponse(
    Guid Id,
    decimal Value,
    string? Unit,
    string Label,
    string? IconKey,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion);
