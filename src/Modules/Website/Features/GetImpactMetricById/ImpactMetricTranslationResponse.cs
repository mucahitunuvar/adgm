namespace GenclikMerkezi.Modules.Website.Features.GetImpactMetricById;

public sealed record ImpactMetricTranslationResponse(string LanguageCode, string Label, string? Unit, string? Period, string? Source);
