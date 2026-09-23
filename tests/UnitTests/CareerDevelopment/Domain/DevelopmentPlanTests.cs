using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Domain;

public class DevelopmentPlanTests
{
    private static DevelopmentPlan CreatePlan(Guid? skillGapId = null, Guid? careerGoalId = null) =>
        DevelopmentPlan.Create(Guid.NewGuid(), skillGapId, careerGoalId, "İngilizce seviyesini artır", Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToAktif()
    {
        var candidateCvId = Guid.NewGuid();
        var skillGapId = Guid.NewGuid();
        var careerGoalId = Guid.NewGuid();
        var createdByAdvisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var plan = DevelopmentPlan.Create(candidateCvId, skillGapId, careerGoalId, "Açıklama", createdByAdvisorId, createdAtUtc);

        Assert.Equal(DevelopmentPlanStatus.Aktif, plan.Status);
        Assert.Equal(candidateCvId, plan.CandidateCvId);
        Assert.Equal(skillGapId, plan.SkillGapId);
        Assert.Equal(careerGoalId, plan.CareerGoalId);
        Assert.Equal("Açıklama", plan.Description);
        Assert.Equal(createdByAdvisorId, plan.CreatedByAdvisorId);
        Assert.Equal(createdAtUtc, plan.CreatedAtUtc);
        Assert.Null(plan.CompletedAtUtc);
    }

    [Fact]
    public void Complete_FromAktif_Succeeds()
    {
        var plan = CreatePlan();
        var completedAtUtc = DateTime.UtcNow.AddDays(30);

        var result = plan.Complete(completedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(DevelopmentPlanStatus.Tamamlandi, plan.Status);
        Assert.Equal(completedAtUtc, plan.CompletedAtUtc);
    }

    [Fact]
    public void Complete_FromNonAktifStatus_Fails()
    {
        var plan = CreatePlan();
        plan.Complete(DateTime.UtcNow);

        var result = plan.Complete(DateTime.UtcNow.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Equal(DevelopmentPlanStatus.Tamamlandi, plan.Status);
    }

    [Fact]
    public void Cancel_FromAktif_Succeeds()
    {
        var plan = CreatePlan();

        var result = plan.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(DevelopmentPlanStatus.IptalEdildi, plan.Status);
    }

    [Fact]
    public void Cancel_FromNonAktifStatus_Fails()
    {
        var plan = CreatePlan();
        plan.Cancel();

        var result = plan.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal(DevelopmentPlanStatus.IptalEdildi, plan.Status);
    }
}
