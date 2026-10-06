using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class ThirdPartyScriptRepository(WebsiteDbContext dbContext) : IThirdPartyScriptRepository
{
    public Task<ThirdPartyScript?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ThirdPartyScripts.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<PagedResult<ThirdPartyScript>> SearchAsync(
        bool? isActive, ThirdPartyScriptCategory? category, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ThirdPartyScripts.AsNoTracking().AsQueryable();

        if (isActive is not null)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (category is not null)
        {
            query = query.Where(s => s.Category == category.Value);
        }

        return query.OrderBy(s => s.SortOrder).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<ThirdPartyScript>> SearchActiveAsync(LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        await dbContext.ThirdPartyScripts
            .AsNoTracking()
            .Where(s => s.IsActive && s.Translations.Any(t => t.LanguageCode == languageCode))
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);

    public void Add(ThirdPartyScript script) => dbContext.ThirdPartyScripts.Add(script);

    public void Remove(ThirdPartyScript script) => dbContext.ThirdPartyScripts.Remove(script);
}
