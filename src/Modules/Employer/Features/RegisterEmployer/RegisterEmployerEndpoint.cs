using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;

internal static class RegisterEmployerEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/employer/register", async (RegisterEmployerRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RegisterEmployerCommand(
                    request.Email,
                    request.Password,
                    request.Name,
                    request.SectorId,
                    request.FoundedYear,
                    request.EmployeeCount,
                    request.WebsiteUrl,
                    request.CountryId,
                    request.ProvinceId,
                    request.DistrictId,
                    request.Address,
                    request.AboutHtml,
                    request.ContactFirstName,
                    request.ContactLastName,
                    request.ContactPhone,
                    request.TaxOfficeId,
                    request.TaxNumber,
                    request.MarketingConsent);
                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/admin/companies/{result.Value.CompanyId}", result.Value)
                    : result.ToProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("RegisterEmployer")
            .WithTags("Employer");
    }
}
