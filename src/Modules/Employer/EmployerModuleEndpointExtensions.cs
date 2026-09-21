using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer;

public static class EmployerModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapEmployerModuleEndpoints(this IEndpointRouteBuilder app)
    {
        RegisterEmployerEndpoint.Map(app);

        return app;
    }
}
