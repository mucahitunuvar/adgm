using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin list (ADR-024 §8.2 Faz 2 Görev 3): search matches the default language's name.
    Task<PagedResult<Partner>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // Public listing (ADR-024 §3/§8.2): active only, SortOrder ascending, only partners with a
    // translation in languageCode - mirrors IVideoRepository.SearchPublicAsync's own filtering.
    Task<IReadOnlyList<Partner>> SearchPublicAsync(LanguageCode languageCode, CancellationToken cancellationToken = default);

    // PartnerMediaUsageProvider's deletion guard: which partners currently use this media asset as
    // their logo.
    Task<IReadOnlyList<Partner>> SearchByLogoMediaIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);

    void Add(Partner partner);

    void Remove(Partner partner);
}
