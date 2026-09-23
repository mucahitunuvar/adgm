using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

public sealed record TrainingRecommendationItemResponse(
    Guid Id, Guid? DevelopmentPlanId, Guid TrainingId, Guid RecommendedByAdvisorId, string? Notes, DateTime RecommendedAtUtc)
{
    public static TrainingRecommendationItemResponse FromDomain(TrainingRecommendation recommendation) => new(
        recommendation.Id, recommendation.DevelopmentPlanId, recommendation.TrainingId,
        recommendation.RecommendedByAdvisorId, recommendation.Notes, recommendation.RecommendedAtUtc);
}
