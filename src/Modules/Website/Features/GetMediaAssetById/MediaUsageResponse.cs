namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;

public sealed record MediaUsageResponse(string SourceKey, Guid SourceId, string Description, string? Url);
