using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;

// "CareerAdvisor" literal rol-claim string'i, GenclikMerkezi.Modules.Identity.Domain.UserRole değil
// - Candidate başka bir modülün Domain tipine bağımlı olamaz (AGENTS.md §9/§12,
// ModuleBoundaryTests), SearchCandidatesEndpoint'teki aynı yaklaşım.
internal static class SendBulkCandidateNotificationEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/bulk-notifications",
                async (SendBulkCandidateNotificationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new SendBulkCandidateNotificationCommand(
                        request.CandidateCvIds, request.Subject, request.Message);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("SendBulkCandidateNotification")
            .WithTags("Candidate");
    }
}
