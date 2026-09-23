using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using SkiaSharp;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Media;

// ADR-024 §6 / Faz 0 Görev 5. Pipeline: decode header only (cheap - guards against decompression
// bombs before any full pixel decode) -> full decode -> apply EXIF orientation (SKCodec reports it
// via EncodedOrigin; SKBitmap.Decode itself does NOT apply it) -> re-encode the oriented bitmap as
// the new "original" in its own source format (metadata is dropped for free: a freshly rendered
// SKBitmap carries none of the source file's EXIF/ICC/XMP - we are not touching the original bytes
// at all) -> generate WebP variants, skipping any variant wider than the (possibly already-rotated)
// image, since ADR-024 §6 forbids upscaling.
public sealed class SkiaSharpImageProcessor : IImageProcessor
{
    private const long MaxPixelCount = 50_000_000;
    private const int OriginalJpegQuality = 90;
    private const int OriginalWebPQuality = 90;
    private const int VariantWebPQuality = 82;

    private static readonly (string Name, int Width)[] VariantSpecs =
    [
        (MediaAssetVariantNames.Small, 400),
        (MediaAssetVariantNames.Medium, 800),
        (MediaAssetVariantNames.Large, 1600),
    ];

    public Result<ProcessedImage> Process(byte[] originalBytes)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(originalBytes));
        if (codec is null)
        {
            return Result.Failure<ProcessedImage>(Error.Validation(
                "MediaAsset.CannotDecodeImage", "The uploaded file could not be decoded as an image."));
        }

        var pixelCount = (long)codec.Info.Width * codec.Info.Height;
        if (pixelCount > MaxPixelCount)
        {
            return Result.Failure<ProcessedImage>(Error.Validation(
                "MediaAsset.ImageTooLarge",
                $"The image has {pixelCount:N0} pixels, exceeding the {MaxPixelCount:N0} pixel limit."));
        }

        using var decoded = SKBitmap.Decode(codec);
        if (decoded is null)
        {
            return Result.Failure<ProcessedImage>(Error.Validation(
                "MediaAsset.CannotDecodeImage", "The uploaded file could not be decoded as an image."));
        }

        using var oriented = ApplyExifOrientation(decoded, codec.EncodedOrigin);

        var originalFormat = codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Png => SKEncodedImageFormat.Png,
            SKEncodedImageFormat.Webp => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Jpeg,
        };
        var originalQuality = originalFormat == SKEncodedImageFormat.Webp ? OriginalWebPQuality : OriginalJpegQuality;

        var reEncodedOriginal = Encode(oriented, originalFormat, originalQuality);

        var variants = new List<ProcessedImageVariant>();
        foreach (var (name, targetWidth) in VariantSpecs)
        {
            if (oriented.Width <= targetWidth)
            {
                continue;
            }

            var targetHeight = (int)Math.Round(oriented.Height * (targetWidth / (double)oriented.Width));
            using var resized = oriented.Resize(
                new SKImageInfo(targetWidth, targetHeight, oriented.ColorType, oriented.AlphaType),
                new SKSamplingOptions(SKCubicResampler.Mitchell));

            if (resized is null)
            {
                continue;
            }

            var variantBytes = Encode(resized, SKEncodedImageFormat.Webp, VariantWebPQuality);
            variants.Add(new ProcessedImageVariant(name, variantBytes, targetWidth, targetHeight));
        }

        return Result.Success(new ProcessedImage(reEncodedOriginal, oriented.Width, oriented.Height, variants));
    }

    private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, quality);
        return data.ToArray();
    }

    // EXIF Orientation 1-8 (SKCodec.EncodedOrigin mirrors the TIFF tag values exactly). SKBitmap.Decode
    // never applies this itself - every caller of SKCodec-based decoding is expected to handle it.
    private static SKBitmap ApplyExifOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft)
        {
            return source.Copy();
        }

        var swapped = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

        var destWidth = swapped ? source.Height : source.Width;
        var destHeight = swapped ? source.Width : source.Height;

        var destination = new SKBitmap(destWidth, destHeight, source.ColorType, source.AlphaType);
        using var canvas = new SKCanvas(destination);

        switch (origin)
        {
            case SKEncodedOrigin.TopRight: // mirror horizontal
                canvas.Translate(source.Width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight: // rotate 180
                canvas.Translate(destWidth, destHeight);
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.BottomLeft: // mirror vertical
                canvas.Translate(0, source.Height);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop: // mirror horizontal + rotate 270 CW
                canvas.Translate(0, destHeight);
                canvas.RotateDegrees(270);
                canvas.Translate(source.Width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.RightTop: // rotate 90 CW
                canvas.Translate(destWidth, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom: // mirror horizontal + rotate 90 CW
                canvas.Translate(destWidth, 0);
                canvas.RotateDegrees(90);
                canvas.Translate(source.Width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.LeftBottom: // rotate 270 CW
                canvas.Translate(0, destHeight);
                canvas.RotateDegrees(270);
                break;
        }

        canvas.DrawBitmap(source, 0, 0, SKSamplingOptions.Default);
        return destination;
    }
}
