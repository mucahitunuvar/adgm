namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrationById;

public sealed record EventRegistrationStatusHistoryEntryResponse(
    string? PreviousStatus, string NewStatus, string ChangedBy, DateTime OccurredAtUtc);
