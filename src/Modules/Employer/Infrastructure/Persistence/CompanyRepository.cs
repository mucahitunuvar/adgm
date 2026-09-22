using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Persistence;

public sealed class CompanyRepository(EmployerDbContext dbContext) : ICompanyRepository
{
    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Company?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Companies.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetCompanyCountsByCareerAdvisorAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Where(c => c.CareerAdvisorId != null
                && c.Status != CompanyStatus.Rejected
                && c.Status != CompanyStatus.Deactivated)
            .GroupBy(c => c.CareerAdvisorId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> GetByCareerAdvisorIdAsync(
        Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Companies
            .Where(c => c.CareerAdvisorId == careerAdvisorId)
            .ToListAsync(cancellationToken);
    }

    public void Add(Company company)
    {
        dbContext.Companies.Add(company);
    }
}
