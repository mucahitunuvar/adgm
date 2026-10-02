using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreatePartner;

internal static class CreatePartnerEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/partners",
                async (CreatePartnerRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreatePartnerCommand(
                        request.LogoMediaId, request.WebsiteUrl, request.SortOrder,
                        request.DefaultLanguageName, request.DefaultLanguageDescription);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/partners", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreatePartner")
            .WithTags("Website");
    }
}
