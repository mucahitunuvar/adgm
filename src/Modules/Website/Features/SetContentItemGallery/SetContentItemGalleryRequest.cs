namespace GenclikMerkezi.Modules.Website.Features.SetContentItemGallery;

public sealed record SetContentItemGalleryRequest(byte[] RowVersion, IReadOnlyList<GalleryItemInput> Items);
