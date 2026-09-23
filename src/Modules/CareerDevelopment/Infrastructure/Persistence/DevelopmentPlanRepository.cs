using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Persistence;

public sealed class DevelopmentPlanRepository(CareerDevelopmentDbContext dbContext) : IDevelopmentPlanRepository
{
    public Task<DevelopmentPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.DevelopmentPlans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DevelopmentPlan>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.DevelopmentPlans
            .AsNoTracking()
            .Where(p => p.CandidateCvId == candidateCvId)
            .ToListAsync(cancellationToken);
    }

    public void Add(DevelopmentPlan developmentPlan)
    {
        dbContext.DevelopmentPlans.Add(developmentPlan);
    }
}
