namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateReference;

public sealed record AddCandidateReferenceRequest(
    Guid ReferenceTypeId,
    Guid ReferenceLanguageId,
    string FirstName,
    string LastName,
    string? Company,
    string? Position,
    string? Email,
    string? PhoneNumber);
