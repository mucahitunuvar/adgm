using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentTypeTranslation;

internal static class DeleteContentTypeTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // rowVersion travels as a JSON body rather than a query parameter: ASP.NET Core's minimal API
        // query-string binding has no built-in conversion for byte[] (verified against a real request -
        // it silently bound to an empty array and FluentValidation's NotEmpty rejected it with 400
        // before the handler ever ran). [FromBody] must be explicit here: unlike POST/PUT/PATCH, a
        // MapDelete body is not inferred automatically (verified too - it threw at startup, "Body was
        // inferred but the method does not allow inferred body parameters").
        app.MapDelete(
                "/api/v1/admin/website/content-types/{id:guid}/translations/{languageCode}",
                async (Guid id, string languageCode, [FromBody] DeleteContentTypeTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteContentTypeTranslationCommand(id, languageCode, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteContentTypeTranslation")
            .WithTags("Website");
    }
}
