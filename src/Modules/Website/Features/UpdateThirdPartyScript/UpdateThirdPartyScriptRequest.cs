namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScript;

public sealed record UpdateThirdPartyScriptRequest(
    byte[] RowVersion,
    string? Provider,
    string? MeasurementId,
    string? ContainerId,
    string? PixelId,
    string? Src,
    bool Async,
    bool Defer,
    string? Category,
    string? Placement,
    int SortOrder);
