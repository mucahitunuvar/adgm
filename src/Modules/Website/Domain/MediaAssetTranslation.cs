using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §6: per-language alt text/caption for a MediaAsset. Unlike ContentItem (Faz 1), the
// default language's AltText is NOT required here - it may be unknown at upload time - but empty
// alt text should be filterable in the admin listing (see GetMediaAssets's missingAltText filter).
public sealed class MediaAssetTranslation : Entity
{
    public LanguageCode LanguageCode { get; private set; } = null!;

    public string AltText { get; private set; } = string.Empty;

    public string Caption { get; private set; } = string.Empty;

    private MediaAssetTranslation(Guid id, LanguageCode languageCode, string altText, string caption)
        : base(id)
    {
        LanguageCode = languageCode;
        AltText = altText;
        Caption = caption;
    }

    private MediaAssetTranslation()
    {
    }

    public static MediaAssetTranslation Create(LanguageCode languageCode, string? altText, string? caption) =>
        new(Guid.NewGuid(), languageCode, (altText ?? string.Empty).Trim(), (caption ?? string.Empty).Trim());

    internal void Update(string? altText, string? caption)
    {
        AltText = (altText ?? string.Empty).Trim();
        Caption = (caption ?? string.Empty).Trim();
    }
}
