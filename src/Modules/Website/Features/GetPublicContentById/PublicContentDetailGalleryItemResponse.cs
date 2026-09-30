namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

public sealed record PublicContentDetailGalleryItemResponse(Guid MediaAssetId, PublicContentDetailImageResponse Image, string AltText, string Caption);
