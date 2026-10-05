using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinitionTranslation;

internal static class UpdateFormDefinitionTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/forms/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, UpdateFormDefinitionTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateFormDefinitionTranslationCommand(
                        id, languageCode, request.RowVersion, request.Title, request.Description, request.SuccessMessage,
                        request.SubmitButtonLabel);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateFormDefinitionTranslation")
            .WithTags("Website");
    }
}
