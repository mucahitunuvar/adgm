namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;

public sealed record UpdateContentCategoryTranslationSeoInput(
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? OgTitle,
    string? OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
