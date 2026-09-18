using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeCandidateNoteRepository : ICandidateNoteRepository
{
    private readonly List<GenclikMerkezi.Modules.CareerAdvisor.Domain.CandidateNote> _candidateNotes = [];

    public IReadOnlyCollection<GenclikMerkezi.Modules.CareerAdvisor.Domain.CandidateNote> CandidateNotes => _candidateNotes.AsReadOnly();

    public void Add(GenclikMerkezi.Modules.CareerAdvisor.Domain.CandidateNote candidateNote)
    {
        _candidateNotes.Add(candidateNote);
    }

    public Task<PagedResult<GenclikMerkezi.Modules.CareerAdvisor.Domain.CandidateNote>> GetByCandidateCvIdAsync(
        Guid candidateCvId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var matches = _candidateNotes
            .Where(n => n.CandidateCvId == candidateCvId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToList();

        var items = matches
            .Skip((pagedRequest.Page - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<GenclikMerkezi.Modules.CareerAdvisor.Domain.CandidateNote>(
            items, matches.Count, pagedRequest.Page, pagedRequest.PageSize));
    }
}
