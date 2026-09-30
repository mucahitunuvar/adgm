namespace GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;

public sealed record MenuResponse(Guid Id, string Location, byte[] RowVersion, IReadOnlyList<MenuItemResponse> Items);
