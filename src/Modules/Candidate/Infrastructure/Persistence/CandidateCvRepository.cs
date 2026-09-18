using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Persistence;

public sealed class CandidateCvRepository(CandidateDbContext dbContext) : ICandidateCvRepository
{
    public Task<CandidateCv?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateCvs
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<CandidateCv?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateCvs
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetCandidateCountsByCareerAdvisorAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CandidateCvs
            .AsNoTracking()
            .Where(c => c.CareerAdvisorId != null)
            .GroupBy(c => c.CareerAdvisorId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<IReadOnlyList<CandidateCv>> GetByCareerAdvisorIdAsync(
        Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CandidateCvs
            .Where(c => c.CareerAdvisorId == careerAdvisorId)
            .ToListAsync(cancellationToken);
    }

    public void Add(CandidateCv candidateCv)
    {
        dbContext.CandidateCvs.Add(candidateCv);
    }
}
