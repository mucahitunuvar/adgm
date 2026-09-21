using GenclikMerkezi.Modules.Employer.Features.ApproveCompany;
using GenclikMerkezi.Modules.Employer.Features.ApproveJob;
using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;
using GenclikMerkezi.Modules.Employer.Features.DeactivateCompany;
using GenclikMerkezi.Modules.Employer.Features.GetCompany;
using GenclikMerkezi.Modules.Employer.Features.GetJobsPendingReview;
using GenclikMerkezi.Modules.Employer.Features.GetMyCompany;
using GenclikMerkezi.Modules.Employer.Features.GetMyCompanyJobs;
using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Employer.Features.RejectCompany;
using GenclikMerkezi.Modules.Employer.Features.RejectJob;
using GenclikMerkezi.Modules.Employer.Features.RequestJobRevision;
using GenclikMerkezi.Modules.Employer.Features.SubmitJobForReview;
using GenclikMerkezi.Modules.Employer.Features.UpdateJob;
using GenclikMerkezi.Modules.Employer.Features.UpdatePersonnelNeed;
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
        SubmitJobForReviewEndpoint.Map(app);
        ApproveJobEndpoint.Map(app);
        RejectJobEndpoint.Map(app);
        RequestJobRevisionEndpoint.Map(app);
        GetPublishedJobsEndpoint.Map(app);
        GetMyCompanyJobsEndpoint.Map(app);
        GetJobsPendingReviewEndpoint.Map(app);

        CreatePersonnelNeedEndpoint.Map(app);
        UpdatePersonnelNeedEndpoint.Map(app);

        return app;
    }
}
