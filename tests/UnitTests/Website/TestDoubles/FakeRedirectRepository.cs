using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeRedirectRepository : IRedirectRepository
{
    private readonly List<Redirect> _redirects = [];

    public IReadOnlyCollection<Redirect> Redirects => _redirects.AsReadOnly();

    public void Seed(Redirect redirect) => _redirects.Add(redirect);

    public Task<Redirect?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_redirects.FirstOrDefault(r => r.Id == id));

    public Task<Redirect?> GetByFromPathAsync(LanguageCode languageCode, string fromPath, CancellationToken cancellationToken = default) =>
        Task.FromResult(_redirects.FirstOrDefault(r => r.LanguageCode == languageCode && r.FromPath == fromPath));

    public Task<PagedResult<Redirect>> SearchAsync(
        LanguageCode? languageCode, bool? isAutomatic, string? search, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        IEnumerable<Redirect> query = _redirects;
        if (languageCode is not null)
        {
            query = query.Where(r => r.LanguageCode == languageCode);
        }

        if (isAutomatic is not null)
        {
            query = query.Where(r => r.IsAutomatic == isAutomatic);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.FromPath.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var items = query.ToList();
        return Task.FromResult(new PagedResult<Redirect>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<Redirect>> GetByTargetContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Redirect>>(_redirects.Where(r => r.TargetContentItemId == contentItemId).ToList());

    public void Add(Redirect redirect) => _redirects.Add(redirect);

    public void Remove(Redirect redirect) => _redirects.Remove(redirect);
}
