namespace GenclikMerkezi.Modules.Website.Features.GetNotFoundPaths;

public sealed record NotFoundLogResponse(Guid Id, string LanguageCode, string Path, int HitCount, DateTime FirstSeenAtUtc, DateTime LastSeenAtUtc);
