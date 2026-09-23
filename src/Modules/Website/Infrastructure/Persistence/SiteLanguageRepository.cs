using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class SiteLanguageRepository(WebsiteDbContext dbContext) : ISiteLanguageRepository
{
    public Task<SiteLanguage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.SiteLanguages.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<SiteLanguage?> GetByCodeAsync(LanguageCode code, CancellationToken cancellationToken = default) =>
        dbContext.SiteLanguages.FirstOrDefaultAsync(l => l.Code == code, cancellationToken);

    public Task<SiteLanguage?> GetDefaultAsync(CancellationToken cancellationToken = default) =>
        dbContext.SiteLanguages.FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

    public async Task<IReadOnlyList<SiteLanguage>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SiteLanguages
            .AsNoTracking()
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SiteLanguage>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SiteLanguages
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken);

    public void Add(SiteLanguage siteLanguage) => dbContext.SiteLanguages.Add(siteLanguage);
}
