namespace GenclikMerkezi.Modules.Website.Features.GetPublicContents;

public sealed record PublicContentSeoResponse(
    string MetaTitle, string MetaDescription, string OgTitle, string OgDescription, string? OgImageUrl, string CanonicalUrl, bool NoIndex);
