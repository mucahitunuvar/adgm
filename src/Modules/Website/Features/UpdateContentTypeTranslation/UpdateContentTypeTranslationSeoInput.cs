namespace GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

public sealed record UpdateContentTypeTranslationSeoInput(
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? OgTitle,
    string? OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
