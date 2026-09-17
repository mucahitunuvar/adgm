using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Contracts.ReferenceData;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

// TaxOffice does not implement ILookupItemFactory<TaxOffice> (it needs a ProvinceId the shared
// factory has no slot for), so it does not use LookupEndpoints.MapWithCrud - only its own read
// route via LookupEndpoints.MapReadOnly, plus these three bespoke mutation routes.
internal static class TaxOfficeEndpoints
{
    private const string BasePath = "/api/v1/reference-data/tax-offices";
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        LookupEndpoints.MapReadOnly(app, "tax-offices", ReferenceDataLookupType.TaxOffice);

        app.MapPost(
                BasePath,
                async (CreateTaxOfficeRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateTaxOfficeCommand(request.Code, request.DisplayName, request.SortOrder, request.ProvinceId);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"{BasePath}/{result.Value}", new { id = result.Value })
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateTaxOffice")
            .WithTags("ReferenceData");

        app.MapPut(
                $"{BasePath}/{{id:guid}}",
                async (Guid id, UpdateTaxOfficeRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateTaxOfficeCommand(id, request.DisplayName, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("UpdateTaxOffice")
            .WithTags("ReferenceData");

        app.MapDelete(
                $"{BasePath}/{{id:guid}}",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivateTaxOfficeCommand(id), cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("DeactivateTaxOffice")
            .WithTags("ReferenceData");
    }
}
