using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.DeactivateCompany;

internal static class DeactivateCompanyEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/companies/{companyId:guid}/deactivate",
                async (Guid companyId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivateCompanyCommand(companyId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("DeactivateCompany")
            .WithTags("Employer");
    }
}
