namespace GenclikMerkezi.Modules.Website.Features.GetRedirects;

public sealed record RedirectResponse(
    Guid Id,
    string LanguageCode,
    string FromPath,
    string TargetKind,
    Guid? TargetContentItemId,
    string? TargetPath,
    string StatusCode,
    bool IsAutomatic,
    int HitCount,
    DateTime? LastHitAtUtc,
    DateTime CreatedAtUtc);
