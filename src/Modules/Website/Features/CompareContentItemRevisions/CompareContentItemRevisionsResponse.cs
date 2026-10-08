namespace GenclikMerkezi.Modules.Website.Features.CompareContentItemRevisions;

public sealed record CompareContentItemRevisionsResponse(
    string LanguageCode,
    ContentItemRevisionFieldDiffResponse<string> Title,
    ContentItemRevisionFieldDiffResponse<string> Summary,
    ContentItemRevisionFieldDiffResponse<string> Body,
    ContentItemRevisionFieldDiffResponse<string> MetaTitle,
    ContentItemRevisionFieldDiffResponse<string> MetaDescription,
    ContentItemRevisionFieldDiffResponse<string> OgTitle,
    ContentItemRevisionFieldDiffResponse<string> OgDescription,
    ContentItemRevisionFieldDiffResponse<bool> NoIndex,
    ContentItemRevisionFieldDiffResponse<string?> CanonicalUrl,
    ContentItemRevisionFieldDiffResponse<IReadOnlyList<Guid>> TagIds,
    ContentItemRevisionFieldDiffResponse<IReadOnlyList<Guid>> CategoryIds);
