namespace GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

public sealed record UpdateContentTypeTranslationRequest(
    byte[] RowVersion,
    string? Name,
    string? RoutePrefix,
    UpdateContentTypeTranslationSeoInput Seo);
