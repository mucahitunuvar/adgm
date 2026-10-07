using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyLogo;

internal static class GetPublicCompanyLogoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/companies/{id:guid}/logo",
                async (Guid id, HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicCompanyLogoQuery(id), cancellationToken);

                    if (result.IsFailure)
                    {
                        return result.ToProblem();
                    }

                    httpContext.Response.Headers[HeaderNames.CacheControl] = "public, max-age=3600";
                    return Results.Bytes(result.Value.Content, result.Value.ContentType);
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetPublicCompanyLogo")
            .WithTags("Employer");
    }
}
