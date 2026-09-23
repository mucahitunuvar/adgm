using GenclikMerkezi.Modules.Website.Infrastructure.Media;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Website.TestSupport;
using SkiaSharp;

namespace GenclikMerkezi.UnitTests.Website.Infrastructure.Media;

public class SkiaSharpImageProcessorTests
{
    private readonly SkiaSharpImageProcessor _processor = new();

    [Fact]
    public void Process_WithLargeLandscapeImage_GeneratesAllThreeAspectPreservingVariants()
    {
        var bytes = JpegTestImageBuilder.CreateJpegWithMarker(2000, 1000);

        var result = _processor.Process(bytes);

        Assert.True(result.IsSuccess);
        Assert.Equal(2000, result.Value.Width);
        Assert.Equal(1000, result.Value.Height);
        Assert.Equal(3, result.Value.Variants.Count);

        var small = Assert.Single(result.Value.Variants, v => v.VariantName == "small");
        Assert.Equal(400, small.Width);
        Assert.Equal(200, small.Height);

        var medium = Assert.Single(result.Value.Variants, v => v.VariantName == "medium");
        Assert.Equal(800, medium.Width);
        Assert.Equal(400, medium.Height);

        var large = Assert.Single(result.Value.Variants, v => v.VariantName == "large");
        Assert.Equal(1600, large.Width);
        Assert.Equal(800, large.Height);
    }

    [Fact]
    public void Process_WithPortraitImage_PreservesAspectRatioAndSkipsWiderVariant()
    {
        var bytes = JpegTestImageBuilder.CreateJpegWithMarker(1000, 3000);

        var result = _processor.Process(bytes);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Variants.Count);

        var small = Assert.Single(result.Value.Variants, v => v.VariantName == "small");
        Assert.Equal(400, small.Width);
        Assert.Equal(1200, small.Height);

        var medium = Assert.Single(result.Value.Variants, v => v.VariantName == "medium");
        Assert.Equal(800, medium.Width);
        Assert.Equal(2400, medium.Height);

        Assert.DoesNotContain(result.Value.Variants, v => v.VariantName == "large");
    }

    [Fact]
    public void Process_WithImageSmallerThanEveryVariant_GeneratesNoVariants_DoesNotUpscale()
    {
        var bytes = JpegTestImageBuilder.CreateJpegWithMarker(100, 50);

        var result = _processor.Process(bytes);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Width);
        Assert.Equal(50, result.Value.Height);
        Assert.Empty(result.Value.Variants);
    }

    [Fact]
    public void Process_WithExifOrientation6_RotatesImage90Clockwise_AndSwapsDimensions()
    {
        // Orientation 6 = rotate 90 CW. Source is 210x90 with a red marker in its top-left corner;
        // after correction the image must be 90x210, and that marker must have moved to the
        // corrected image's top-right corner (rotating a page 90° clockwise swings its top-left
        // corner around to become the top-right corner).
        var bytes = JpegTestImageBuilder.CreateJpegWithMarker(210, 90, exifOrientation: 6);

        var result = _processor.Process(bytes);

        Assert.True(result.IsSuccess);
        Assert.Equal(90, result.Value.Width);
        Assert.Equal(210, result.Value.Height);

        using var correctedBitmap = SKBitmap.Decode(result.Value.OriginalBytes);
        var topRightPixel = correctedBitmap.GetPixel(correctedBitmap.Width - 5, 5);
        var topLeftPixel = correctedBitmap.GetPixel(5, 5);

        Assert.True(topRightPixel.Red > topRightPixel.Blue, "Expected the marker (red) to have moved to the top-right corner.");
        Assert.True(topLeftPixel.Blue > topLeftPixel.Red, "Expected the top-left corner to be background (blue) after correction.");
    }

    [Fact]
    public void Process_WithoutExifOrientation_LeavesImageUnrotated()
    {
        var bytes = JpegTestImageBuilder.CreateJpegWithMarker(210, 90);

        var result = _processor.Process(bytes);

        Assert.True(result.IsSuccess);
        Assert.Equal(210, result.Value.Width);
        Assert.Equal(90, result.Value.Height);

        using var bitmap = SKBitmap.Decode(result.Value.OriginalBytes);
        var topLeftPixel = bitmap.GetPixel(5, 5);
        Assert.True(topLeftPixel.Red > topLeftPixel.Blue, "Expected the marker (red) to remain in the top-left corner.");
    }

    [Fact]
    public void Process_WithUndecodableBytes_ReturnsFailure()
    {
        var result = _processor.Process([1, 2, 3, 4, 5]);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.CannotDecodeImage", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Process_WithImageExceedingPixelLimit_ReturnsFailure()
    {
        // 8000x6300 = 50,400,000 pixels, just over the 50-million-pixel decompression-bomb guard.
        // A solid-color image compresses (and therefore encodes) near-instantly despite the large
        // declared dimensions.
        var bytes = JpegTestImageBuilder.CreateJpegWithMarker(8000, 6300);

        var result = _processor.Process(bytes);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.ImageTooLarge", result.Error.Code);
    }
}
