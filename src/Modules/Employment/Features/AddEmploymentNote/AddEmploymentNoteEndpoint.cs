using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employment.Features.AddEmploymentNote;

internal static class AddEmploymentNoteEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/employments/{employmentId:guid}/notes",
                async (Guid employmentId, AddEmploymentNoteRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new AddEmploymentNoteCommand(employmentId, request.Content);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/career-advisor/employments/{employmentId}/notes", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("AddEmploymentNote")
            .WithTags("Employment");
    }
}
