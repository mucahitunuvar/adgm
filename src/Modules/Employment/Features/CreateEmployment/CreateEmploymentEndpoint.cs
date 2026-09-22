using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employment.Features.CreateEmployment;

internal static class CreateEmploymentEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/employments",
                async (CreateEmploymentRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateEmploymentCommand(
                        request.CandidateCvId, request.CompanyId, request.PositionId, request.InterviewId, request.StartDateUtc);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created("/api/v1/career-advisor/employments", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateEmployment")
            .WithTags("Employment");
    }
}
