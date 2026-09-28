namespace GenclikMerkezi.Modules.Website.Features.GetContentTypeById;

public sealed record ContentTypeTranslationResponse(string LanguageCode, string Name, string RoutePrefix, ContentTypeSeoResponse Seo);
