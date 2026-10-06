using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IThirdPartyScriptRepository
{
    Task<ThirdPartyScript?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin list (ADR-024 §13 Faz 3 Görev 7).
    Task<PagedResult<ThirdPartyScript>> SearchAsync(
        bool? isActive, ThirdPartyScriptCategory? category, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // Public listing (GetPublicSite): active only, SortOrder ascending, only scripts with a
    // translation in languageCode - mirrors IPartnerRepository.SearchPublicAsync's own filtering.
    Task<IReadOnlyList<ThirdPartyScript>> SearchActiveAsync(LanguageCode languageCode, CancellationToken cancellationToken = default);

    void Add(ThirdPartyScript script);

    void Remove(ThirdPartyScript script);
}
