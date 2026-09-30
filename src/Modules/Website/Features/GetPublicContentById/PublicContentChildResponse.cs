namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

public sealed record PublicContentChildResponse(Guid Id, string Title, string Summary, string Path, PublicContentDetailImageResponse? CoverImage);
