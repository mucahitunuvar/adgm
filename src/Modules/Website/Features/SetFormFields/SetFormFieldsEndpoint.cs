using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SetFormFields;

internal static class SetFormFieldsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/forms/{id:guid}/fields",
                async (Guid id, SetFormFieldsRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new SetFormFieldsCommand(id, request.RowVersion, request.Fields);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("SetFormFields")
            .WithTags("Website");
    }
}
