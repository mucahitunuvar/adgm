using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeSiteSettingsRepository : ISiteSettingsRepository
{
    private SiteSettings? _siteSettings;

    public void Seed(SiteSettings siteSettings) => _siteSettings = siteSettings;

    public Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(_siteSettings);

    public void Add(SiteSettings siteSettings) => _siteSettings = siteSettings;
}
