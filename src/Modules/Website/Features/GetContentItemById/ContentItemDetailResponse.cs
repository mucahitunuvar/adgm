namespace GenclikMerkezi.Modules.Website.Features.GetContentItemById;

public sealed record ContentItemDetailResponse(
    Guid Id,
    Guid ContentTypeId,
    Guid? ParentId,
    string Status,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    int SortOrder,
    bool IsFeatured,
    Guid? CoverImageMediaId,
    string? CoverImageUrl,
    Guid? DetailImageMediaId,
    string? DetailImageUrl,
    bool IsVisible,
    byte[] RowVersion,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<ContentItemTranslationResponse> Translations);
