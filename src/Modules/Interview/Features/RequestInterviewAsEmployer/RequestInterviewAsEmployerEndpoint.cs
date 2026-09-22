using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsEmployer;

internal static class RequestInterviewAsEmployerEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/employer/interviews",
                async (RequestInterviewAsEmployerRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new RequestInterviewAsEmployerCommand(request.CandidateCvId);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created("/api/v1/employer/interviews", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("RequestInterviewAsEmployer")
            .WithTags("Interview");
    }
}
