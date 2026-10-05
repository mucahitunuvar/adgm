using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocuments;

internal static class GetLegalDocumentsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/legal-documents",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetLegalDocumentsQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetLegalDocuments")
            .WithTags("Website");
    }
}
