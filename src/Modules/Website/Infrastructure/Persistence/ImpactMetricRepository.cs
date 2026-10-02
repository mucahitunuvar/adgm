using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class ImpactMetricRepository(WebsiteDbContext dbContext) : IImpactMetricRepository
{
    public Task<ImpactMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ImpactMetrics.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<PagedResult<ImpactMetric>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ImpactMetrics.AsNoTracking().AsQueryable();

        if (isActive is not null)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Translations.Any(
                t => t.LanguageCode == defaultLanguageCode && t.Label.Contains(search)));
        }

        return query.OrderBy(m => m.SortOrder).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<ImpactMetric>> SearchActiveAsync(LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        await dbContext.ImpactMetrics
            .AsNoTracking()
            .Where(m => m.IsActive && m.Translations.Any(t => t.LanguageCode == languageCode))
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

    public void Add(ImpactMetric metric) => dbContext.ImpactMetrics.Add(metric);

    public void Remove(ImpactMetric metric) => dbContext.ImpactMetrics.Remove(metric);
}
