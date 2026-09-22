using GenclikMerkezi.Modules.Employment.Features.AddEmploymentNote;
using GenclikMerkezi.Modules.Employment.Features.CreateEmployment;
using GenclikMerkezi.Modules.Employment.Features.EndEmployment;
using GenclikMerkezi.Modules.Employment.Features.GetEmploymentNotes;
using GenclikMerkezi.Modules.Employment.Features.GetEmploymentsForCandidate;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employment;

public static class EmploymentModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapEmploymentModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateEmploymentEndpoint.Map(app);
        EndEmploymentEndpoint.Map(app);
        AddEmploymentNoteEndpoint.Map(app);
        GetEmploymentsForCandidateEndpoint.Map(app);
        GetEmploymentNotesEndpoint.Map(app);

        return app;
    }
}
