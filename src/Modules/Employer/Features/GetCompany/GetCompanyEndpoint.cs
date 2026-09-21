using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetCompany;

internal static class GetCompanyEndpoint
{
    private const string AdminRole = "Admin";
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/companies/{companyId:guid}",
                async (Guid companyId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetCompanyQuery(companyId), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole, CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .Produces<GetCompanyResponse>(StatusCodes.Status200OK)
            .WithName("GetCompany")
            .WithTags("Employer");
    }
}
