using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.Modules.Employment.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employment.Infrastructure.Persistence;

public sealed class EmploymentNoteRepository(EmploymentDbContext dbContext) : IEmploymentNoteRepository
{
    public void Add(EmploymentNote employmentNote)
    {
        dbContext.EmploymentNotes.Add(employmentNote);
    }

    public Task<PagedResult<EmploymentNote>> GetByEmploymentIdAsync(
        Guid employmentId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        return dbContext.EmploymentNotes
            .AsNoTracking()
            .Where(n => n.EmploymentId == employmentId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToPagedResultAsync(pagedRequest, cancellationToken);
    }
}
