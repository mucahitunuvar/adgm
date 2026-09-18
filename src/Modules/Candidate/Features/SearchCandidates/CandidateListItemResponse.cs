namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidates;

public sealed record CandidateListItemResponse(
    Guid CandidateCvId,
    string FullName,
    string Email,
    string? ProvinceName,
    string? DistrictName,
    int CompletionPercentage);
