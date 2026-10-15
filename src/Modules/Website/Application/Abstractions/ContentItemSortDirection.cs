namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Paired with ContentItemSortField - ignored by IContentItemRepository.SearchAsync when no sort
// field is given (the admin list's default order, SortOrder then Id, does not change).
public enum ContentItemSortDirection
{
    Asc,
    Desc,
}
