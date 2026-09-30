namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

public sealed record PublicContentDetailSeoResponse(
    string MetaTitle, string MetaDescription, string OgTitle, string OgDescription, string? OgImageUrl, string CanonicalUrl, bool NoIndex);
