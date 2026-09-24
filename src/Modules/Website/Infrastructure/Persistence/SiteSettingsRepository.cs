using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class SiteSettingsRepository(WebsiteDbContext dbContext) : ISiteSettingsRepository
{
    public Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default) =>
        dbContext.SiteSettingsEntries.FirstOrDefaultAsync(s => s.Id == SiteSettings.SingletonId, cancellationToken);

    public void Add(SiteSettings siteSettings) => dbContext.SiteSettingsEntries.Add(siteSettings);
}
