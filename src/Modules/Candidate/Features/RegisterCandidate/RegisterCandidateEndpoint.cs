using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;

internal static class RegisterCandidateEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/candidates/register", async (RegisterCandidateRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RegisterCandidateCommand(
                    request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber);
                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/candidates/{result.Value.CandidateCvId}", result.Value)
                    : result.ToProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("RegisterCandidate")
            .WithTags("Candidate");
    }
}
