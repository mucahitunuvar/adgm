using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;

internal static class CreateSiteLanguageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/languages",
                async (CreateSiteLanguageRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateSiteLanguageCommand(request.Code, request.Name, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/languages", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateSiteLanguage")
            .WithTags("Website");
    }
}
