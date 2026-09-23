using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Persistence;

public sealed class CareerGoalRepository(CareerDevelopmentDbContext dbContext) : ICareerGoalRepository
{
    public Task<CareerGoal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CareerGoals.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CareerGoal>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CareerGoals
            .AsNoTracking()
            .Where(g => g.CandidateCvId == candidateCvId)
            .ToListAsync(cancellationToken);
    }

    public void Add(CareerGoal careerGoal)
    {
        dbContext.CareerGoals.Add(careerGoal);
    }
}
