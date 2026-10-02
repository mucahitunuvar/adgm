namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetric;

public sealed record UpdateImpactMetricRequest(byte[] RowVersion, decimal Value, string? IconKey, int SortOrder);
