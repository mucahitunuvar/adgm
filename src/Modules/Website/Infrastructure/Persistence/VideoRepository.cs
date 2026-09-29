using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class VideoRepository(WebsiteDbContext dbContext) : IVideoRepository
{
    public Task<Video?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Videos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<PagedResult<Video>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Videos.AsNoTracking().AsQueryable();

        if (isActive is not null)
        {
            query = query.Where(v => v.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Translations.Any(
                t => t.LanguageCode == defaultLanguageCode && t.Title.Contains(search)));
        }

        return query.OrderBy(v => v.SortOrder).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public Task<PagedResult<Video>> SearchPublicAsync(
        LanguageCode languageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Videos
            .AsNoTracking()
            .Where(v => v.IsActive && v.Translations.Any(t => t.LanguageCode == languageCode))
            .OrderBy(v => v.SortOrder);

        return query.ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<Video>> SearchByCoverImageIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        await dbContext.Videos.AsNoTracking().Where(v => v.CoverImageMediaId == mediaAssetId).ToListAsync(cancellationToken);

    public void Add(Video video) => dbContext.Videos.Add(video);

    public void Remove(Video video) => dbContext.Videos.Remove(video);
}
