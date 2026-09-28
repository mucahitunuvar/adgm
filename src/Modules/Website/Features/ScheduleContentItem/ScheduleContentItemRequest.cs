namespace GenclikMerkezi.Modules.Website.Features.ScheduleContentItem;

public sealed record ScheduleContentItemRequest(byte[] RowVersion, DateTime? PublishAtUtc, DateTime? UnpublishAtUtc);
