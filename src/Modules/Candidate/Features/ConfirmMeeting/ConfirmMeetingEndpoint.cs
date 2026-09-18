using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.ConfirmMeeting;

internal static class ConfirmMeetingEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/meeting-requests/{meetingRequestId:guid}/confirm",
                async (Guid candidateCvId, Guid meetingRequestId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ConfirmMeetingCommand(candidateCvId, meetingRequestId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("ConfirmMeeting")
            .WithTags("Candidate");
    }
}
