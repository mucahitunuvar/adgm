using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteThirdPartyScriptTranslation;

internal static class DeleteThirdPartyScriptTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // rowVersion travels as a JSON body, not a query parameter - see DeletePartnerTranslationEndpoint's
        // remarks for why (byte[] query binding and inferred DELETE bodies both fail at runtime/startup
        // respectively).
        app.MapDelete(
                "/api/v1/admin/website/third-party-scripts/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, [FromBody] DeleteThirdPartyScriptTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new DeleteThirdPartyScriptTranslationCommand(id, languageCode, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteThirdPartyScriptTranslation")
            .WithTags("Website");
    }
}
