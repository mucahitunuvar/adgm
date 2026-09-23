using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support.Features.TransferSupportTicket;

internal static class TransferSupportTicketEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/support/tickets/{supportTicketId:guid}/transfer",
                async (Guid supportTicketId, TransferSupportTicketRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new TransferSupportTicketCommand(supportTicketId, request.NewAssigneeUserId);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole, AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("TransferSupportTicket")
            .WithTags("Support");
    }
}
