using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1b Görev 4): per-language override for a gallery item's alt text/caption. Public
// resolution chain (ContentItemGalleryItem override -> MediaAsset's own translation -> empty string)
// lives in the Application layer's public response builder, not here.
public sealed class ContentItemGalleryItemTranslation : Entity
{
    public const int MaxAltTextLength = 500;
    public const int MaxCaptionLength = 1000;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string? AltTextOverride { get; private set; }

    public string? CaptionOverride { get; private set; }

    private ContentItemGalleryItemTranslation(Guid id, LanguageCode languageCode, string? altTextOverride, string? captionOverride)
        : base(id)
    {
        LanguageCode = languageCode;
        AltTextOverride = altTextOverride;
        CaptionOverride = captionOverride;
    }

    private ContentItemGalleryItemTranslation()
    {
    }

    public static Result<ContentItemGalleryItemTranslation> Create(LanguageCode languageCode, string? altTextOverride, string? captionOverride)
    {
        var normalizedAlt = NormalizeOptional(altTextOverride);
        if (normalizedAlt is not null && normalizedAlt.Length > MaxAltTextLength)
        {
            return Result.Failure<ContentItemGalleryItemTranslation>(Error.Validation(
                "ContentItemGalleryItem.AltTextOverrideTooLong", $"Alt text override must be at most {MaxAltTextLength} characters."));
        }

        var normalizedCaption = NormalizeOptional(captionOverride);
        if (normalizedCaption is not null && normalizedCaption.Length > MaxCaptionLength)
        {
            return Result.Failure<ContentItemGalleryItemTranslation>(Error.Validation(
                "ContentItemGalleryItem.CaptionOverrideTooLong", $"Caption override must be at most {MaxCaptionLength} characters."));
        }

        return Result.Success(new ContentItemGalleryItemTranslation(Guid.NewGuid(), languageCode, normalizedAlt, normalizedCaption));
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
