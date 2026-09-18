using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;

internal static class CreateCareerAdvisorEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/career-advisors", async (CreateCareerAdvisorRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CreateCareerAdvisorCommand(
                    request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber);
                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/admin/career-advisors/{result.Value.CareerAdvisorId}", result.Value)
                    : result.ToProblem();
            })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateCareerAdvisor")
            .WithTags("CareerAdvisor");
    }
}
