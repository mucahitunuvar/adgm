namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoryById;

public sealed record ContentCategorySeoResponse(
    string MetaTitle,
    string MetaDescription,
    string MetaKeywords,
    string OgTitle,
    string OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
