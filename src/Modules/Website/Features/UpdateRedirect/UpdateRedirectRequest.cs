namespace GenclikMerkezi.Modules.Website.Features.UpdateRedirect;

public sealed record UpdateRedirectRequest(string? TargetKind, Guid? TargetContentItemId, string? TargetPath, string? StatusCode);
