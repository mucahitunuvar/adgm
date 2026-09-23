using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

public sealed record CareerGoalItemResponse(
    Guid Id, string Description, Guid? TargetPositionId, Guid SetByAdvisorId, DateTime CreatedAtUtc)
{
    public static CareerGoalItemResponse FromDomain(CareerGoal careerGoal) =>
        new(careerGoal.Id, careerGoal.Description, careerGoal.TargetPositionId, careerGoal.SetByAdvisorId, careerGoal.CreatedAtUtc);
}
