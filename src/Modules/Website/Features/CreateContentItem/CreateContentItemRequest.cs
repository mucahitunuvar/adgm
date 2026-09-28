namespace GenclikMerkezi.Modules.Website.Features.CreateContentItem;

public sealed record CreateContentItemRequest(
    Guid ContentTypeId,
    Guid? ParentId,
    int SortOrder,
    bool IsFeatured,
    Guid? CoverImageMediaId,
    Guid? DetailImageMediaId,
    string? DefaultLanguageTitle,
    string? DefaultLanguageSlug,
    string? DefaultLanguageSummary,
    string? DefaultLanguageBody,
    CreateContentItemSeoInput Seo);
