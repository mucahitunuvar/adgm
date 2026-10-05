using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteLegalDocumentDraft;

internal static class DeleteLegalDocumentDraftEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // rowVersion travels as a JSON body, not a query parameter - see DeleteVideoTranslationEndpoint's
        // remarks for why (byte[] query binding and inferred DELETE bodies both fail at runtime/startup
        // respectively).
        app.MapDelete(
                "/api/v1/admin/website/legal-documents/{id:guid}/versions/draft",
                async (Guid id, [FromBody] DeleteLegalDocumentDraftRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteLegalDocumentDraftCommand(id, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteLegalDocumentDraft")
            .WithTags("Website");
    }
}
