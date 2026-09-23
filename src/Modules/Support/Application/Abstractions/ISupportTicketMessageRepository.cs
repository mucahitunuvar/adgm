using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Support.Application.Abstractions;

public interface ISupportTicketMessageRepository
{
    void Add(SupportTicketMessage supportTicketMessage);

    Task<PagedResult<SupportTicketMessage>> GetByTicketIdAsync(
        Guid supportTicketId, PagedRequest pagedRequest, CancellationToken cancellationToken = default);
}
