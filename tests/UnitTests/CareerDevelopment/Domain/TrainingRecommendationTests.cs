using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Domain;

public class TrainingRecommendationTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var candidateCvId = Guid.NewGuid();
        var developmentPlanId = Guid.NewGuid();
        var trainingId = Guid.NewGuid();
        var recommendedByAdvisorId = Guid.NewGuid();
        var recommendedAtUtc = DateTime.UtcNow;

        var recommendation = TrainingRecommendation.Create(
            candidateCvId, developmentPlanId, trainingId, recommendedByAdvisorId, "Uygun bir eğitim", recommendedAtUtc);

        Assert.Equal(candidateCvId, recommendation.CandidateCvId);
        Assert.Equal(developmentPlanId, recommendation.DevelopmentPlanId);
        Assert.Equal(trainingId, recommendation.TrainingId);
        Assert.Equal(recommendedByAdvisorId, recommendation.RecommendedByAdvisorId);
        Assert.Equal("Uygun bir eğitim", recommendation.Notes);
        Assert.Equal(recommendedAtUtc, recommendation.RecommendedAtUtc);
    }

    [Fact]
    public void Create_WithoutDevelopmentPlanId_LeavesItNull()
    {
        var recommendation = TrainingRecommendation.Create(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.Null(recommendation.DevelopmentPlanId);
    }
}
