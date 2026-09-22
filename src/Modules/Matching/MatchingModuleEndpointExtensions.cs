using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Matching;

public static class MatchingModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapMatchingModuleEndpoints(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
