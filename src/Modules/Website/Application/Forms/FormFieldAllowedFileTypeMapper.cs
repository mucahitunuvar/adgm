using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Forms;

// ADR-024 §12.2 (Faz 3 Görev 4): maps a FormField's admin-configured allowed file types to the
// extension/content-type/magic-byte checks SubmitFormSubmissionCommandHandler enforces per uploaded
// file - the same three-layer check (extension, content-type, signature) MediaAsset's own document
// upload uses (FileValidationPolicy + FileSignatureValidator), just keyed off FormFieldAllowedFileType
// instead of a fixed per-category policy, since which types are allowed varies per field.
public static class FormFieldAllowedFileTypeMapper
{
    public static string Extension(FormFieldAllowedFileType type) => type switch
    {
        FormFieldAllowedFileType.Pdf => "pdf",
        FormFieldAllowedFileType.Docx => "docx",
        FormFieldAllowedFileType.Jpg => "jpg",
        FormFieldAllowedFileType.Png => "png",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    public static string ContentType(FormFieldAllowedFileType type) => type switch
    {
        FormFieldAllowedFileType.Pdf => "application/pdf",
        FormFieldAllowedFileType.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        FormFieldAllowedFileType.Jpg => "image/jpeg",
        FormFieldAllowedFileType.Png => "image/png",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    // Jpg additionally accepts the ".jpeg" extension (same two-extensions-one-type shape
    // MediaValidationPolicies.Image already uses) - handled by the caller building the extension set,
    // not here, since this method answers "what IS this FormFieldAllowedFileType" rather than "what
    // extensions does it accept".
    public static bool HasValidSignature(FormFieldAllowedFileType type, byte[] bytes) => type switch
    {
        FormFieldAllowedFileType.Pdf => FileSignatureValidator.HasPdfSignature(bytes),
        FormFieldAllowedFileType.Docx => FileSignatureValidator.HasZipSignature(bytes),
        FormFieldAllowedFileType.Jpg => FileSignatureValidator.HasJpgSignature(bytes),
        FormFieldAllowedFileType.Png => FileSignatureValidator.HasPngSignature(bytes),
        _ => false,
    };
}
