using GenclikMerkezi.Modules.Employer.Features.ApproveCompany;
using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.Modules.Employer.Features.DeactivateCompany;
using GenclikMerkezi.Modules.Employer.Features.GetCompany;
using GenclikMerkezi.Modules.Employer.Features.GetMyCompany;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Employer.Features.RejectCompany;
using GenclikMerkezi.Modules.Employer.Features.UpdateJob;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer;

public static class EmployerModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapEmployerModuleEndpoints(this IEndpointRouteBuilder app)
    {
        RegisterEmployerEndpoint.Map(app);
        ApproveCompanyEndpoint.Map(app);
        RejectCompanyEndpoint.Map(app);
        DeactivateCompanyEndpoint.Map(app);
        GetCompanyEndpoint.Map(app);
        GetMyCompanyEndpoint.Map(app);

        CreateJobEndpoint.Map(app);
        UpdateJobEndpoint.Map(app);

        return app;
    }
}
