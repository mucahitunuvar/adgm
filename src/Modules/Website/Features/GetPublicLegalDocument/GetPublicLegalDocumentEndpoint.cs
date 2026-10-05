using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocument;

internal static class GetPublicLegalDocumentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/legal-documents/{key}",
                async (string key, string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicLegalDocumentQuery(key, lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetPublicLegalDocument")
            .WithTags("Website");
    }
}
