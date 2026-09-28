namespace GenclikMerkezi.Modules.Website.Features.GetContentItemById;

public sealed record ContentItemSeoResponse(
    string MetaTitle,
    string MetaDescription,
    string MetaKeywords,
    string OgTitle,
    string OgDescription,
    Guid? OgImageMediaId,
    string? CanonicalUrl,
    bool NoIndex);
