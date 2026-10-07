using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobDetail;

// Görev 3 (Employer public jobs master prompt): bulunamayan/yayında olmayan/firması onaysız ilan
// için aynı 404 (neden ayrımı sızdırılmaz). GetPublicCompanyProfileQueryHandler'daki (Görev 2)
// Approved kontrolü ve sanitizer kullanımıyla aynı desen.
public sealed class GetPublicJobDetailQueryHandler(
    IJobRepository jobRepository,
    ICompanyRepository companyRepository,
    IReferenceDataLookupReader referenceDataLookupReader,
    IHtmlContentSanitizer htmlContentSanitizer)
    : IRequestHandler<GetPublicJobDetailQuery, Result<GetPublicJobDetailResponse>>
{
    private static readonly Error NotFoundError = Error.NotFound("Job.NotFound", "The specified job could not be found.");

    public async Task<Result<GetPublicJobDetailResponse>> Handle(
        GetPublicJobDetailQuery request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetBySlugAsync(request.Slug, cancellationToken);

        if (job is null || job.Status != JobStatus.Published)
        {
            return Result.Failure<GetPublicJobDetailResponse>(NotFoundError);
        }

        var company = await companyRepository.GetByIdAsync(job.CompanyId, cancellationToken);

        if (company is null || company.Status != CompanyStatus.Approved)
        {
            return Result.Failure<GetPublicJobDetailResponse>(NotFoundError);
        }

        var provinceNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.Province, [job.ProvinceId], cancellationToken);
        var employmentTypeNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.EmploymentType, [job.EmploymentTypeId], cancellationToken);
        var workLocationTypeNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.WorkLocationType, [job.WorkLocationTypeId], cancellationToken);
        var positionNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.Position, [job.PositionId], cancellationToken);
        var departmentNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.Department, [job.DepartmentId], cancellationToken);
        var experienceLevelNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.ExperienceLevel, [job.ExperienceLevelId], cancellationToken);

        var educationLevelIds = job.EducationLevelPreferences.Select(p => p.EducationLevelId).Distinct().ToArray();
        var educationLevelNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.EducationLevel, educationLevelIds, cancellationToken);

        var drivingLicenseIds = job.DrivingLicensePreferences.Select(p => p.DriversLicenseTypeId).Distinct().ToArray();
        var drivingLicenseNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.DriversLicenseType, drivingLicenseIds, cancellationToken);

        var militaryStatusIds = job.MilitaryStatusPreferences.Select(p => p.MilitaryStatusId).Distinct().ToArray();
        var militaryStatusNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.MilitaryStatus, militaryStatusIds, cancellationToken);

        var languageIds = job.LanguageRequirements.Select(r => r.LanguageId).Distinct().ToArray();
        var languageLevelIds = job.LanguageRequirements.Select(r => r.LanguageLevelId).Distinct().ToArray();
        var languageNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.Language, languageIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var languageLevelNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.LanguageLevel, languageLevelIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);

        var languageRequirements = job.LanguageRequirements
            .Select(r => new PublicJobLanguageRequirementResponse(
                languageNamesById.TryGetValue(r.LanguageId, out var languageName) ? languageName : null,
                languageLevelNamesById.TryGetValue(r.LanguageLevelId, out var levelName) ? levelName : null))
            .ToList();

        return Result.Success(new GetPublicJobDetailResponse(
            job.Id,
            job.Slug!, // Published ilan her zaman slug'a sahiptir (Görev 1).
            job.Title,
            new PublicJobCompanyResponse(company.Id, company.Name, company.Logo is not null && company.ShowLogoOnWebsite),
            provinceNames.FirstOrDefault()?.DisplayName,
            employmentTypeNames.FirstOrDefault()?.DisplayName,
            workLocationTypeNames.FirstOrDefault()?.DisplayName,
            positionNames.FirstOrDefault()?.DisplayName,
            departmentNames.FirstOrDefault()?.DisplayName,
            job.IsForDisabledCandidates,
            job.PublishedAtUtc!.Value,
            job.DescriptionHtml is null ? null : htmlContentSanitizer.Sanitize(job.DescriptionHtml),
            experienceLevelNames.FirstOrDefault()?.DisplayName,
            educationLevelNames.Select(l => l.DisplayName).ToList(),
            drivingLicenseNames.Select(l => l.DisplayName).ToList(),
            militaryStatusNames.Select(l => l.DisplayName).ToList(),
            languageRequirements));
    }
}
