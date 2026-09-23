using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 Faz 0 Görev 5's fixed upload limits. SVG/GIF and legacy Office formats (doc/xls) are
// deliberately excluded (XSS risk and macro risk respectively - see the master prompt).
public static class MediaValidationPolicies
{
    public static readonly FileValidationPolicy Image = FileValidationPolicy.Create(
        ["jpg", "jpeg", "png", "webp"],
        ["image/jpeg", "image/png", "image/webp"],
        maxSizeInBytes: 10 * 1024 * 1024);

    public static readonly FileValidationPolicy Document = FileValidationPolicy.Create(
        ["pdf", "docx", "xlsx"],
        [
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ],
        maxSizeInBytes: 20 * 1024 * 1024);

    // The processor's own output (WebP variants, re-encoded original) - generous ceiling since this
    // is content Website itself just generated, not user input; the real limit already applied above.
    public static readonly FileValidationPolicy ProcessedImageOutput = FileValidationPolicy.Create(
        ["jpg", "jpeg", "png", "webp"],
        ["image/jpeg", "image/png", "image/webp"],
        maxSizeInBytes: 20 * 1024 * 1024);
}
