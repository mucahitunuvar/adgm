using GenclikMerkezi.Modules.Employment.Features.CreateEmployment;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employment;

public static class EmploymentModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapEmploymentModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateEmploymentEndpoint.Map(app);

        return app;
    }
}
