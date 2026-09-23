using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeMediaAssetRepository : IMediaAssetRepository
{
    private readonly List<MediaAsset> _mediaAssets = [];

    public IReadOnlyCollection<MediaAsset> MediaAssets => _mediaAssets.AsReadOnly();

    public void Seed(MediaAsset mediaAsset) => _mediaAssets.Add(mediaAsset);

    public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_mediaAssets.FirstOrDefault(m => m.Id == id));

    public Task<PagedResult<MediaAsset>> SearchAsync(
        MediaAssetKind? kind,
        string? folder,
        string? search,
        bool? missingAltText,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _mediaAssets.AsEnumerable();

        if (kind is not null)
        {
            query = query.Where(m => m.Kind == kind.Value);
        }

        if (!string.IsNullOrWhiteSpace(folder))
        {
            query = query.Where(m => m.Folder.Value == folder);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m =>
                m.Original.OriginalFileName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || m.Translations.Any(t => t.AltText.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        if (missingAltText == true)
        {
            query = query.Where(m => !m.Translations.Any(t => t.AltText.Length > 0));
        }

        var matches = query.OrderByDescending(m => m.CreatedAtUtc).ToList();
        var page = matches.Skip((pagedRequest.Page - 1) * pagedRequest.PageSize).Take(pagedRequest.PageSize).ToList();

        return Task.FromResult(new PagedResult<MediaAsset>(page, matches.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<string>> GetDistinctFoldersAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> folders = _mediaAssets
            .Select(m => m.Folder.Value)
            .Where(f => f.Length > 0)
            .Distinct()
            .OrderBy(f => f)
            .ToList();
        return Task.FromResult(folders);
    }

    public void Add(MediaAsset mediaAsset) => _mediaAssets.Add(mediaAsset);

    public void Remove(MediaAsset mediaAsset) => _mediaAssets.RemoveAll(m => m.Id == mediaAsset.Id);
}
