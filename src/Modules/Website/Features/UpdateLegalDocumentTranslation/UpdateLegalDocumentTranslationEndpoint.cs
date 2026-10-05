using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentTranslation;

internal static class UpdateLegalDocumentTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/legal-documents/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, UpdateLegalDocumentTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateLegalDocumentTranslationCommand(id, languageCode, request.RowVersion, request.Title);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateLegalDocumentTranslation")
            .WithTags("Website");
    }
}
