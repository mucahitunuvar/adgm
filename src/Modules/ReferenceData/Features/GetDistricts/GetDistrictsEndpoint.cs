using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetDistricts;

internal static class GetDistrictsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/reference-data/districts",
                async (
                    string? provinceId,
                    string? activeOnly,
                    string? page,
                    string? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var provinceIdResult = OptionalQueryParameter.Parse<Guid>(provinceId, nameof(provinceId));
                    if (provinceIdResult.IsFailure) return provinceIdResult.ToProblem();
                    var activeOnlyResult = OptionalQueryParameter.Parse<bool>(activeOnly, nameof(activeOnly));
                    if (activeOnlyResult.IsFailure) return activeOnlyResult.ToProblem();
                    var pageResult = OptionalQueryParameter.Parse<int>(page, nameof(page));
                    if (pageResult.IsFailure) return pageResult.ToProblem();
                    var pageSizeResult = OptionalQueryParameter.Parse<int>(pageSize, nameof(pageSize));
                    if (pageSizeResult.IsFailure) return pageSizeResult.ToProblem();

                    var query = new GetDistrictsQuery(provinceIdResult.Value, activeOnlyResult.Value ?? true)
                    {
                        Page = pageResult.Value ?? 1,
                        PageSize = pageSizeResult.Value ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .Produces<PagedResult<LookupItemSummary>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .WithName("Getdistricts")
            .WithTags("ReferenceData");
    }
}
