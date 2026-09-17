using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvPersonalInfo;

public sealed record UpdateCandidateCvPersonalInfoCommand(
    Guid CandidateCvId,
    string? Title,
    Guid? GenderId,
    DateOnly? BirthDate,
    Guid? DriversLicenseTypeId,
    Guid? NationalityId,
    decimal? NetSalaryExpectation,
    Guid? MilitaryStatusId,
    DisabilityInfoRequest? DisabilityInfo) : IRequest<Result>;
