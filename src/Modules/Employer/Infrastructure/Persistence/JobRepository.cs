using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Persistence;

public sealed class JobRepository(EmployerDbContext dbContext) : IJobRepository
{
    private static IQueryable<Job> IncludeChildCollections(IQueryable<Job> query) => query
        .Include(j => j.GenderPreferences)
        .Include(j => j.MilitaryStatusPreferences)
        .Include(j => j.EducationLevelPreferences)
        .Include(j => j.DrivingLicensePreferences)
        .Include(j => j.LanguageRequirements);

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return IncludeChildCollections(dbContext.Jobs).FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public Task<Job?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return IncludeChildCollections(dbContext.Jobs).AsNoTracking().FirstOrDefaultAsync(j => j.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await IncludeChildCollections(dbContext.Jobs)
            .Where(j => j.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await IncludeChildCollections(dbContext.Jobs)
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Published)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetPendingReviewByAdvisorIdAsync(
        Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        var companyIds = dbContext.Companies
            .Where(c => c.CareerAdvisorId == careerAdvisorId)
            .Select(c => c.Id);

        return await IncludeChildCollections(dbContext.Jobs)
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.UnderReview && companyIds.Contains(j.CompanyId))
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetPublishedCountByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return dbContext.Jobs
            .AsNoTracking()
            .Where(j => j.CompanyId == companyId && j.Status == JobStatus.Published)
            .CountAsync(cancellationToken);
    }

    public Task<PagedResult<PublicJobListItem>> SearchPublicJobsAsync(
        PublicJobSearchFilter filter, PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var query =
            from job in dbContext.Jobs.AsNoTracking()
            join company in dbContext.Companies.AsNoTracking() on job.CompanyId equals company.Id
            where job.Status == JobStatus.Published && company.Status == CompanyStatus.Approved
            select new { job, company };

        if (filter.ProvinceId is { } provinceId)
        {
            query = query.Where(x => x.job.ProvinceId == provinceId);
        }

        if (filter.EmploymentTypeId is { } employmentTypeId)
        {
            query = query.Where(x => x.job.EmploymentTypeId == employmentTypeId);
        }

        if (filter.WorkLocationTypeId is { } workLocationTypeId)
        {
            query = query.Where(x => x.job.WorkLocationTypeId == workLocationTypeId);
        }

        if (filter.PositionId is { } positionId)
        {
            query = query.Where(x => x.job.PositionId == positionId);
        }

        if (filter.DepartmentId is { } departmentId)
        {
            query = query.Where(x => x.job.DepartmentId == departmentId);
        }

        if (filter.CompanyId is { } companyId)
        {
            query = query.Where(x => x.job.CompanyId == companyId);
        }

        if (filter.IsForDisabledCandidates is { } isForDisabledCandidates)
        {
            query = query.Where(x => x.job.IsForDisabledCandidates == isForDisabledCandidates);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x => x.job.Title.Contains(filter.Search) || x.company.Name.Contains(filter.Search));
        }

        var projected = query
            .OrderByDescending(x => x.job.PublishedAtUtc)
            .ThenByDescending(x => x.job.Id)
            .Select(x => new PublicJobListItem(
                x.job.Id,
                x.job.Slug,
                x.job.Title,
                x.company.Id,
                x.company.Name,
                x.company.Logo!.FileKey != null && x.company.ShowLogoOnWebsite,
                x.job.ProvinceId,
                x.job.EmploymentTypeId,
                x.job.WorkLocationTypeId,
                x.job.PositionId,
                x.job.IsForDisabledCandidates,
                x.job.PublishedAtUtc!.Value,
                x.job.DescriptionHtml));

        return projected.ToPagedResultAsync(paging, cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetNeedingSlugBackfillAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Jobs
            .Where(j => j.Slug == null && j.PublishedAtUtc != null)
            .ToListAsync(cancellationToken);
    }

    public void Add(Job job)
    {
        dbContext.Jobs.Add(job);
    }
}
