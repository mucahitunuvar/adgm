using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveCompany;

internal static class ApproveCompanyEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/companies/{companyId:guid}/approve",
                async (Guid companyId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ApproveCompanyCommand(companyId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("ApproveCompany")
            .WithTags("Employer");
    }
}
