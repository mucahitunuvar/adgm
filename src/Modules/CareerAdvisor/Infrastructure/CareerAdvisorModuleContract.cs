using GenclikMerkezi.Contracts.CareerAdvisor;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure;

public sealed class CareerAdvisorModuleContract(CareerAdvisorDbContext dbContext) : ICareerAdvisorModuleContract
{
    public async Task<IReadOnlyList<ActiveCareerAdvisorSummary>> GetActiveAdvisorsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CareerAdvisors
            .AsNoTracking()
            .Where(a => a.IsActive)
            .Select(a => new ActiveCareerAdvisorSummary(a.Id))
            .ToListAsync(cancellationToken);
    }
}
