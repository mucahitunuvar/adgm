namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItem;

public sealed record UpdateContentItemRequest(byte[] RowVersion, int SortOrder, bool IsFeatured, Guid? CoverImageMediaId, Guid? DetailImageMediaId);
