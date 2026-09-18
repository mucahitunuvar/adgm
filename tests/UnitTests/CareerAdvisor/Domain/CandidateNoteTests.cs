namespace GenclikMerkezi.UnitTests.CareerAdvisor.Domain;

public class CandidateNoteTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var candidateCvId = Guid.NewGuid();
        var careerAdvisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var note = GenclikMerkezi.Modules.CareerAdvisor.Domain.CandidateNote.Create(
            candidateCvId, careerAdvisorId, GenclikMerkezi.Modules.CareerAdvisor.Domain.NoteType.IsGorusmesi, "İçerik", createdAtUtc);

        Assert.Equal(candidateCvId, note.CandidateCvId);
        Assert.Equal(careerAdvisorId, note.CareerAdvisorId);
        Assert.Equal(GenclikMerkezi.Modules.CareerAdvisor.Domain.NoteType.IsGorusmesi, note.NoteType);
        Assert.Equal("İçerik", note.Content);
        Assert.Equal(createdAtUtc, note.CreatedAtUtc);
        Assert.NotEqual(Guid.Empty, note.Id);
    }
}
