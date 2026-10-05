namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 Faz 0 Görev 5 / Faz 3 Görev 4: real-content checks that go beyond extension/content-type, so
// a renamed file cannot pass itself off as a document (or, since Faz 3 Görev 4, image) type it is not.
// MediaAsset's own image uploads still have no equivalent helper here - IImageProcessor's own decode
// failure already serves as that check there - but WebsiteFormAttachment uploads are never run through
// IImageProcessor, so Jpg/Png need a real signature check of their own.
public static class FileSignatureValidator
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] JpgSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static bool HasPdfSignature(byte[] bytes) => StartsWith(bytes, PdfSignature);

    // DOCX and XLSX are both OOXML ZIP containers - this cannot distinguish between them, only
    // confirm the file is genuinely a ZIP archive (which extension/content-type already declared).
    public static bool HasZipSignature(byte[] bytes) => StartsWith(bytes, ZipSignature);

    public static bool HasJpgSignature(byte[] bytes) => StartsWith(bytes, JpgSignature);

    public static bool HasPngSignature(byte[] bytes) => StartsWith(bytes, PngSignature);

    private static bool StartsWith(byte[] bytes, byte[] signature)
    {
        if (bytes.Length < signature.Length)
        {
            return false;
        }

        for (var i = 0; i < signature.Length; i++)
        {
            if (bytes[i] != signature[i])
            {
                return false;
            }
        }

        return true;
    }
}
