using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.Persistence;

public sealed class CandidateNoteRepository(CareerAdvisorDbContext dbContext) : ICandidateNoteRepository
{
    public void Add(CandidateNote candidateNote)
    {
        dbContext.CandidateNotes.Add(candidateNote);
    }

    public Task<PagedResult<CandidateNote>> GetByCandidateCvIdAsync(
        Guid candidateCvId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateNotes
            .AsNoTracking()
            .Where(n => n.CandidateCvId == candidateCvId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToPagedResultAsync(pagedRequest, cancellationToken);
    }
}
