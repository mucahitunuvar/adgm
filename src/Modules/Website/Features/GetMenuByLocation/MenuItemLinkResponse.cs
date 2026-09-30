namespace GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;

public sealed record MenuItemLinkResponse(string Kind, Guid? ContentItemId, Guid? ContentTypeId, string? InternalPath, string? ExternalUrl);
