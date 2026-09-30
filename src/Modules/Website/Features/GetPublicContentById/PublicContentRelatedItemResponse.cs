namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

public sealed record PublicContentRelatedItemResponse(Guid Id, string Title, string Summary, string Path, string? CoverImageUrl);
