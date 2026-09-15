using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.ChangePassword;

internal static class ChangePasswordEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/auth/change-password", async (ChangePasswordRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
                var result = await sender.Send(command, cancellationToken);

                return result.ToNoContentOrProblem();
            })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("ChangePassword")
            .WithTags("Auth");
    }
}
