using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateFormDefinition;

internal static class CreateFormDefinitionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/forms",
                async (CreateFormDefinitionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateFormDefinitionCommand(
                        request.Key, request.RetentionDays, request.NotificationEmails, request.PrivacyNoticeKey, request.ExplicitConsents,
                        request.DefaultLanguageTitle, request.DefaultLanguageDescription, request.DefaultLanguageSuccessMessage,
                        request.DefaultLanguageSubmitButtonLabel);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/forms", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateFormDefinition")
            .WithTags("Website");
    }
}
