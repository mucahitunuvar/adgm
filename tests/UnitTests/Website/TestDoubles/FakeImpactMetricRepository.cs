using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeImpactMetricRepository : IImpactMetricRepository
{
    private readonly List<ImpactMetric> _metrics = [];

    public void Seed(ImpactMetric metric) => _metrics.Add(metric);

    public Task<ImpactMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_metrics.FirstOrDefault(m => m.Id == id));

    public Task<PagedResult<ImpactMetric>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var items = _metrics.OrderBy(m => m.SortOrder).ToList();
        return Task.FromResult(new PagedResult<ImpactMetric>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<ImpactMetric>> SearchActiveAsync(LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ImpactMetric>>(
            _metrics.Where(m => m.IsActive && m.Translations.Any(t => t.LanguageCode == languageCode)).OrderBy(m => m.SortOrder).ToList());

    public void Add(ImpactMetric metric) => _metrics.Add(metric);

    public void Remove(ImpactMetric metric) => _metrics.RemoveAll(m => m.Id == metric.Id);
}
