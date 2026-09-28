namespace GenclikMerkezi.Modules.Website.Features.PublishContentItem;

public sealed record PublishContentItemRequest(byte[] RowVersion, DateTime? PublishAtUtc, DateTime? UnpublishAtUtc);
