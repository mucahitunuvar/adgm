using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IMediaAssetRepository
{
    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

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
