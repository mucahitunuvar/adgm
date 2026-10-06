using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Faz 4 Görev 4: GetEventRegistrations' own list-row projection - deliberately not the full
// EventRegistration aggregate (whose StatusHistory is an EF owned collection, always loaded with the
// owner otherwise) since the admin list never needs the history, only the summary fields.
public sealed record EventRegistrationListItem(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    EventRegistrationStatus Status,
    DateTime CreatedAtUtc,
    DateTime? VerifiedAtUtc,
    DateTime? WaitlistedAtUtc,
    byte[] RowVersion);
