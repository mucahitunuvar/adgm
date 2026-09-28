namespace GenclikMerkezi.Modules.Website.Features.CreateContentType;

public sealed record CreateContentTypeSeoInput(
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? OgTitle,
    string? OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
