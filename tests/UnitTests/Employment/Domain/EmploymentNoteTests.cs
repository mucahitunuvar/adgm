using GenclikMerkezi.Modules.Employment.Domain;

namespace GenclikMerkezi.UnitTests.Employment.Domain;

public class EmploymentNoteTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var employmentId = Guid.NewGuid();
        var careerAdvisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var note = EmploymentNote.Create(employmentId, careerAdvisorId, "İşe uyum süreci iyi gidiyor.", createdAtUtc);

        Assert.Equal(employmentId, note.EmploymentId);
        Assert.Equal(careerAdvisorId, note.CareerAdvisorId);
        Assert.Equal("İşe uyum süreci iyi gidiyor.", note.Content);
        Assert.Equal(createdAtUtc, note.CreatedAtUtc);
    }
}
