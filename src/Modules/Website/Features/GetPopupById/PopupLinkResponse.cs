namespace GenclikMerkezi.Modules.Website.Features.GetPopupById;

public sealed record PopupLinkResponse(string Kind, Guid? ContentItemId, Guid? ContentTypeId, string? InternalPath, string? ExternalUrl);
