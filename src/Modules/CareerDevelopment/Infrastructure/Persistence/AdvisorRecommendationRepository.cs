using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Persistence;

public sealed class AdvisorRecommendationRepository(CareerDevelopmentDbContext dbContext) : IAdvisorRecommendationRepository
{
    public Task<AdvisorRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.AdvisorRecommendations.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdvisorRecommendation>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AdvisorRecommendations
            .AsNoTracking()
            .Where(a => a.CandidateCvId == candidateCvId)
            .ToListAsync(cancellationToken);
    }

    public void Add(AdvisorRecommendation advisorRecommendation)
    {
        dbContext.AdvisorRecommendations.Add(advisorRecommendation);
    }
}
