using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface ISiteLanguageRepository
{
    Task<SiteLanguage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SiteLanguage?> GetByCodeAsync(LanguageCode code, CancellationToken cancellationToken = default);

    Task<SiteLanguage?> GetDefaultAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SiteLanguage>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SiteLanguage>> GetActiveAsync(CancellationToken cancellationToken = default);

    void Add(SiteLanguage siteLanguage);
}
