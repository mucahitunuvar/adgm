namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetricTranslation;

public sealed record UpdateImpactMetricTranslationRequest(byte[] RowVersion, string? Label, string? Unit, string? Period, string? Source);
