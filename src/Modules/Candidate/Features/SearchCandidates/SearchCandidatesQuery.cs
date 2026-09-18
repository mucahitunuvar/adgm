using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidates;

// CareerAdvisorId is intentionally absent from this filter set (ADR-020): the CareerAdvisor module
// (and its planned automatic/load-balanced assignment) doesn't exist yet, so an Advisor currently
// sees the same unfiltered pool an Admin does - see SearchCandidatesEndpoint's role check.
public sealed record SearchCandidatesQuery(
    string? SearchText,
    Guid? ProvinceId,
    Guid? DistrictId,
    Guid? EducationLevelId,
    Guid? SectorId,
    int? MinCompletionPercentage)
    : PagedRequest, IRequest<Result<SearchCandidatesResponse>>;
