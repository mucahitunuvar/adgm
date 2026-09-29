namespace GenclikMerkezi.Modules.Website.Features.CreateContentCategory;

public sealed record CreateContentCategorySeoInput(
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? OgTitle,
    string? OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
