namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoryById;

public sealed record ContentCategoryTranslationResponse(string LanguageCode, string Name, string Slug, ContentCategorySeoResponse Seo);
