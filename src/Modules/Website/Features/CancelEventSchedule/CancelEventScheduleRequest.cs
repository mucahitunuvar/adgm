namespace GenclikMerkezi.Modules.Website.Features.CancelEventSchedule;

public sealed record CancelEventScheduleRequest(byte[] RowVersion, string? Reason);
