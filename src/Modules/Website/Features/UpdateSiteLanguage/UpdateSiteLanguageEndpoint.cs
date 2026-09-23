using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;

internal static class UpdateSiteLanguageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/languages/{id:guid}",
                async (Guid id, UpdateSiteLanguageRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateSiteLanguageCommand(id, request.Name, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateSiteLanguage")
            .WithTags("Website");
    }
}
