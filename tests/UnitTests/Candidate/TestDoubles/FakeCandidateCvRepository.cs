using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Candidate.TestDoubles;

public sealed class FakeCandidateCvRepository : ICandidateCvRepository
{
    private readonly List<CandidateCv> _candidateCvs = [];

    public IReadOnlyCollection<CandidateCv> CandidateCvs => _candidateCvs.AsReadOnly();

    public Task<CandidateCv?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvs.FirstOrDefault(c => c.Id == id));

    public Task<CandidateCv?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_candidateCvs.FirstOrDefault(c => c.UserId == userId));

    public void Add(CandidateCv candidateCv)
    {
        _candidateCvs.Add(candidateCv);
    }

    public Task<PagedResult<CandidateCvSummary>> SearchAsync(
        CandidateCvSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _candidateCvs.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            query = query.Where(c =>
                c.FirstName.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)
                || c.LastName.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)
                || c.Email.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase));
        }

        var matched = query.OrderByDescending(c => c.CompletionPercentage).ToList();

        var items = matched
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CandidateCvSummary(c.Id, c.UserId, c.FirstName, c.LastName, c.Email, c.CompletionPercentage))
            .ToList();

        return Task.FromResult(new PagedResult<CandidateCvSummary>(items, matched.Count, filter.Page, filter.PageSize));
    }
}
