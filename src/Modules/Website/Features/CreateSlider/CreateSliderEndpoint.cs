using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateSlider;

internal static class CreateSliderEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/sliders",
                async (CreateSliderRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateSliderCommand(request.Key, request.DefaultLanguageName);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/sliders", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateSlider")
            .WithTags("Website");
    }
}
