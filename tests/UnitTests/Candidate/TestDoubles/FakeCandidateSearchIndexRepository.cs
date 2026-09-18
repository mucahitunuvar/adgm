using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;

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
}
