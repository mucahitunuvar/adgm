using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;

internal static class UpdateLegalDocumentDraftBodyEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/legal-documents/{id:guid}/versions/draft/translations/{languageCode}",
                async (
                    Guid id, string languageCode, UpdateLegalDocumentDraftBodyRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateLegalDocumentDraftBodyCommand(id, languageCode, request.RowVersion, request.Body);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateLegalDocumentDraftBody")
            .WithTags("Website");
    }
}
