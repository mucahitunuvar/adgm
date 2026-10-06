namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// ADR-024 §13 (Faz 3 Görev 7): "scripts alanı: aktif script'lerin tipli tanımları (frontend yalnızca
// onay verilen kategorilerdekini yükler)" - every active script's full typed definition, so the
// frontend can render the right tag (<script src=.../> vs a provider-specific snippet) once the
// visitor has consented to its Category.
public sealed record PublicSiteScriptResponse(
    Guid Id,
    string Provider,
    string Category,
    string Placement,
    string? MeasurementId,
    string? ContainerId,
    string? PixelId,
    string? Src,
    bool Async,
    bool Defer);
