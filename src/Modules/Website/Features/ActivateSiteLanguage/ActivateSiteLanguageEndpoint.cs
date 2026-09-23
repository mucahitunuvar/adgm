using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;

internal static class ActivateSiteLanguageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/languages/{id:guid}/activate",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ActivateSiteLanguageCommand(id), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("ActivateSiteLanguage")
            .WithTags("Website");
    }
}
