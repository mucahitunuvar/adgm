namespace GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;

public sealed record ConvertNotFoundPathToRedirectRequest(string? TargetKind, Guid? TargetContentItemId, string? TargetPath, string? StatusCode);
