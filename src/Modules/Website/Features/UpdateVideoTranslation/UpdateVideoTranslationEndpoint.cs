using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateVideoTranslation;

internal static class UpdateVideoTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/videos/{id:guid}/translations/{languageCode}",
                async (Guid id, string languageCode, UpdateVideoTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateVideoTranslationCommand(id, languageCode, request.RowVersion, request.Title, request.Description);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateVideoTranslation")
            .WithTags("Website");
    }
}
