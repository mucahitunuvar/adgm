using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Support.TestDoubles;

public sealed class FakeSupportTicketMessageRepository : ISupportTicketMessageRepository
{
    private readonly List<SupportTicketMessage> _messages = [];

    public IReadOnlyCollection<SupportTicketMessage> Messages => _messages.AsReadOnly();

    public void Add(SupportTicketMessage supportTicketMessage)
    {
        _messages.Add(supportTicketMessage);
    }

    public Task<PagedResult<SupportTicketMessage>> GetByTicketIdAsync(
        Guid supportTicketId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var matches = _messages.Where(m => m.SupportTicketId == supportTicketId).ToList();

        var page = matches
            .Skip((pagedRequest.Page - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<SupportTicketMessage>(page, matches.Count, pagedRequest.Page, pagedRequest.PageSize));
    }
}
