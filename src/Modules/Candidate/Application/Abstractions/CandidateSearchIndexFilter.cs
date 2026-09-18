namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public sealed record CandidateSearchIndexFilter(
    string? SearchText,
    Guid? ProvinceId,
    Guid? DistrictId,
    Guid? EducationLevelId,
    Guid? SectorId,
    int? MinCompletionPercentage,
    int Page,
    int PageSize);
