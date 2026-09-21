using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
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

    public void Add(Job job)
    {
        dbContext.Jobs.Add(job);
    }
}
