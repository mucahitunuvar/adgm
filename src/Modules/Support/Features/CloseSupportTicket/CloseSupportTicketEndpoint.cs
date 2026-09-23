using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support.Features.CloseSupportTicket;

internal static class CloseSupportTicketEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/support/tickets/{supportTicketId:guid}/close",
                async (Guid supportTicketId, CloseSupportTicketRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CloseSupportTicketCommand(supportTicketId, request.Reason);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole, AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("CloseSupportTicket")
            .WithTags("Support");
    }
}
