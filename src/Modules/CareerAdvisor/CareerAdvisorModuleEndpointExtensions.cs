using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor;

public static class CareerAdvisorModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCareerAdvisorModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateCareerAdvisorEndpoint.Map(app);
        DeactivateCareerAdvisorEndpoint.Map(app);

        return app;
    }
}
