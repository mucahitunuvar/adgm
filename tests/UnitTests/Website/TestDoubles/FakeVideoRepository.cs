using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeVideoRepository : IVideoRepository
{
    private readonly List<Video> _videos = [];

    public void Seed(Video video) => _videos.Add(video);

    public int GetByIdsAsyncCallCount { get; private set; }

    public Task<Video?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_videos.FirstOrDefault(v => v.Id == id));

    public Task<IReadOnlyList<Video>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        GetByIdsAsyncCallCount++;
        return Task.FromResult<IReadOnlyList<Video>>(_videos.Where(v => ids.Contains(v.Id)).ToList());
    }

    public Task<PagedResult<Video>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var items = _videos.OrderBy(v => v.SortOrder).ToList();
        return Task.FromResult(new PagedResult<Video>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<PagedResult<Video>> SearchPublicAsync(
        LanguageCode languageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var items = _videos.Where(v => v.IsActive).OrderBy(v => v.SortOrder).ToList();
        return Task.FromResult(new PagedResult<Video>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<Video>> SearchByCoverImageIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Video>>(_videos.Where(v => v.CoverImageMediaId == mediaAssetId).ToList());

    public void Add(Video video) => _videos.Add(video);

    public void Remove(Video video) => _videos.RemoveAll(v => v.Id == video.Id);
}
