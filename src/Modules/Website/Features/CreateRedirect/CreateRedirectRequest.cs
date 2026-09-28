namespace GenclikMerkezi.Modules.Website.Features.CreateRedirect;

public sealed record CreateRedirectRequest(
    string? LanguageCode, string? FromPath, string? TargetKind, Guid? TargetContentItemId, string? TargetPath, string? StatusCode);
