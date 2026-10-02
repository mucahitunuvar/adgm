namespace GenclikMerkezi.Modules.Website.Features.GetSliderById;

public sealed record SlideLinkResponse(string Kind, Guid? ContentItemId, Guid? ContentTypeId, string? InternalPath, string? ExternalUrl);
