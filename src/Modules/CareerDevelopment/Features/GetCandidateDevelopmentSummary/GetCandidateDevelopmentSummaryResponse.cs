namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

public sealed record GetCandidateDevelopmentSummaryResponse(
    IReadOnlyList<SkillGapItemResponse> SkillGaps,
    IReadOnlyList<CareerGoalItemResponse> CareerGoals,
    IReadOnlyList<DevelopmentPlanItemResponse> DevelopmentPlans,
    IReadOnlyList<TrainingRecommendationItemResponse> TrainingRecommendations,
    IReadOnlyList<AdvisorRecommendationItemResponse> AdvisorRecommendations);
