using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Persistence;

public sealed class TrainingRecommendationRepository(CareerDevelopmentDbContext dbContext) : ITrainingRecommendationRepository
{
    public Task<TrainingRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.TrainingRecommendations.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainingRecommendation>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TrainingRecommendations
            .AsNoTracking()
            .Where(t => t.CandidateCvId == candidateCvId)
            .ToListAsync(cancellationToken);
    }

    public void Add(TrainingRecommendation trainingRecommendation)
    {
        dbContext.TrainingRecommendations.Add(trainingRecommendation);
    }
}
