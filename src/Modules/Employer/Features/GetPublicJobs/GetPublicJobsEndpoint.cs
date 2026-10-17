using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;

internal static class GetPublicJobsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/jobs",
                async (
                    string? provinceId,
                    string? employmentTypeId,
                    string? workLocationTypeId,
                    string? positionId,
                    string? departmentId,
                    string? companyId,
                    string? isForDisabledCandidates,
                    string? q,
                    string? page,
                    string? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    // Bound as raw strings, not Guid?/bool?/int?, so an empty value means "filter not
                    // applied" rather than crashing - see OptionalQueryParameter's own comment.
                    var provinceIdResult = OptionalQueryParameter.Parse<Guid>(provinceId, nameof(provinceId));
                    if (provinceIdResult.IsFailure) return provinceIdResult.ToProblem();
                    var employmentTypeIdResult = OptionalQueryParameter.Parse<Guid>(employmentTypeId, nameof(employmentTypeId));
                    if (employmentTypeIdResult.IsFailure) return employmentTypeIdResult.ToProblem();
                    var workLocationTypeIdResult = OptionalQueryParameter.Parse<Guid>(workLocationTypeId, nameof(workLocationTypeId));
                    if (workLocationTypeIdResult.IsFailure) return workLocationTypeIdResult.ToProblem();
                    var positionIdResult = OptionalQueryParameter.Parse<Guid>(positionId, nameof(positionId));
                    if (positionIdResult.IsFailure) return positionIdResult.ToProblem();
                    var departmentIdResult = OptionalQueryParameter.Parse<Guid>(departmentId, nameof(departmentId));
                    if (departmentIdResult.IsFailure) return departmentIdResult.ToProblem();
                    var companyIdResult = OptionalQueryParameter.Parse<Guid>(companyId, nameof(companyId));
                    if (companyIdResult.IsFailure) return companyIdResult.ToProblem();
                    var isForDisabledCandidatesResult = OptionalQueryParameter.Parse<bool>(isForDisabledCandidates, nameof(isForDisabledCandidates));
                    if (isForDisabledCandidatesResult.IsFailure) return isForDisabledCandidatesResult.ToProblem();
                    var pageResult = OptionalQueryParameter.Parse<int>(page, nameof(page));
                    if (pageResult.IsFailure) return pageResult.ToProblem();
                    var pageSizeResult = OptionalQueryParameter.Parse<int>(pageSize, nameof(pageSize));
                    if (pageSizeResult.IsFailure) return pageSizeResult.ToProblem();

                    var query = new GetPublicJobsQuery(
                        provinceIdResult.Value, employmentTypeIdResult.Value, workLocationTypeIdResult.Value, positionIdResult.Value,
                        departmentIdResult.Value, companyIdResult.Value, isForDisabledCandidatesResult.Value, q)
                    {
                        Page = pageResult.Value ?? 1,
                        PageSize = pageSizeResult.Value ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .Produces<PagedResult<PublicJobListItemResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesRateLimited()
            .WithName("GetPublicJobs")
            .WithTags("Employer");
    }
}
