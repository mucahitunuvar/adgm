using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

public sealed record AdvisorRecommendationItemResponse(Guid Id, Guid AdvisorId, string Content, DateTime CreatedAtUtc)
{
    public static AdvisorRecommendationItemResponse FromDomain(AdvisorRecommendation recommendation) =>
        new(recommendation.Id, recommendation.AdvisorId, recommendation.Content, recommendation.CreatedAtUtc);
}
