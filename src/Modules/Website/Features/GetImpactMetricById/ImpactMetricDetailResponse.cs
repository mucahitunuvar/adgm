namespace GenclikMerkezi.Modules.Website.Features.GetImpactMetricById;

public sealed record ImpactMetricDetailResponse(
    Guid Id,
    decimal Value,
    string? IconKey,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion,
    IReadOnlyList<ImpactMetricTranslationResponse> Translations,
    DateTime CreatedAtUtc);
