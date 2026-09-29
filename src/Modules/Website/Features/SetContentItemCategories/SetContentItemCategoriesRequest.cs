namespace GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;

public sealed record SetContentItemCategoriesRequest(byte[] RowVersion, IReadOnlyList<Guid> CategoryIds);
