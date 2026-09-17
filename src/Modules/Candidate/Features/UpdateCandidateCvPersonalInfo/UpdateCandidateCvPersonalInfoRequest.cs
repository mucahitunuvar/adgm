namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvPersonalInfo;

public sealed record UpdateCandidateCvPersonalInfoRequest(
    string? Title,
    Guid? GenderId,
    DateOnly? BirthDate,
    Guid? DriversLicenseTypeId,
    Guid? NationalityId,
    decimal? NetSalaryExpectation,
    Guid? MilitaryStatusId,
    DisabilityInfoRequest? DisabilityInfo);
