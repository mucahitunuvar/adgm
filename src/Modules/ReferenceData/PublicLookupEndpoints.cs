using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Features.GetPublicLookupItems;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.ReferenceData;

// Madde 39 (Faz 3 Koşu 3C): /public/jobs filtre dropdown'larının (il/pozisyon/departman/çalışma
// şekli/çalışma yeri tipi) ihtiyaç duyduğu 5 set - yalnız bunlar, ReferenceData'nın tüm kataloğu
// değil. Zaten var olan AllowAnonymous /api/v1/reference-data/* uçlarından (LookupEndpoints.MapReadOnly)
// kasıtlı olarak ayrı: onlar Code/IsActive/SortOrder taşır ve hiçbir rate limiting policy'sine tabi
// değildir (dokunulmayan, önceden var olan bir tutarsızlık - bkz. tamamlanma raporu). Bu public aile
// ise public-read rate limiting + dar id/ad şekliyle FRONTEND-API-SOZLESMESI.md §3'teki diğer
// /public/* uçlarıyla aynı sözleşmeye uyar.
internal static class PublicLookupEndpoints
{
    private const string BasePath = "/api/v1/public/reference-data";

    public static void Map(IEndpointRouteBuilder app, string routeSegment, ReferenceDataLookupType lookupType)
    {
        app.MapGet(
                $"{BasePath}/{routeSegment}",
                async (
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetPublicLookupItemsQuery(lookupType)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .Produces<PagedResult<PublicLookupItemResponse>>(StatusCodes.Status200OK)
            .ProducesRateLimited()
            .WithName($"GetPublic{routeSegment}")
            .WithTags("ReferenceData");
    }
}
