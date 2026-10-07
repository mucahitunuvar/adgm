using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;

// Görev 3 (Employer public jobs master prompt). Yalnızca public kurallara uyan (Published ilan +
// Approved firma, Görev 1-2) ilanlar - filtreleme IJobRepository.SearchPublicJobsAsync'in Jobs+
// Companies join'inde yapılır, burada yalnızca lookup adları toplu çözülür ve özet üretilir
// (GetGeneralPoolQueryHandler'daki toplu-çözüm + Dictionary/TryGetValue deseniyle aynı).
public sealed class GetPublicJobsQueryHandler(IJobRepository jobRepository, IReferenceDataLookupReader referenceDataLookupReader)
    : IRequestHandler<GetPublicJobsQuery, Result<PagedResult<PublicJobListItemResponse>>>
{
    private const int MinSearchLength = 2;
    private const int MaxSearchLength = 100;
    private const int SummaryMaxLength = 200;

    public async Task<Result<PagedResult<PublicJobListItemResponse>>> Handle(
        GetPublicJobsQuery request, CancellationToken cancellationToken)
    {
        if (request.Q is { Length: > 0 } q && (q.Length < MinSearchLength || q.Length > MaxSearchLength))
        {
            return Result.Failure<PagedResult<PublicJobListItemResponse>>(Error.Validation(
                "Job.SearchLengthInvalid", $"q must be between {MinSearchLength} and {MaxSearchLength} characters."));
        }

        var filter = new PublicJobSearchFilter(
            request.ProvinceId,
            request.EmploymentTypeId,
            request.WorkLocationTypeId,
            request.PositionId,
            request.DepartmentId,
            request.CompanyId,
            request.IsForDisabledCandidates,
            string.IsNullOrWhiteSpace(request.Q) ? null : request.Q);

        var paged = await jobRepository.SearchPublicJobsAsync(filter, request, cancellationToken);

        var provinceIds = paged.Items.Select(i => i.ProvinceId).Distinct().ToArray();
        var employmentTypeIds = paged.Items.Select(i => i.EmploymentTypeId).Distinct().ToArray();
        var workLocationTypeIds = paged.Items.Select(i => i.WorkLocationTypeId).Distinct().ToArray();
        var positionIds = paged.Items.Select(i => i.PositionId).Distinct().ToArray();

        var provinceNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.Province, provinceIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var employmentTypeNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.EmploymentType, employmentTypeIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var workLocationTypeNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.WorkLocationType, workLocationTypeIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var positionNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.Position, positionIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);

        var items = paged.Items.Select(i => new PublicJobListItemResponse(
                i.JobId,
                i.Slug!, // Published ilan her zaman slug'a sahiptir (Görev 1: Approve() ilk yayında atar).
                i.Title,
                new PublicJobCompanyResponse(i.CompanyId, i.CompanyName, i.CompanyHasLogo),
                provinceNamesById.TryGetValue(i.ProvinceId, out var provinceName) ? provinceName : null,
                employmentTypeNamesById.TryGetValue(i.EmploymentTypeId, out var employmentTypeName) ? employmentTypeName : null,
                workLocationTypeNamesById.TryGetValue(i.WorkLocationTypeId, out var workLocationTypeName) ? workLocationTypeName : null,
                positionNamesById.TryGetValue(i.PositionId, out var positionName) ? positionName : null,
                i.IsForDisabledCandidates,
                i.PublishedAtUtc,
                JobSummaryTextBuilder.Build(i.DescriptionHtml, SummaryMaxLength)))
            .ToList();

        return Result.Success(new PagedResult<PublicJobListItemResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
