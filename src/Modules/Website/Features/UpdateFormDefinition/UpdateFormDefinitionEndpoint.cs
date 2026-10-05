using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinition;

internal static class UpdateFormDefinitionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/forms/{id:guid}",
                async (Guid id, UpdateFormDefinitionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateFormDefinitionCommand(
                        id, request.RowVersion, request.RetentionDays, request.NotificationEmails, request.PrivacyNoticeKey,
                        request.ExplicitConsents);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateFormDefinition")
            .WithTags("Website");
    }
}
