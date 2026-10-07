namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §10 (Faz 5 Görev 1): one page item returned by IExternalSearchSource. SearchableText is
// the source's own concatenation of its searchable fields (e.g. Employer's title + company + sector
// names) - the external source, not Website, decides what belongs in it, since Website has no
// visibility into the source module's own fields (ADR-016 module independence).
public sealed record ExternalSearchDocument(
    string SourceId,
    string TypeKey,
    string Title,
    string Summary,
    string Url,
    string SearchableText,
    DateTime PublishedAtUtc,
    bool IncludeInSitemap);
