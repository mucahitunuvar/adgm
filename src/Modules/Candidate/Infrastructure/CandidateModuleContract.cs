using GenclikMerkezi.Contracts.Candidate;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure;

// Saf okuma projeksiyonu (CompanyModuleContract deseni) - doğrudan DbContext'e sorgu atar, iş
// mantığı/komut delegasyonu gerektirmez.
public sealed class CandidateModuleContract(CandidateDbContext dbContext) : ICandidateModuleContract
{
    public async Task<CandidateCvSummary?> GetCandidateCvByIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CandidateCvs
            .AsNoTracking()
            .Where(c => c.Id == candidateCvId)
            .Select(c => new CandidateCvSummary(c.Id, c.FirstName, c.LastName, c.CareerAdvisorId, c.UserId, c.Email))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CandidateCvSummary?> GetCandidateCvByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CandidateCvs
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new CandidateCvSummary(c.Id, c.FirstName, c.LastName, c.CareerAdvisorId, c.UserId, c.Email))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetCareerAdvisorIdForCandidateAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CandidateCvs
            .AsNoTracking()
            .Where(c => c.Id == candidateCvId)
            .Select(c => c.CareerAdvisorId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
