namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// GetContentItemsQueryHandler parses the admin list's `orderBy` query parameter into this via
// EnumParsing.TryParseDefined (same pattern as ContentItemStatus) - the repository-level sort
// vocabulary IContentItemRepository.SearchAsync accepts. PublishAt sorts by the same "effective
// publish date" (PublishAtUtc ?? PublishedAtUtc) SearchPublicListAsync's from/to filter already uses,
// not the raw PublishAtUtc column - an immediately-published item (the common case) never has
// PublishAtUtc set, only PublishedAtUtc.
public enum ContentItemSortField
{
    Title,
    CreatedAt,
    UpdatedAt,
    PublishAt,
    SortOrder,
}
