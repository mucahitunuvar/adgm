namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;

public sealed record UpdateContentCategoryTranslationRequest(
    byte[] RowVersion, string? Name, string? Slug, UpdateContentCategoryTranslationSeoInput Seo);
