using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class MediaAssetRepository(WebsiteDbContext dbContext) : IMediaAssetRepository
{
    public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.MediaAssets.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<PagedResult<MediaAsset>> SearchAsync(
        MediaAssetKind? kind,
        string? folder,
        string? search,
        bool? missingAltText,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.MediaAssets.AsNoTracking();

        if (kind is not null)
        {
            query = query.Where(m => m.Kind == kind.Value);
        }

        if (!string.IsNullOrWhiteSpace(folder))
        {
            // EF Core cannot translate member access into a value-converted property's CLR value
            // (m.Folder.Value) - only equality between two instances of the converted type itself,
            // the same pattern LanguageCode comparisons already rely on elsewhere.
            var folderResult = MediaFolder.Create(folder);
            if (folderResult.IsSuccess)
            {
                var folderValue = folderResult.Value;
                query = query.Where(m => m.Folder == folderValue);
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m =>
                m.Original.OriginalFileName.Contains(search)
                || m.Translations.Any(t => t.AltText.Contains(search)));
        }

        if (missingAltText == true)
        {
            query = query.Where(m => !m.Translations.Any(t => t.AltText != ""));
        }

        return query.OrderByDescending(m => m.CreatedAtUtc).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetDistinctFoldersAsync(CancellationToken cancellationToken = default)
    {
        var rootFolder = MediaFolder.Create(string.Empty).Value;

        // Same translation limitation as SearchAsync's folder filter - .Value cannot be projected
        // inside the query, so Folder is selected as a whole and unwrapped after materialization.
        var folders = await dbContext.MediaAssets
            .AsNoTracking()
            .Where(m => m.Folder != rootFolder)
            .Select(m => m.Folder)
            .Distinct()
            .ToListAsync(cancellationToken);

        return folders.Select(f => f.Value).OrderBy(f => f, StringComparer.Ordinal).ToList();
    }

    public void Add(MediaAsset mediaAsset) => dbContext.MediaAssets.Add(mediaAsset);

    public void Remove(MediaAsset mediaAsset) => dbContext.MediaAssets.Remove(mediaAsset);
}
