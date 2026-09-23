namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;

public sealed record CreateTrainingRecommendationRequest(Guid? DevelopmentPlanId, Guid TrainingId, string? Notes);
