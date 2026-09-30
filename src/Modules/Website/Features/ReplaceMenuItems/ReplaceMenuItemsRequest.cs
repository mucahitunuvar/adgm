namespace GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;

public sealed record ReplaceMenuItemsRequest(byte[] RowVersion, IReadOnlyList<MenuItemTreeInput> Items);
