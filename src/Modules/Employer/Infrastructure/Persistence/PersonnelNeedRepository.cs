using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Persistence;

public sealed class PersonnelNeedRepository(EmployerDbContext dbContext) : IPersonnelNeedRepository
{
    private static IQueryable<PersonnelNeed> IncludeChildCollections(IQueryable<PersonnelNeed> query) => query
        .Include(p => p.GenderPreferences)
        .Include(p => p.MilitaryStatusPreferences)
        .Include(p => p.EducationLevelPreferences)
        .Include(p => p.DrivingLicensePreferences);

    public Task<PersonnelNeed?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return IncludeChildCollections(dbContext.PersonnelNeeds).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PersonnelNeed>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await IncludeChildCollections(dbContext.PersonnelNeeds)
            .Where(p => p.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PersonnelNeed>> GetOwnPoolByAdvisorIdAsync(
        Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        var companyIds = dbContext.Companies
            .Where(c => c.CareerAdvisorId == careerAdvisorId)
            .Select(c => c.Id);

        return await IncludeChildCollections(dbContext.PersonnelNeeds)
            .AsNoTracking()
            .Where(p => p.Status == PersonnelNeedStatus.KendiHavuzunda && companyIds.Contains(p.CompanyId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PersonnelNeed>> GetGeneralPoolAsync(CancellationToken cancellationToken = default)
    {
        return await IncludeChildCollections(dbContext.PersonnelNeeds)
            .AsNoTracking()
            .Where(p => p.Status == PersonnelNeedStatus.GenelHavuzda)
            .ToListAsync(cancellationToken);
    }

    public void Add(PersonnelNeed personnelNeed)
    {
        dbContext.PersonnelNeeds.Add(personnelNeed);
    }
}
