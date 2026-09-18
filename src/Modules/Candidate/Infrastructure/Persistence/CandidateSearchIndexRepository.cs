using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Persistence;

public sealed class CandidateSearchIndexRepository(CandidateDbContext dbContext) : ICandidateSearchIndexRepository
{
    public Task<CandidateSearchIndex?> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateSearchIndexes
            .Include(i => i.EducationLevels)
            .Include(i => i.Sectors)
            .FirstOrDefaultAsync(i => i.Id == candidateCvId, cancellationToken);
    }

    public void Add(CandidateSearchIndex index)
    {
        dbContext.CandidateSearchIndexes.Add(index);
    }
}
