using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §11.2 (Faz 4 Görev 5): the participant CSV export's row shape - exactly the eight columns the
// master prompt specifies ("firstName,lastName,email,phone,status,registeredAtUtc,confirmedAtUtc,
// language"), deliberately no referenceNo (events have none, unlike FormSubmission).
public sealed record EventRegistrationExportItem(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    EventRegistrationStatus Status,
    DateTime RegisteredAtUtc,
    DateTime? ConfirmedAtUtc,
    string LanguageCode);
