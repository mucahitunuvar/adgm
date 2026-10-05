using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteLegalDocument;

internal static class DeleteLegalDocumentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/admin/website/legal-documents/{id:guid}",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeleteLegalDocumentCommand(id), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteLegalDocument")
            .WithTags("Website");
    }
}
