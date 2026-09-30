namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §17 (Faz 1b Görev 7): the fully resolved SEO fields a public response sends - no field is
// ever empty when a title/summary exists, since ContentSeoResolver.Resolve has already applied the
// full fallback chain.
public sealed record ResolvedContentSeo(
    string MetaTitle, string MetaDescription, string OgTitle, string OgDescription, Guid? OgImageMediaId, string CanonicalUrl, bool NoIndex);
