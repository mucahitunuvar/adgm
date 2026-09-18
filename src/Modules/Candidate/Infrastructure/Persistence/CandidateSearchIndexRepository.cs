using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Persistence;

public sealed class CandidateSearchIndexRepository(CandidateDbContext dbContext) : ICandidateSearchIndexRepository
{
    public Task<CandidateSearchIndex?> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateSearchIndexes
            .Include(i => i.EducationLevels)
            .Include(i => i.Sectors)
            .FirstOrDefaultAsync(i => i.Id == candidateCvId, cancellationToken);
    }

    public void Add(CandidateSearchIndex index)
    {
        dbContext.CandidateSearchIndexes.Add(index);
    }

    public Task<PagedResult<CandidateSearchIndexSummary>> SearchAsync(
        CandidateSearchIndexFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.CandidateSearchIndexes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            // FullNameNormalized is stored already-normalized (TurkishTextNormalizer) - the query
            // text must go through the same normalization, or e.g. a lowercase "irem" would never
            // match a stored "IREM" (or worse, a Turkish "İrem" folded to "IREM"). Email is matched
            // as typed - it isn't Turkish-text-normalized.
            var normalizedSearchText = TurkishTextNormalizer.Normalize(filter.SearchText);
            var searchText = filter.SearchText.Trim();
            query = query.Where(i =>
                EF.Functions.Like(i.FullNameNormalized, $"%{normalizedSearchText}%")
                || EF.Functions.Like(i.Email, $"%{searchText}%"));
        }

        if (filter.ProvinceId is not null)
        {
            query = query.Where(i => i.ProvinceId == filter.ProvinceId);
        }

        if (filter.DistrictId is not null)
        {
            query = query.Where(i => i.DistrictId == filter.DistrictId);
        }

        if (filter.EducationLevelId is not null)
        {
            query = query.Where(i => i.EducationLevels.Any(e => e.EducationLevelId == filter.EducationLevelId));
        }

        if (filter.SectorId is not null)
        {
            query = query.Where(i => i.Sectors.Any(s => s.SectorId == filter.SectorId));
        }

        if (filter.MinCompletionPercentage is not null)
        {
            query = query.Where(i => i.CompletionPercentage >= filter.MinCompletionPercentage);
        }

        return query
            .OrderByDescending(i => i.CompletionPercentage)
            .Select(i => new CandidateSearchIndexSummary(
                i.Id, i.FirstName, i.LastName, i.Email, i.ProvinceId, i.DistrictId, i.CompletionPercentage))
            .ToPagedResultAsync(new PagedRequest { Page = filter.Page, PageSize = filter.PageSize }, cancellationToken);
    }
}
