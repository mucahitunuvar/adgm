using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.UploadCompanyLogo;

internal static class UploadCompanyLogoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/employer/companies/{companyId:guid}/logo",
                async (Guid companyId, IFormFile file, ISender sender, CancellationToken cancellationToken) =>
                {
                    await using var stream = file.OpenReadStream();
                    var command = new UploadCompanyLogoCommand(companyId, stream, file.FileName, file.ContentType);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .DisableAntiforgery()
            .Produces<string>(StatusCodes.Status200OK)
            .WithName("UploadCompanyLogo")
            .WithTags("Employer");
    }
}
