using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support.Features.AddSupportTicketMessage;

internal static class AddSupportTicketMessageEndpoint
{
    private const string CandidateRole = "Candidate";
    private const string EmployerRole = "Employer";
    private const string CareerAdvisorRole = "CareerAdvisor";
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/support/tickets/{supportTicketId:guid}/messages",
                async (Guid supportTicketId, AddSupportTicketMessageRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new AddSupportTicketMessageCommand(supportTicketId, request.Content);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/support/tickets/{supportTicketId}/messages", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CandidateRole, EmployerRole, CareerAdvisorRole, AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("AddSupportTicketMessage")
            .WithTags("Support");
    }
}
