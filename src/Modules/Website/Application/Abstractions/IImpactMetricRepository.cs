using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IImpactMetricRepository
{
    Task<ImpactMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin list (ADR-024 §8.2 Faz 2 Görev 3): search matches the default language's label.
    Task<PagedResult<ImpactMetric>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // Public query (ADR-024 §3/§8.2, consumed by Görev 5's impact-stats block): active only,
    // SortOrder ascending, only metrics with a translation in languageCode.
    Task<IReadOnlyList<ImpactMetric>> SearchActiveAsync(LanguageCode languageCode, CancellationToken cancellationToken = default);

    void Add(ImpactMetric metric);

    void Remove(ImpactMetric metric);
}
