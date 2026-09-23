using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support.Features.CreateSupportTicket;

internal static class CreateSupportTicketEndpoint
{
    private const string CandidateRole = "Candidate";
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/support/tickets",
                async (CreateSupportTicketRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateSupportTicketCommand(request.Subject, request.Priority);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created("/api/v1/support/tickets", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CandidateRole, EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateSupportTicket")
            .WithTags("Support");
    }
}
