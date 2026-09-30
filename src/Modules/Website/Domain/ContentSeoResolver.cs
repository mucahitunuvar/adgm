namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §17 (Faz 1b Görev 7): the single SEO resolution chain the public list SEO, public detail
// SEO and preview link response all share ("kural tek bir yerde yazılır"):
//   metaTitle          -> title
//   metaDescription    -> summary (word-boundary-truncated to 160 chars) -> site default description
//   ogTitle            -> resolved metaTitle
//   ogDescription      -> resolved metaDescription
//   ogImage            -> detail image -> cover image -> site default OG image (caller resolves the id to a URL)
//   canonicalUrl       -> the content item's own path (relative, caller supplies it)
//   noIndex            -> as stored, unchanged
public static class ContentSeoResolver
{
    private const int MaxMetaDescriptionLength = SeoMetadata.MaxMetaDescriptionLength;

    public static ResolvedContentSeo Resolve(
        SeoMetadata seo,
        string title,
        string summary,
        string canonicalPath,
        Guid? detailImageMediaId,
        Guid? coverImageMediaId,
        Guid? siteDefaultOgImageMediaId,
        string siteDefaultMetaDescription)
    {
        var metaTitle = seo.MetaTitle.Length > 0 ? seo.MetaTitle : title;
        var metaDescription = seo.MetaDescription.Length > 0
            ? seo.MetaDescription
            : (summary.Length > 0 ? TruncateAtWordBoundary(summary, MaxMetaDescriptionLength) : siteDefaultMetaDescription);
        var ogTitle = seo.OgTitle.Length > 0 ? seo.OgTitle : metaTitle;
        var ogDescription = seo.OgDescription.Length > 0 ? seo.OgDescription : metaDescription;
        var ogImageMediaId = seo.OgImageMediaId ?? detailImageMediaId ?? coverImageMediaId ?? siteDefaultOgImageMediaId;

        return new ResolvedContentSeo(metaTitle, metaDescription, ogTitle, ogDescription, ogImageMediaId, canonicalPath, seo.NoIndex);
    }

    private static string TruncateAtWordBoundary(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        var truncated = text[..maxLength];
        var lastSpace = truncated.LastIndexOf(' ');
        return lastSpace > 0 ? truncated[..lastSpace] : truncated;
    }
}
