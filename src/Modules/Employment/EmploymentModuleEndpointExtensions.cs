using GenclikMerkezi.Modules.Employment.Features.AddEmploymentNote;
using GenclikMerkezi.Modules.Employment.Features.CreateEmployment;
using GenclikMerkezi.Modules.Employment.Features.EndEmployment;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employment;

public static class EmploymentModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapEmploymentModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateEmploymentEndpoint.Map(app);
        EndEmploymentEndpoint.Map(app);
        AddEmploymentNoteEndpoint.Map(app);

        return app;
    }
}
