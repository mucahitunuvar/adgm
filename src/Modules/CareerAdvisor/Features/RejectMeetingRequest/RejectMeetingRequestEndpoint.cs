using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.RejectMeetingRequest;

internal static class RejectMeetingRequestEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/career-advisor/meeting-requests/{meetingRequestId:guid}/reject",
                async (Guid meetingRequestId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RejectMeetingRequestCommand(meetingRequestId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("RejectMeetingRequest")
            .WithTags("CareerAdvisor");
    }
}
