using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Candidate.TestDoubles;

public sealed class FakeCandidateSearchIndexRepository : ICandidateSearchIndexRepository
{
    private readonly List<CandidateSearchIndex> _searchIndexes = [];

    public IReadOnlyCollection<CandidateSearchIndex> SearchIndexes => _searchIndexes.AsReadOnly();

    public Task<CandidateSearchIndex?> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_searchIndexes.FirstOrDefault(i => i.Id == candidateCvId));

    public void Add(CandidateSearchIndex index)
    {
        _searchIndexes.Add(index);
    }

    public Task<PagedResult<CandidateSearchIndexSummary>> SearchAsync(
        CandidateSearchIndexFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _searchIndexes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var normalizedSearchText = TurkishTextNormalizer.Normalize(filter.SearchText);
            var searchText = filter.SearchText.Trim();
            query = query.Where(i =>
                i.FullNameNormalized.Contains(normalizedSearchText, StringComparison.Ordinal)
                || i.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase));
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
            query = query.Where(i => i.EducationLevelIds.Contains(filter.EducationLevelId.Value));
        }

        if (filter.SectorId is not null)
        {
            query = query.Where(i => i.SectorIds.Contains(filter.SectorId.Value));
        }

        if (filter.MinCompletionPercentage is not null)
        {
            query = query.Where(i => i.CompletionPercentage >= filter.MinCompletionPercentage);
        }

        var matched = query.OrderByDescending(i => i.CompletionPercentage).ToList();

        var items = matched
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(i => new CandidateSearchIndexSummary(
                i.Id, i.FirstName, i.LastName, i.Email, i.ProvinceId, i.DistrictId, i.CompletionPercentage))
            .ToList();

        return Task.FromResult(new PagedResult<CandidateSearchIndexSummary>(items, matched.Count, filter.Page, filter.PageSize));
    }
}
