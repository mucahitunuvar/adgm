namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

public sealed record ContentPreviewSeoResponse(
    string MetaTitle, string MetaDescription, string OgTitle, string OgDescription, string? OgImageUrl, string CanonicalUrl, bool NoIndex);
