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
// kasıtlı olarak ayrı: onlar Code/IsActive/SortOrder taşır (bu public aile dar id/ad şekli taşır).
// İkisi de aynı "public-read" rate limiting politikasına bağlıdır - LookupEndpoints.MapReadOnly
// önceden bu politikaya tabi değildi (dokunulmayan, önceden var olan bir tutarsızlık), sonradan
// kapatıldı. Bu public aile ise ayrıca FRONTEND-API-SOZLESMESI.md §3'teki diğer /public/* uçlarıyla
// aynı dar şekil sözleşmesine uyar.
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
