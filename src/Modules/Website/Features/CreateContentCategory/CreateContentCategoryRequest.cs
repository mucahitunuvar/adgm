namespace GenclikMerkezi.Modules.Website.Features.CreateContentCategory;

public sealed record CreateContentCategoryRequest(
    Guid? ParentId, int SortOrder, string? DefaultLanguageName, string? DefaultLanguageSlug, CreateContentCategorySeoInput Seo);
