using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Support.TestDoubles;

public sealed class FakeSupportTicketRepository : ISupportTicketRepository
{
    private readonly List<SupportTicket> _tickets = [];

    public IReadOnlyCollection<SupportTicket> Tickets => _tickets.AsReadOnly();

    public Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tickets.FirstOrDefault(t => t.Id == id));

    public Task<PagedResult<SupportTicket>> GetAllAsync(
        Guid? openedByUserId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var matches = openedByUserId is null
            ? _tickets
            : _tickets.Where(t => t.OpenedByUserId == openedByUserId.Value).ToList();

        var page = matches
            .Skip((pagedRequest.Page - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<SupportTicket>(page, matches.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<SupportTicket>> GetOverdueOpenTicketsAsync(
        DateTime threshold, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SupportTicket> matches = _tickets
            .Where(t => t.Status == SupportTicketStatus.Acik && t.OpenedSinceUtc <= threshold)
            .ToList();

        return Task.FromResult(matches);
    }

    public void Add(SupportTicket supportTicket)
    {
        _tickets.Add(supportTicket);
    }
}
