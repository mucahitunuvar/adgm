using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateAdvisorRecommendation;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment;

public static class CareerDevelopmentModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCareerDevelopmentModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateSkillGapEndpoint.Map(app);
        CreateCareerGoalEndpoint.Map(app);
        CreateAdvisorRecommendationEndpoint.Map(app);

        return app;
    }
}
