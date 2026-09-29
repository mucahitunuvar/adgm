using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteVideoTranslation;

internal static class DeleteVideoTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // rowVersion travels as a JSON body, not a query parameter - see
        // DeleteContentTypeTranslationEndpoint's remarks for why (byte[] query binding and inferred
        // DELETE bodies both fail at runtime/startup respectively).
        app.MapDelete(
                "/api/v1/admin/website/videos/{id:guid}/translations/{languageCode}",
                async (Guid id, string languageCode, [FromBody] DeleteVideoTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteVideoTranslationCommand(id, languageCode, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteVideoTranslation")
            .WithTags("Website");
    }
}
