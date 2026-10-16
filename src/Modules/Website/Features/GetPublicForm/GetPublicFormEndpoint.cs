using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicForm;

internal static class GetPublicFormEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/forms/{key}",
                async (string key, string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicFormQuery(key, lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .Produces<PublicFormDefinitionResponse>(StatusCodes.Status200OK)
            .ProducesNotFoundProblem()
            .ProducesRateLimited()
            .WithName("GetPublicForm")
            .WithTags("Website");
    }
}
