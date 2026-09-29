using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1b Görev 4): per-language override for an attachment's display name. Public
// resolution (override -> original file name) lives in the Application layer's public response
// builder, not here.
public sealed class ContentItemAttachmentTranslation : Entity
{
    public const int MaxDisplayNameLength = 200;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string? DisplayNameOverride { get; private set; }

    private ContentItemAttachmentTranslation(Guid id, LanguageCode languageCode, string? displayNameOverride)
        : base(id)
    {
        LanguageCode = languageCode;
        DisplayNameOverride = displayNameOverride;
    }

    private ContentItemAttachmentTranslation()
    {
    }

    public static Result<ContentItemAttachmentTranslation> Create(LanguageCode languageCode, string? displayNameOverride)
    {
        var normalized = NormalizeOptional(displayNameOverride);
        if (normalized is not null && normalized.Length > MaxDisplayNameLength)
        {
            return Result.Failure<ContentItemAttachmentTranslation>(Error.Validation(
                "ContentItemAttachment.DisplayNameOverrideTooLong",
                $"Display name override must be at most {MaxDisplayNameLength} characters."));
        }

        return Result.Success(new ContentItemAttachmentTranslation(Guid.NewGuid(), languageCode, normalized));
    }

    private static string? NormalizeOptional(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
