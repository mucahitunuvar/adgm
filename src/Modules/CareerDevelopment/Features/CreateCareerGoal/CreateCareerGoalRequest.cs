namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;

public sealed record CreateCareerGoalRequest(string Description, Guid? TargetPositionId);
