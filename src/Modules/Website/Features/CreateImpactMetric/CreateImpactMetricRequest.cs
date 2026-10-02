namespace GenclikMerkezi.Modules.Website.Features.CreateImpactMetric;

public sealed record CreateImpactMetricRequest(
    decimal Value,
    string? IconKey,
    int SortOrder,
    string? DefaultLanguageLabel,
    string? DefaultLanguageUnit,
    string? DefaultLanguagePeriod,
    string? DefaultLanguageSource);
