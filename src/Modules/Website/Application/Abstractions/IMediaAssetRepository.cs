using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IMediaAssetRepository
{
    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Faz 2 Görev 5 master prompt §5.3: the public block data resolver's bulk lookup - every image
    // media id referenced anywhere in a page layout's blocks is fetched in one query, not one query
    // per block ("tüm medya ID'leri tek sorguda").
    Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<PagedResult<MediaAsset>> SearchAsync(
        MediaAssetKind? kind,
        string? folder,
        string? search,
        bool? missingAltText,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetDistinctFoldersAsync(CancellationToken cancellationToken = default);

    void Add(MediaAsset mediaAsset);

    void Remove(MediaAsset mediaAsset);
}
