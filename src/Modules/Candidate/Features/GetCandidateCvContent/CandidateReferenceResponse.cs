namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed record CandidateReferenceResponse(
    Guid Id,
    Guid ReferenceTypeId,
    Guid ReferenceLanguageId,
    string FirstName,
    string LastName,
    string? Company,
    string? Position,
    string? Email,
    string? PhoneNumber);
