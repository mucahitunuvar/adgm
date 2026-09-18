using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.RequestMeeting;

internal static class RequestMeetingEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/meeting-requests",
                async (Guid candidateCvId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RequestMeetingCommand(candidateCvId), cancellationToken);

                    return result.IsSuccess
                        ? Results.Created(
                            $"/api/v1/candidates/{candidateCvId}/meeting-requests/{result.Value.MeetingRequestId}", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("RequestMeeting")
            .WithTags("Candidate");
    }
}
