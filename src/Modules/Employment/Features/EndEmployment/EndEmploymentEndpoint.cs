using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employment.Features.EndEmployment;

internal static class EndEmploymentEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/employments/{employmentId:guid}/end",
                async (Guid employmentId, EndEmploymentRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new EndEmploymentCommand(employmentId, request.DepartureReason, request.EndDateUtc);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("EndEmployment")
            .WithTags("Employment");
    }
}
