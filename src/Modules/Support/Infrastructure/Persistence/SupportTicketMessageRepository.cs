using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Support.Infrastructure.Persistence;

public sealed class SupportTicketMessageRepository(SupportDbContext dbContext) : ISupportTicketMessageRepository
{
    public void Add(SupportTicketMessage supportTicketMessage)
    {
        dbContext.SupportTicketMessages.Add(supportTicketMessage);
    }

    public Task<PagedResult<SupportTicketMessage>> GetByTicketIdAsync(
        Guid supportTicketId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        return dbContext.SupportTicketMessages
            .AsNoTracking()
            .Where(m => m.SupportTicketId == supportTicketId)
            .OrderBy(m => m.CreatedAtUtc)
            .ToPagedResultAsync(pagedRequest, cancellationToken);
    }
}
