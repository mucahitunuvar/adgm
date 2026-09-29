using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeNotFoundLogRepository : INotFoundLogRepository
{
    private readonly List<NotFoundLog> _notFoundLogs = [];

    public IReadOnlyCollection<NotFoundLog> NotFoundLogs => _notFoundLogs.AsReadOnly();

    public void Seed(NotFoundLog notFoundLog) => _notFoundLogs.Add(notFoundLog);

    public Task<NotFoundLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_notFoundLogs.FirstOrDefault(n => n.Id == id));

    public Task<NotFoundLog?> GetByPathAsync(LanguageCode languageCode, string path, CancellationToken cancellationToken = default) =>
        Task.FromResult(_notFoundLogs.FirstOrDefault(n => n.LanguageCode == languageCode && n.Path == path));

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(_notFoundLogs.Count);

    public Task<PagedResult<NotFoundLog>> SearchAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var items = _notFoundLogs.OrderByDescending(n => n.HitCount).ToList();
        return Task.FromResult(new PagedResult<NotFoundLog>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<int> DeleteStaleAsync(DateTime olderThanUtc, int maxHitCount, CancellationToken cancellationToken = default)
    {
        var stale = _notFoundLogs.Where(n => n.LastSeenAtUtc < olderThanUtc && n.HitCount < maxHitCount).ToList();
        foreach (var log in stale)
        {
            _notFoundLogs.Remove(log);
        }

        return Task.FromResult(stale.Count);
    }

    public void Add(NotFoundLog notFoundLog) => _notFoundLogs.Add(notFoundLog);

    public void Remove(NotFoundLog notFoundLog) => _notFoundLogs.Remove(notFoundLog);

    public void DetachFailedAdd(NotFoundLog notFoundLog) => _notFoundLogs.Remove(notFoundLog);
}
