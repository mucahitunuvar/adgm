using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartnerTranslation;

internal static class UpdatePartnerTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/partners/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, UpdatePartnerTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdatePartnerTranslationCommand(id, languageCode, request.RowVersion, request.Name, request.Description);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdatePartnerTranslation")
            .WithTags("Website");
    }
}
