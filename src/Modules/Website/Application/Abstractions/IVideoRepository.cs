using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IVideoRepository
{
    Task<Video?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin list (Faz 1b Görev 2): search matches the default language's title.
    Task<PagedResult<Video>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // Public list (ADR-024 §5): active only, SortOrder ascending, only videos with a translation in
    // languageCode - the repository filters both conditions so the query never materializes videos the
    // handler would just filter back out.
    Task<PagedResult<Video>> SearchPublicAsync(
        LanguageCode languageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // VideoMediaUsageProvider's deletion guard (ADR-024 §5 Faz 1b Görev 2): which videos currently use
    // this media asset as their cover image.
    Task<IReadOnlyList<Video>> SearchByCoverImageIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);

    void Add(Video video);

    void Remove(Video video);
}
