namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Result of IImageProcessor.Process: the orientation-corrected, metadata-stripped re-encode of the
// original (same raster format, not resized) plus every variant smaller than the original's width
// (ADR-024 §6: no upscaling - a variant is simply omitted if the original is already smaller).
public sealed record ProcessedImage(
    byte[] OriginalBytes, int Width, int Height, IReadOnlyList<ProcessedImageVariant> Variants);
