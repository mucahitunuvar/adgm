using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employment.Infrastructure.Persistence;

public sealed class EmploymentRepository(EmploymentDbContext dbContext) : IEmploymentRepository
{
    public Task<Domain.Employment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Employments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Employment>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Employments
            .AsNoTracking()
            .Where(e => e.CandidateCvId == candidateCvId)
            .ToListAsync(cancellationToken);
    }

    public void Add(Domain.Employment employment)
    {
        dbContext.Employments.Add(employment);
    }
}
