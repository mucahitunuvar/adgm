namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisionByNumber;

public sealed record ContentItemRevisionTranslationSnapshotResponse(
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
