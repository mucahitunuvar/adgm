using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;

// GetOwnPoolPersonnelNeeds de bu paylaşılan response'u kullanıyor (JobResponse/GetPublishedJobs deseni).
public sealed record PersonnelNeedResponse(
    Guid Id,
    Guid CompanyId,
    Guid EmploymentTypeId,
    Guid WorkLocationTypeId,
    Guid PositionId,
    Guid DepartmentId,
    int Quantity,
    Guid ProvinceId,
    Guid ExperienceLevelId,
    string? DetailsText,
    IReadOnlyList<Guid> GenderPreferenceIds,
    IReadOnlyList<Guid> MilitaryStatusPreferenceIds,
    IReadOnlyList<Guid> EducationLevelPreferenceIds,
    IReadOnlyList<Guid> DrivingLicensePreferenceIds,
    PersonnelNeedStatus Status,
    Guid? PooledByAdvisorId,
    DateTime? PooledAtUtc,
    Guid? ClosedByAdvisorId,
    DateTime? ClosedAtUtc,
    Guid? FulfilledByCandidateCvId,
    DateTime CreatedAtUtc)
{
    public static PersonnelNeedResponse FromDomain(PersonnelNeed personnelNeed) => new(
        personnelNeed.Id,
        personnelNeed.CompanyId,
        personnelNeed.EmploymentTypeId,
        personnelNeed.WorkLocationTypeId,
        personnelNeed.PositionId,
        personnelNeed.DepartmentId,
        personnelNeed.Quantity,
        personnelNeed.ProvinceId,
        personnelNeed.ExperienceLevelId,
        personnelNeed.DetailsText,
        personnelNeed.GenderPreferences.Select(p => p.GenderId).ToList(),
        personnelNeed.MilitaryStatusPreferences.Select(p => p.MilitaryStatusId).ToList(),
        personnelNeed.EducationLevelPreferences.Select(p => p.EducationLevelId).ToList(),
        personnelNeed.DrivingLicensePreferences.Select(p => p.DriversLicenseTypeId).ToList(),
        personnelNeed.Status,
        personnelNeed.PooledByAdvisorId,
        personnelNeed.PooledAtUtc,
        personnelNeed.ClosedByAdvisorId,
        personnelNeed.ClosedAtUtc,
        personnelNeed.FulfilledByCandidateCvId,
        personnelNeed.CreatedAtUtc);
}
