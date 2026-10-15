namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoryById;

public sealed record ContentCategoryDetailResponse(
    Guid Id,
    Guid ContentTypeId,
    Guid? ParentId,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion,
    IReadOnlyList<ContentCategoryTranslationResponse> Translations);
