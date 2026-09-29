namespace GenclikMerkezi.Modules.Website.Features.GetContentItemById;

public sealed record ContentItemTranslationResponse(
    string LanguageCode, string Title, string Slug, string FullPath, string Summary, string Body, ContentItemSeoResponse Seo,
    IReadOnlyList<Guid> TagIds);
