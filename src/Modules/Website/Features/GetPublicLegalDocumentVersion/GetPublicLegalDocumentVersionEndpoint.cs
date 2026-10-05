using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocumentVersion;

internal static class GetPublicLegalDocumentVersionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/legal-documents/{key}/versions/{versionNumber:int}",
                async (string key, int versionNumber, string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicLegalDocumentVersionQuery(key, versionNumber, lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetPublicLegalDocumentVersion")
            .WithTags("Website");
    }
}
