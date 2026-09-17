using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure;

// The implementation behind ADR-016 Decision 2's published Contracts interface. ListAsync and
// ListByParentAsync are cached per (type, activeOnly, page, pageSize[, parentId]) combination
// (ADR-017 Decision 3) and invalidated by the admin-managed CRUD handlers via
// ReferenceDataCacheKeys/LookupCacheInvalidator on every mutation, using ICacheService.RemoveByPrefix
// to clear every combination for a type at once.
// ExistsAndActiveAsync is deliberately NOT cached: it backs write-time validation elsewhere (e.g.
// "is this SectorId still active"), where staleness would be a correctness bug, not just a
// slightly-stale dropdown.
public sealed class ReferenceDataLookupReader(ReferenceDataDbContext dbContext, ICacheService cacheService)
    : IReferenceDataLookupReader
{
    // SEED lookups (ADR-016 Decision 1) change only via migration, never at runtime, so they can
    // safely use a much longer TTL than the service's configured default for ADMIN-MANAGED types
    // (which rely on CRUD-triggered invalidation, not a short timer, to stay fresh).
    private static readonly TimeSpan SeedTtl = TimeSpan.FromHours(24);

    private static readonly HashSet<ReferenceDataLookupType> SeedTypes =
    [
        ReferenceDataLookupType.Country,
        ReferenceDataLookupType.Province,
        ReferenceDataLookupType.District,
        ReferenceDataLookupType.Language,
    ];

    public async Task<bool> ExistsAndActiveAsync(
        ReferenceDataLookupType type, Guid id, CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(type);
        return await query.AnyAsync(l => l.Id == id && l.IsActive, cancellationToken);
    }

    public Task<PagedResult<LookupItemSummary>> ListAsync(
        ReferenceDataLookupType type,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = ReferenceDataCacheKeys.List(type, activeOnly, paging);

        return cacheService.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                var query = GetQueryable(type);

                if (activeOnly)
                {
                    query = query.Where(l => l.IsActive);
                }

                return await query
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new LookupItemSummary(l.Id, l.Code, l.DisplayName, l.IsActive, l.SortOrder))
                    .ToPagedResultAsync(paging, token);
            },
            GetTtl(type),
            cancellationToken);
    }

    public Task<PagedResult<LookupItemSummary>> ListByParentAsync(
        ReferenceDataLookupType type,
        Guid parentId,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = ReferenceDataCacheKeys.ListByParent(type, parentId, activeOnly, paging);

        return cacheService.GetOrCreateAsync(
            cacheKey,
            async token =>
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
                    .ToPagedResultAsync(paging, token);
            },
            GetTtl(type),
            cancellationToken);
    }

    private static TimeSpan? GetTtl(ReferenceDataLookupType type) => SeedTypes.Contains(type) ? SeedTtl : null;

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
