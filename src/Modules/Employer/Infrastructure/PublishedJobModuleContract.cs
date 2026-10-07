using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employer.Infrastructure;

// Görev 4 (Employer public jobs master prompt). CompanyModuleContract/PersonnelNeedModuleContract'ın
// deseniyle aynı: saf okuma projeksiyonu, doğrudan DbContext'e sorgu atar (IJobRepository üzerinden
// değil - oradaki GetPublicJobsQuery'nin (Görev 3) opsiyonel filtreleri burada gereksiz). Lookup
// adlarını (il, çalışma şekli vb.) IReferenceDataLookupReader ile burada, Infrastructure katmanında
// çözer - tüketen taraf hiçbir id çözümlemesi yapmak zorunda kalmasın diye (ADR-016 Decision 2).
public sealed class PublishedJobModuleContract(EmployerDbContext dbContext, IReferenceDataLookupReader referenceDataLookupReader)
    : IPublishedJobModuleContract
{
    private const int MaxPageSize = 200;
    private const int SummaryMaxLength = 500;

    public async Task<PagedResult<PublishedJobSummary>> GetPublishedJobSummariesAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query =
            from job in dbContext.Jobs.AsNoTracking()
            join company in dbContext.Companies.AsNoTracking() on job.CompanyId equals company.Id
            where job.Status == JobStatus.Published && company.Status == CompanyStatus.Approved
            select new { job, company };

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = totalCount == 0
            ? []
            : await query
                .OrderBy(x => x.job.PublishedAtUtc)
                .ThenBy(x => x.job.Id)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(x => new
                {
                    x.job.Id,
                    x.job.Slug,
                    x.job.Title,
                    x.company.Name,
                    x.job.ProvinceId,
                    x.job.EmploymentTypeId,
                    x.job.WorkLocationTypeId,
                    x.job.PositionId,
                    x.job.DescriptionHtml,
                    x.job.PublishedAtUtc,
                })
                .ToListAsync(cancellationToken);

        var provinceIds = rows.Select(r => r.ProvinceId).Distinct().ToArray();
        var employmentTypeIds = rows.Select(r => r.EmploymentTypeId).Distinct().ToArray();
        var workLocationTypeIds = rows.Select(r => r.WorkLocationTypeId).Distinct().ToArray();
        var positionIds = rows.Select(r => r.PositionId).Distinct().ToArray();

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

        var items = rows.Select(r => new PublishedJobSummary(
                r.Id,
                r.Slug!, // Published ilan her zaman slug'a sahiptir (Görev 1).
                r.Title,
                r.Name,
                provinceNamesById.TryGetValue(r.ProvinceId, out var provinceName) ? provinceName : null,
                employmentTypeNamesById.TryGetValue(r.EmploymentTypeId, out var employmentTypeName) ? employmentTypeName : null,
                workLocationTypeNamesById.TryGetValue(r.WorkLocationTypeId, out var workLocationTypeName) ? workLocationTypeName : null,
                positionNamesById.TryGetValue(r.PositionId, out var positionName) ? positionName : null,
                JobSummaryTextBuilder.Build(r.DescriptionHtml, SummaryMaxLength),
                r.PublishedAtUtc!.Value))
            .ToList();

        return new PagedResult<PublishedJobSummary>(items, totalCount, normalizedPage, normalizedPageSize);
    }
}
