namespace GenclikMerkezi.Modules.Website.Features.CreateContentPreviewLink;

public sealed record CreateContentPreviewLinkResponse(string Token, string PreviewPath, DateTime ExpiresAtUtc);
