using SkiaSharp;

namespace GenclikMerkezi.UnitTests.Website.TestSupport;

// Builds small synthetic JPEGs for SkiaSharpImageProcessorTests, including ones with a real EXIF
// Orientation tag - SkiaSharp's own encoder has no public API to write custom EXIF tags, so an
// orientation tag is injected by splicing a hand-built minimal APP1/EXIF segment (TIFF header + one
// IFD0 entry) right after the JPEG's SOI marker. This is a real, well-formed EXIF segment - not a
// mock of one - so it exercises the same SKCodec.EncodedOrigin parsing a real camera-orientated
// photo would.
internal static class JpegTestImageBuilder
{
    // Produces a plain-colored bitmap with a distinct marker square in its top-left corner, so a
    // test can verify which corner that marker ends up in after orientation correction.
    public static byte[] CreateJpegWithMarker(int width, int height, int? exifOrientation = null)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Blue);
            var markerSize = Math.Min(width, height) / 3;
            using var paint = new SKPaint { Color = SKColors.Red };
            canvas.DrawRect(SKRect.Create(0, 0, markerSize, markerSize), paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        var jpegBytes = data.ToArray();

        return exifOrientation is null ? jpegBytes : InjectExifOrientation(jpegBytes, exifOrientation.Value);
    }

    private static byte[] InjectExifOrientation(byte[] jpegBytes, int orientation)
    {
        var app1Segment = BuildExifOrientationApp1Segment(orientation);

        var result = new byte[2 + app1Segment.Length + (jpegBytes.Length - 2)];
        result[0] = jpegBytes[0];
        result[1] = jpegBytes[1];
        Array.Copy(app1Segment, 0, result, 2, app1Segment.Length);
        Array.Copy(jpegBytes, 2, result, 2 + app1Segment.Length, jpegBytes.Length - 2);
        return result;
    }

    private static byte[] BuildExifOrientationApp1Segment(int orientation)
    {
        // TIFF header (little-endian) + IFD0 with a single Orientation (tag 0x0112, type SHORT) entry.
        byte[] tiffAndIfd =
        [
            0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00, // "II", magic 42, IFD0 offset = 8
            0x01, 0x00, // 1 entry
            0x12, 0x01, // Tag 0x0112 = Orientation
            0x03, 0x00, // Type 3 = SHORT
            0x01, 0x00, 0x00, 0x00, // Count = 1
            (byte)orientation, 0x00, 0x00, 0x00, // Value (SHORT, left-justified) + padding
            0x00, 0x00, 0x00, 0x00, // Next IFD offset = 0 (none)
        ];

        var payloadLength = 6 + tiffAndIfd.Length; // "Exif\0\0" + TIFF/IFD
        var lengthField = (ushort)(payloadLength + 2); // JPEG segment length includes itself

        var segment = new List<byte>
        {
            0xFF, 0xE1,
            (byte)(lengthField >> 8), (byte)(lengthField & 0xFF),
            0x45, 0x78, 0x69, 0x66, 0x00, 0x00, // "Exif\0\0"
        };
        segment.AddRange(tiffAndIfd);

        return segment.ToArray();
    }
}
