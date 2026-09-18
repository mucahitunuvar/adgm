namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

// Repository-level projection: ProvinceId/DistrictId stay as ids here - the feature handler (not the
// repository) resolves their display names via IReferenceDataLookupReader (ADR-016), keeping the
// cross-module orchestration at the Feature layer rather than inside Infrastructure persistence code.
public sealed record CandidateSearchIndexSummary(
    Guid CandidateCvId,
    string FirstName,
    string LastName,
    string Email,
    Guid? ProvinceId,
    Guid? DistrictId,
    int CompletionPercentage);
