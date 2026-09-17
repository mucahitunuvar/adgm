using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Persistence;

public sealed class CandidateCvContentRepository(CandidateDbContext dbContext) : ICandidateCvContentRepository
{
    public Task<CandidateCvContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateCvContents
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Languages)
            .Include(c => c.Certificates)
            .Include(c => c.References)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<CandidateCvContent?> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateCvContents
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Languages)
            .Include(c => c.Certificates)
            .Include(c => c.References)
            .FirstOrDefaultAsync(c => c.CandidateCvId == candidateCvId, cancellationToken);
    }

    public void Add(CandidateCvContent candidateCvContent)
    {
        dbContext.CandidateCvContents.Add(candidateCvContent);
    }
}
