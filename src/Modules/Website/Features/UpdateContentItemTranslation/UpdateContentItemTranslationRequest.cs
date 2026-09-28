namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;

public sealed record UpdateContentItemTranslationRequest(
    byte[] RowVersion, string? Title, string? Slug, string? Summary, string? Body, UpdateContentItemTranslationSeoInput Seo);
