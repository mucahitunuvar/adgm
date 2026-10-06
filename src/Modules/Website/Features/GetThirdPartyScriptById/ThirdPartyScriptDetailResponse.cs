namespace GenclikMerkezi.Modules.Website.Features.GetThirdPartyScriptById;

public sealed record ThirdPartyScriptDetailResponse(
    Guid Id,
    string Provider,
    string? MeasurementId,
    string? ContainerId,
    string? PixelId,
    string? Src,
    bool Async,
    bool Defer,
    string Category,
    string Placement,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion,
    IReadOnlyList<ThirdPartyScriptTranslationResponse> Translations,
    DateTime CreatedAtUtc);
