namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateReference;

public sealed record UpdateCandidateReferenceRequest(
    Guid ReferenceTypeId,
    Guid ReferenceLanguageId,
    string FirstName,
    string LastName,
    string? Company,
    string? Position,
    string? Email,
    string? PhoneNumber);
