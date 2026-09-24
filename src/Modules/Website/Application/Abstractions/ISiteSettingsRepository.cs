using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface ISiteSettingsRepository
{
    // Null until the first UpdateSiteSettingsCommand ever runs - see SiteSettings.CreateDefault's
    // remarks for how read paths and the update command each handle that.
    Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default);

    void Add(SiteSettings siteSettings);
}
