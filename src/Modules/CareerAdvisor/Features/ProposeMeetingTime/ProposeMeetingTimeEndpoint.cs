using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.ProposeMeetingTime;

internal static class ProposeMeetingTimeEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/career-advisor/meeting-requests/{meetingRequestId:guid}/propose-time",
                async (Guid meetingRequestId, ProposeMeetingTimeRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new ProposeMeetingTimeCommand(meetingRequestId, request.ProposedDateTimeUtc);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("ProposeMeetingTime")
            .WithTags("CareerAdvisor");
    }
}
