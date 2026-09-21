using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.RejectCompany;

internal static class RejectCompanyEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/companies/{companyId:guid}/reject",
                async (Guid companyId, RejectCompanyRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RejectCompanyCommand(companyId, request.Reason), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("RejectCompany")
            .WithTags("Employer");
    }
}
