using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;

// GetMyCompanyJobs ve GetJobsPendingReview de bu paylaşılan response'u kullanıyor (GetCompanyResponse/
// GetMyCompany deseniyle aynı - Görev 1).
public sealed record JobResponse(
    Guid Id,
    Guid CompanyId,
    string Title,
    bool IsForDisabledCandidates,
    Guid EmploymentTypeId,
    Guid WorkLocationTypeId,
    Guid PositionId,
    Guid DepartmentId,
    Guid ProvinceId,
    string? DescriptionHtml,
    Guid ExperienceLevelId,
    IReadOnlyList<Guid> GenderPreferenceIds,
    IReadOnlyList<Guid> MilitaryStatusPreferenceIds,
    IReadOnlyList<Guid> EducationLevelPreferenceIds,
    IReadOnlyList<Guid> DrivingLicensePreferenceIds,
    IReadOnlyList<JobLanguageRequirementResponse> LanguageRequirements,
    JobStatus Status,
    Guid? ReviewedByAdvisorId,
    DateTime? ReviewedAtUtc,
    string? RejectionReason,
    string? RevisionNotes,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc)
{
    public static JobResponse FromDomain(Job job) => new(
        job.Id,
        job.CompanyId,
        job.Title,
        job.IsForDisabledCandidates,
        job.EmploymentTypeId,
        job.WorkLocationTypeId,
        job.PositionId,
        job.DepartmentId,
        job.ProvinceId,
        job.DescriptionHtml,
        job.ExperienceLevelId,
        job.GenderPreferences.Select(p => p.GenderId).ToList(),
        job.MilitaryStatusPreferences.Select(p => p.MilitaryStatusId).ToList(),
        job.EducationLevelPreferences.Select(p => p.EducationLevelId).ToList(),
        job.DrivingLicensePreferences.Select(p => p.DriversLicenseTypeId).ToList(),
        job.LanguageRequirements.Select(r => new JobLanguageRequirementResponse(r.LanguageId, r.LanguageLevelId)).ToList(),
        job.Status,
        job.ReviewedByAdvisorId,
        job.ReviewedAtUtc,
        job.RejectionReason,
        job.RevisionNotes,
        job.PublishedAtUtc,
        job.CreatedAtUtc);
}
