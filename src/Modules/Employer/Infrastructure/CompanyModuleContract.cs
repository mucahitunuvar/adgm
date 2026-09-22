using GenclikMerkezi.Contracts.Employer;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employer.Infrastructure;

// Saf okuma projeksiyonu (CareerAdvisorModuleContract.GetActiveAdvisorsAsync deseni) - doğrudan
// DbContext'e sorgu atar, iş mantığı/komut delegasyonu gerektirmez.
public sealed class CompanyModuleContract(EmployerDbContext dbContext) : ICompanyModuleContract
{
    public async Task<CompanySummary?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => new CompanySummary(c.Id, c.Name, c.CareerAdvisorId, c.UserId, c.ContactEmail))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CompanySummary?> GetCompanyByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new CompanySummary(c.Id, c.Name, c.CareerAdvisorId, c.UserId, c.ContactEmail))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetCareerAdvisorIdForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.CareerAdvisorId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CompanySummary>> GetCompaniesByIdsAsync(
        IReadOnlyCollection<Guid> companyIds, CancellationToken cancellationToken = default)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .Select(c => new CompanySummary(c.Id, c.Name, c.CareerAdvisorId, c.UserId, c.ContactEmail))
            .ToListAsync(cancellationToken);
    }
}
