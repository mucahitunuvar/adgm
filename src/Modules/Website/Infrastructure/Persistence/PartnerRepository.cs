using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class PartnerRepository(WebsiteDbContext dbContext) : IPartnerRepository
{
    public Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Partners.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<PagedResult<Partner>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Partners.AsNoTracking().AsQueryable();

        if (isActive is not null)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Translations.Any(
                t => t.LanguageCode == defaultLanguageCode && t.Name.Contains(search)));
        }

        return query.OrderBy(p => p.SortOrder).ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<Partner>> SearchPublicAsync(LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        await dbContext.Partners
            .AsNoTracking()
            .Where(p => p.IsActive && p.Translations.Any(t => t.LanguageCode == languageCode))
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Partner>> SearchByLogoMediaIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        await dbContext.Partners.AsNoTracking().Where(p => p.LogoMediaId == mediaAssetId).ToListAsync(cancellationToken);

    public void Add(Partner partner) => dbContext.Partners.Add(partner);

    public void Remove(Partner partner) => dbContext.Partners.Remove(partner);
}
