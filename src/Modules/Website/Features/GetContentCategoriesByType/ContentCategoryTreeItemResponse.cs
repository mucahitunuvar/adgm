namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoriesByType;

public sealed record ContentCategoryTreeItemResponse(
    Guid Id,
    string Name,
    string Slug,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion,
    int AssignedContentCount,
    IReadOnlyList<ContentCategoryTreeItemResponse> Children);
