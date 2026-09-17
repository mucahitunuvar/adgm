using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateEducation;

public sealed record UpdateEducationCommand(
    Guid CandidateCvId,
    Guid EducationId,
    Guid EducationLevelId,
    DateOnly StartDate,
    string CompletionStatus,
    DateOnly? EndDate,
    Guid? DiplomaGradingSystemId,
    decimal? DiplomaGrade,
    Guid? SchoolId,
    string? SchoolNameFreeText,
    Guid? ProvinceId,
    string? Description) : IRequest<Result>;
