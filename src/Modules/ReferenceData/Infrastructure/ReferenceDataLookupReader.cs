using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure;

// The implementation behind ADR-016 Decision 2's published Contracts interface. ListAsync/
// ListByParentAsync are cached (this data is small and rarely written) and invalidated by the
// admin-managed CRUD handlers via ReferenceDataCacheKeys on every mutation.
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

    public async Task<IReadOnlyList<LookupItemSummary>> ListAsync(
        ReferenceDataLookupType type, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var cacheKey = ReferenceDataCacheKeys.List(type, activeOnly);

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<LookupItemSummary>? cached) && cached is not null)
        {
            return cached;
        }

        var query = GetQueryable(type);

        if (activeOnly)
        {
            query = query.Where(l => l.IsActive);
        }

        var items = await query
            .OrderBy(l => l.SortOrder)
            .Select(l => new LookupItemSummary(l.Id, l.Code, l.DisplayName, l.IsActive, l.SortOrder))
            .ToListAsync(cancellationToken);

        cache.Set(cacheKey, (IReadOnlyList<LookupItemSummary>)items, CacheDuration);

        return items;
    }

    public async Task<IReadOnlyList<LookupItemSummary>> ListByParentAsync(
        ReferenceDataLookupType type,
        Guid parentId,
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

        return await query
            .OrderBy(l => l.SortOrder)
            .Select(l => new LookupItemSummary(l.Id, l.Code, l.DisplayName, l.IsActive, l.SortOrder))
            .ToListAsync(cancellationToken);
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
