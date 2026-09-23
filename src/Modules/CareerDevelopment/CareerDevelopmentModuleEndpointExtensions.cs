using GenclikMerkezi.Modules.CareerDevelopment.Features.CancelDevelopmentPlan;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CompleteDevelopmentPlan;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateAdvisorRecommendation;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;
using GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment;

public static class CareerDevelopmentModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCareerDevelopmentModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateSkillGapEndpoint.Map(app);
        CreateCareerGoalEndpoint.Map(app);
        CreateAdvisorRecommendationEndpoint.Map(app);
        CreateDevelopmentPlanEndpoint.Map(app);
        CreateTrainingRecommendationEndpoint.Map(app);
        CompleteDevelopmentPlanEndpoint.Map(app);
        CancelDevelopmentPlanEndpoint.Map(app);
        GetCandidateDevelopmentSummaryEndpoint.Map(app);

        return app;
    }
}
