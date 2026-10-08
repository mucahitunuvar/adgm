namespace GenclikMerkezi.Modules.Website.Application.ContentRevisions;

// ADR-024 §4 (Faz 5 Görev 7): one language's captured fields - exactly the subset the master prompt
// lists ("her çeviri için Title, Summary, Body, SEO alanları: MetaTitle, MetaDescription, OgTitle,
// OgDescription, NoIndex, CanonicalUrl; TagIds"). MetaKeywords and OgImageMediaId are deliberately
// excluded (not part of that list) and are therefore never touched by a restore either.
public sealed record ContentItemRevisionSnapshotTranslation(
    string LanguageCode,
    string Title,
    string Summary,
    string Body,
    string MetaTitle,
    string MetaDescription,
    string OgTitle,
    string OgDescription,
    bool NoIndex,
    string? CanonicalUrl,
    IReadOnlyList<Guid> TagIds);
