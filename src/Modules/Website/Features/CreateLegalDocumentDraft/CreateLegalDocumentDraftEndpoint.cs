using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;

internal static class CreateLegalDocumentDraftEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/legal-documents/{id:guid}/versions/draft",
                async (Guid id, CreateLegalDocumentDraftRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateLegalDocumentDraftCommand(id, request.RowVersion, request.ChangeSummary);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created($"/api/v1/admin/website/legal-documents/{id}", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateLegalDocumentDraft")
            .WithTags("Website");
    }
}
