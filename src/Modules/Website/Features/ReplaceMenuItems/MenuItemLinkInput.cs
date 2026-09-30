namespace GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;

public sealed record MenuItemLinkInput(string Kind, Guid? ContentItemId, Guid? ContentTypeId, string? InternalPath, string? ExternalUrl);
