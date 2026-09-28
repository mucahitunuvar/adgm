namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;

public sealed record UpdateContentItemTranslationSeoInput(
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? OgTitle,
    string? OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
