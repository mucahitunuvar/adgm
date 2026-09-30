namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

public sealed record ContentPreviewGalleryItemResponse(Guid MediaAssetId, ContentPreviewImageResponse Image, string AltText, string Caption);
