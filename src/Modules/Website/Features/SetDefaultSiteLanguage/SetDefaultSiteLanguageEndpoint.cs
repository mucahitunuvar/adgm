using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;

internal static class SetDefaultSiteLanguageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/languages/{id:guid}/set-default",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new SetDefaultSiteLanguageCommand(id), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("SetDefaultSiteLanguage")
            .WithTags("Website");
    }
}
