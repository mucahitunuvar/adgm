using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.Modules.CareerAdvisor.Features.GetCandidateNotes;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.GetCandidateNotes;

public class GetCandidateNotesQueryHandlerTests
{
    private readonly FakeCandidateNoteRepository _candidateNoteRepository = new();

    private GetCandidateNotesQueryHandler CreateHandler() => new(_candidateNoteRepository);

    [Fact]
    public async Task Handle_WithUnknownCandidateCvId_ReturnsEmptyList_NotFailure()
    {
        var result = await CreateHandler().Handle(new GetCandidateNotesQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyNotesForTheRequestedCandidate_NewestFirst()
    {
        var candidateCvId = Guid.NewGuid();
        var otherCandidateCvId = Guid.NewGuid();
        var careerAdvisorId = Guid.NewGuid();

        var older = CandidateNote.Create(candidateCvId, careerAdvisorId, NoteType.Genel, "Eski not", DateTime.UtcNow.AddDays(-1));
        var newer = CandidateNote.Create(candidateCvId, careerAdvisorId, NoteType.IsGorusmesi, "Yeni not", DateTime.UtcNow);
        var unrelated = CandidateNote.Create(otherCandidateCvId, careerAdvisorId, NoteType.Genel, "Başka aday", DateTime.UtcNow);

        _candidateNoteRepository.Add(older);
        _candidateNoteRepository.Add(newer);
        _candidateNoteRepository.Add(unrelated);

        var result = await CreateHandler().Handle(new GetCandidateNotesQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(newer.Id, result.Value.Items[0].Id);
        Assert.Equal(older.Id, result.Value.Items[1].Id);
    }
}
