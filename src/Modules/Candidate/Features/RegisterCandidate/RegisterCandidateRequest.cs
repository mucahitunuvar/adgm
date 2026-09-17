namespace GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;

public sealed record RegisterCandidateRequest(string Email, string Password, string FirstName, string LastName, string? PhoneNumber);
