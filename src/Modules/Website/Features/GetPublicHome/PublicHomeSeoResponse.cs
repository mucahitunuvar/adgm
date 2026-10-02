namespace GenclikMerkezi.Modules.Website.Features.GetPublicHome;

public sealed record PublicHomeSeoResponse(
    string MetaTitle, string MetaDescription, string OgTitle, string OgDescription, string? OgImageUrl, string CanonicalUrl, bool NoIndex);
