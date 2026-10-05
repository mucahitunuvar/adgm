using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;

internal static class CreateLegalDocumentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/legal-documents",
                async (CreateLegalDocumentRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateLegalDocumentCommand(request.Key, request.Kind, request.DefaultLanguageTitle);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/legal-documents", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateLegalDocument")
            .WithTags("Website");
    }
}
