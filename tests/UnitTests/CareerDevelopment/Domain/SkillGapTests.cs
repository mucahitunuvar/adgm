using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Domain;

public class SkillGapTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var candidateCvId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        var identifiedByAdvisorId = Guid.NewGuid();
        var identifiedAtUtc = DateTime.UtcNow;

        var skillGap = SkillGap.Create(candidateCvId, skillId, identifiedByAdvisorId, "İngilizce yetersiz", identifiedAtUtc);

        Assert.Equal(candidateCvId, skillGap.CandidateCvId);
        Assert.Equal(skillId, skillGap.SkillId);
        Assert.Equal(identifiedByAdvisorId, skillGap.IdentifiedByAdvisorId);
        Assert.Equal("İngilizce yetersiz", skillGap.Notes);
        Assert.Equal(identifiedAtUtc, skillGap.IdentifiedAtUtc);
    }

    [Fact]
    public void Create_WithoutNotes_LeavesNotesNull()
    {
        var skillGap = SkillGap.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.Null(skillGap.Notes);
    }
}
