using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Domain;

public class CareerGoalTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var candidateCvId = Guid.NewGuid();
        var targetPositionId = Guid.NewGuid();
        var setByAdvisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var careerGoal = CareerGoal.Create(candidateCvId, "Yazılım mühendisi olmak", targetPositionId, setByAdvisorId, createdAtUtc);

        Assert.Equal(candidateCvId, careerGoal.CandidateCvId);
        Assert.Equal("Yazılım mühendisi olmak", careerGoal.Description);
        Assert.Equal(targetPositionId, careerGoal.TargetPositionId);
        Assert.Equal(setByAdvisorId, careerGoal.SetByAdvisorId);
        Assert.Equal(createdAtUtc, careerGoal.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithoutTargetPosition_LeavesTargetPositionIdNull()
    {
        var careerGoal = CareerGoal.Create(Guid.NewGuid(), "Açıklama", null, Guid.NewGuid(), DateTime.UtcNow);

        Assert.Null(careerGoal.TargetPositionId);
    }
}
