namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidateCvs;

public sealed record CandidateCvListItemResponse(
    Guid Id, Guid UserId, string FirstName, string LastName, string Email, int CompletionPercentage);
