using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateExperience;

public sealed record UpdateExperienceCommand(
    Guid CandidateCvId,
    Guid ExperienceId,
    string CompanyName,
    Guid? PositionId,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrentJob,
    Guid? SectorId,
    Guid? WorkFieldId,
    Guid? EmploymentTypeId,
    Guid? CountryId,
    Guid? ProvinceId,
    string? JobDescription) : IRequest<Result>;
