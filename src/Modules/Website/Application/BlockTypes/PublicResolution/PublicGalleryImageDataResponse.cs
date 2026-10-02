namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: gallery's per-image `data` - alt text resolved from the media
// library (Faz 1b's gallery alt-text chain has no per-use override here, unlike content gallery items).
public sealed record PublicGalleryImageDataResponse(Guid Id, PublicBlockImageResponse Image, string AltText);
