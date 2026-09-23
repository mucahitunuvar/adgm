using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Support.Infrastructure.Persistence;

public sealed class SupportTicketRepository(SupportDbContext dbContext) : ISupportTicketRepository
{
    public Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.SupportTickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public Task<PagedResult<SupportTicket>> GetAllAsync(
        Guid? openedByUserId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.SupportTickets.AsNoTracking();

        if (openedByUserId is not null)
        {
            query = query.Where(t => t.OpenedByUserId == openedByUserId.Value);
        }

        return query
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetOverdueOpenTicketsAsync(
        DateTime threshold, CancellationToken cancellationToken = default)
    {
        return await dbContext.SupportTickets
            .Where(t => t.Status == SupportTicketStatus.Acik && t.OpenedSinceUtc <= threshold)
            .ToListAsync(cancellationToken);
    }

    public void Add(SupportTicket supportTicket)
    {
        dbContext.SupportTickets.Add(supportTicket);
    }
}
