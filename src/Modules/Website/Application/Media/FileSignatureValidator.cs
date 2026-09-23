namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 Faz 0 Görev 5: real-content checks that go beyond extension/content-type, so a renamed
// file cannot pass itself off as a document type it is not. Images have no equivalent helper here -
// IImageProcessor's own decode failure already serves as that check.
public static class FileSignatureValidator
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];

    public static bool HasPdfSignature(byte[] bytes) => StartsWith(bytes, PdfSignature);

    // DOCX and XLSX are both OOXML ZIP containers - this cannot distinguish between them, only
    // confirm the file is genuinely a ZIP archive (which extension/content-type already declared).
    public static bool HasZipSignature(byte[] bytes) => StartsWith(bytes, ZipSignature);

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
