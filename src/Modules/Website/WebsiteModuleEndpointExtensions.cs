using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website;

public static class WebsiteModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapWebsiteModuleEndpoints(this IEndpointRouteBuilder app)
    {
        // No features yet (Faz 0 Görev 1 is scaffold-only) - each feature's own Endpoint.Map(app)
        // call is added here as it lands, same as every other module.
        return app;
    }
}
