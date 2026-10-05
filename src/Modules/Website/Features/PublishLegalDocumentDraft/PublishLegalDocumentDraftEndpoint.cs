using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;

internal static class PublishLegalDocumentDraftEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/legal-documents/{id:guid}/versions/draft/publish",
                async (Guid id, PublishLegalDocumentDraftRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new PublishLegalDocumentDraftCommand(id, request.RowVersion, request.EffectiveAtUtc);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("PublishLegalDocumentDraft")
            .WithTags("Website");
    }
}
