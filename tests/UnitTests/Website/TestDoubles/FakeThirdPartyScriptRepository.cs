using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeThirdPartyScriptRepository : IThirdPartyScriptRepository
{
    private readonly List<ThirdPartyScript> _scripts = [];

    public void Seed(ThirdPartyScript script) => _scripts.Add(script);

    public Task<ThirdPartyScript?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_scripts.FirstOrDefault(s => s.Id == id));

    public Task<PagedResult<ThirdPartyScript>> SearchAsync(
        bool? isActive, ThirdPartyScriptCategory? category, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = _scripts.AsEnumerable();
        if (isActive is not null)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (category is not null)
        {
            query = query.Where(s => s.Category == category.Value);
        }

        var items = query.OrderBy(s => s.SortOrder).ToList();
        return Task.FromResult(new PagedResult<ThirdPartyScript>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<ThirdPartyScript>> SearchActiveAsync(LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ThirdPartyScript>>(_scripts
            .Where(s => s.IsActive && s.Translations.Any(t => t.LanguageCode == languageCode))
            .OrderBy(s => s.SortOrder)
            .ToList());

    public void Add(ThirdPartyScript script) => _scripts.Add(script);

    public void Remove(ThirdPartyScript script) => _scripts.Remove(script);
}
