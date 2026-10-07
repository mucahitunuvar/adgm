using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.SetCompanyLogoVisibility;

internal static class SetCompanyLogoVisibilityEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/employer/companies/me/logo-visibility",
                async (SetCompanyLogoVisibilityRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new SetCompanyLogoVisibilityCommand(request.ShowLogoOnWebsite, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("SetCompanyLogoVisibility")
            .WithTags("Employer");
    }
}
