namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;

public sealed record EventRegistrationSummaryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? VerifiedAtUtc,
    DateTime? WaitlistedAtUtc,
    byte[] RowVersion);
