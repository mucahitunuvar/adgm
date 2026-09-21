using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetGeneralPool;

internal static class GetGeneralPoolEndpoint
{
    private const string AdminRole = "Admin";
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/career-advisor/general-pool",
                async (int? page, int? pageSize, ISender sender, CancellationToken cancellationToken) =>
                {
                    var query = new GetGeneralPoolQuery
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole, CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("GetGeneralPool")
            .WithTags("CareerAdvisor");
    }
}
