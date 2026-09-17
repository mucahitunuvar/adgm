using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.AddEducation;

public sealed record AddEducationCommand(
    Guid CandidateCvId,
    Guid EducationLevelId,
    DateOnly StartDate,
    string CompletionStatus,
    DateOnly? EndDate,
    Guid? DiplomaGradingSystemId,
    decimal? DiplomaGrade,
    Guid? SchoolId,
    string? SchoolNameFreeText,
    Guid? ProvinceId,
    string? Description) : IRequest<Result<Guid>>;
