namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;

public sealed record CreateDevelopmentPlanRequest(Guid? SkillGapId, Guid? CareerGoalId, string Description);
