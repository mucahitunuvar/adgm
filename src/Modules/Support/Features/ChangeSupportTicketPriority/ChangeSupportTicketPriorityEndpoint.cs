using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support.Features.ChangeSupportTicketPriority;

internal static class ChangeSupportTicketPriorityEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/support/tickets/{supportTicketId:guid}/priority",
                async (
                    Guid supportTicketId, ChangeSupportTicketPriorityRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new ChangeSupportTicketPriorityCommand(supportTicketId, request.Priority);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole, AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("ChangeSupportTicketPriority")
            .WithTags("Support");
    }
}
