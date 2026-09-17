using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure;

// The implementation behind ADR-016 Decision 2's published Contracts interface. ListAsync is
// cached (this data is small and rarely written) and invalidated by the admin-managed CRUD
// handlers via ReferenceDataCacheKeys on every mutation. Paging is applied in-memory over the
// cached full list rather than caching per (type, activeOnly, page, pageSize) combination - that
// would turn LookupCacheInvalidator's fixed two-key invalidation into an unbounded key space.
// These lists are small (largest is District at ~975 rows), so caching the full list and paging
// it in memory is both simpler and cheap.
// ExistsAndActiveAsync is deliberately NOT cached: it backs write-time validation elsewhere (e.g.
// "is this SectorId still active"), where staleness would be a correctness bug, not just a
// slightly-stale dropdown.
public sealed class ReferenceDataLookupReader(ReferenceDataDbContext dbContext, IMemoryCache cache)
    : IReferenceDataLookupReader
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<bool> ExistsAndActiveAsync(
        ReferenceDataLookupType type, Guid id, CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(type);
        return await query.AnyAsync(l => l.Id == id && l.IsActive, cancellationToken);
    }

    public async Task<PagedResult<LookupItemSummary>> ListAsync(
        ReferenceDataLookupType type,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = ReferenceDataCacheKeys.List(type, activeOnly);

        if (!cache.TryGetValue(cacheKey, out IReadOnlyList<LookupItemSummary>? all) || all is null)
        {
            var query = GetQueryable(type);

            if (activeOnly)
            {
                query = query.Where(l => l.IsActive);
            }

            all = await query
                .OrderBy(l => l.SortOrder)
                .Select(l => new LookupItemSummary(l.Id, l.Code, l.DisplayName, l.IsActive, l.SortOrder))
                .ToListAsync(cancellationToken);

            cache.Set(cacheKey, all, CacheDuration);
        }

        var page = all
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToList();

        return new PagedResult<LookupItemSummary>(page, all.Count, paging.Page, paging.PageSize);
    }

    public Task<PagedResult<LookupItemSummary>> ListByParentAsync(
        ReferenceDataLookupType type,
        Guid parentId,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<LookupItem> query = type switch
        {
            ReferenceDataLookupType.District => dbContext.Districts.Where(d => d.ProvinceId == parentId),
            ReferenceDataLookupType.TaxOffice => dbContext.TaxOffices.Where(t => t.ProvinceId == parentId),
            _ => throw new NotSupportedException($"{type} does not support a parent-scoped lookup."),
        };

        if (activeOnly)
        {
            query = query.Where(l => l.IsActive);
        }

        return query
            .OrderBy(l => l.SortOrder)
            .Select(l => new LookupItemSummary(l.Id, l.Code, l.DisplayName, l.IsActive, l.SortOrder))
            .ToPagedResultAsync(paging, cancellationToken);
    }

    private IQueryable<LookupItem> GetQueryable(ReferenceDataLookupType type) => type switch
    {
        ReferenceDataLookupType.Country => dbContext.Countries,
        ReferenceDataLookupType.Province => dbContext.Provinces,
        ReferenceDataLookupType.District => dbContext.Districts,
        ReferenceDataLookupType.Language => dbContext.Languages,
        ReferenceDataLookupType.Sector => dbContext.Sectors,
        ReferenceDataLookupType.Position => dbContext.Positions,
        ReferenceDataLookupType.Department => dbContext.Departments,
        ReferenceDataLookupType.WorkLocationType => dbContext.WorkLocationTypes,
        ReferenceDataLookupType.EmploymentType => dbContext.EmploymentTypes,
        ReferenceDataLookupType.EducationLevel => dbContext.EducationLevels,
        ReferenceDataLookupType.Gender => dbContext.Genders,
        ReferenceDataLookupType.MilitaryStatus => dbContext.MilitaryStatuses,
        ReferenceDataLookupType.DriversLicenseType => dbContext.DriversLicenseTypes,
        ReferenceDataLookupType.LanguageLevel => dbContext.LanguageLevels,
        ReferenceDataLookupType.ExperienceLevel => dbContext.ExperienceLevels,
        ReferenceDataLookupType.Nationality => dbContext.Nationalities,
        ReferenceDataLookupType.DisabilityCategory => dbContext.DisabilityCategories,
        ReferenceDataLookupType.DiplomaGradingSystem => dbContext.DiplomaGradingSystems,
        ReferenceDataLookupType.ReferenceType => dbContext.ReferenceTypes,
        ReferenceDataLookupType.Currency => dbContext.Currencies,
        ReferenceDataLookupType.Skill => dbContext.Skills,
        ReferenceDataLookupType.SchoolCategory => dbContext.SchoolCategories,
        ReferenceDataLookupType.TaxOffice => dbContext.TaxOffices,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown ReferenceData lookup type."),
    };
}
