namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §17 (Faz 1a Görev 3): the admin list query's projection shape - only the requested (or
// default) language's title is carried, never the full translation collection, so the list endpoint
// never loads every language's Title/Slug/Body/Seo for every row on a page.
public sealed record ContentItemListItem(
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
