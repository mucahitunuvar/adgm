namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public sealed record CandidateCvPdfReference(
    string ReferenceTypeName,
    string ReferenceLanguageName,
    string FirstName,
    string LastName,
    string? Company,
    string? Position,
    string? Email,
    string? PhoneNumber);
