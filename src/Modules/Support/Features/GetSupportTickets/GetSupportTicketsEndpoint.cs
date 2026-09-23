using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTickets;

internal static class GetSupportTicketsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/support/tickets",
                async (int? page, int? pageSize, ISender sender, CancellationToken cancellationToken) =>
                {
                    var query = new GetSupportTicketsQuery
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("GetSupportTickets")
            .WithTags("Support");
    }
}
