using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.Modules.Interview.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Interview.Infrastructure.Persistence;

public sealed class InterviewRepository(InterviewDbContext dbContext) : IInterviewRepository
{
    public Task<Domain.Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Interviews.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Interview>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Interviews
            .AsNoTracking()
            .Where(i => i.CandidateCvId == candidateCvId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Interview>> GetByCompanyIdAsync(
        Guid companyId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Interviews
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Interview>> GetPendingByOrganizingAdvisorIdAsync(
        Guid organizingAdvisorId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Interviews
            .AsNoTracking()
            .Where(i => i.OrganizingAdvisorId == organizingAdvisorId
                && (i.Status == InterviewStatus.TalepEdildi || i.Status == InterviewStatus.Planlandi))
            .ToListAsync(cancellationToken);
    }

    public void Add(Domain.Interview interview)
    {
        dbContext.Interviews.Add(interview);
    }
}
