namespace GenclikMerkezi.Modules.Website.Features.GetContentItems;

public sealed record ContentItemSummaryResponse(
    Guid Id,
    Guid ContentTypeId,
    Guid? ParentId,
    string Title,
    string Status,
    int SortOrder,
    bool IsFeatured,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    byte[] RowVersion);
