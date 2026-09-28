namespace GenclikMerkezi.Modules.Website.Features.GetContentTypeById;

public sealed record ContentTypeSeoResponse(
    string MetaTitle,
    string MetaDescription,
    string MetaKeywords,
    string OgTitle,
    string OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
