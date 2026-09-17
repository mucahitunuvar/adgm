using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Persistence;

public sealed class CandidateCvRepository(CandidateDbContext dbContext) : ICandidateCvRepository
{
    public Task<CandidateCv?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateCvs
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<CandidateCv?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.CandidateCvs
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public void Add(CandidateCv candidateCv)
    {
        dbContext.CandidateCvs.Add(candidateCv);
    }

    public Task<PagedResult<CandidateCvSummary>> SearchAsync(
        CandidateCvSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.CandidateCvs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var searchText = filter.SearchText.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.FirstName, $"%{searchText}%")
                || EF.Functions.Like(c.LastName, $"%{searchText}%")
                || EF.Functions.Like(c.Email, $"%{searchText}%"));
        }

        return query
            .OrderByDescending(c => c.CompletionPercentage)
            .Select(c => new CandidateCvSummary(c.Id, c.UserId, c.FirstName, c.LastName, c.Email, c.CompletionPercentage))
            .ToPagedResultAsync(new PagedRequest { Page = filter.Page, PageSize = filter.PageSize }, cancellationToken);
    }
}
