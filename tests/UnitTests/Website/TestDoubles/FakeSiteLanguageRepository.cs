using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeSiteLanguageRepository : ISiteLanguageRepository
{
    private readonly List<SiteLanguage> _siteLanguages = [];

    public IReadOnlyCollection<SiteLanguage> SiteLanguages => _siteLanguages.AsReadOnly();

    public void Seed(SiteLanguage siteLanguage) => _siteLanguages.Add(siteLanguage);

    public Task<SiteLanguage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_siteLanguages.FirstOrDefault(l => l.Id == id));

    public Task<SiteLanguage?> GetByCodeAsync(LanguageCode code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_siteLanguages.FirstOrDefault(l => l.Code == code));

    public Task<SiteLanguage?> GetDefaultAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_siteLanguages.FirstOrDefault(l => l.IsDefault));

    public Task<IReadOnlyList<SiteLanguage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SiteLanguage> all = _siteLanguages.OrderBy(l => l.SortOrder).ToList();
        return Task.FromResult(all);
    }

    public Task<IReadOnlyList<SiteLanguage>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SiteLanguage> active = _siteLanguages.Where(l => l.IsActive).OrderBy(l => l.SortOrder).ToList();
        return Task.FromResult(active);
    }

    public void Add(SiteLanguage siteLanguage) => _siteLanguages.Add(siteLanguage);
}
