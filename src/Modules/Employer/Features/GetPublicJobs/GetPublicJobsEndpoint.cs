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
                    Guid? provinceId,
                    Guid? employmentTypeId,
                    Guid? workLocationTypeId,
                    Guid? positionId,
                    Guid? departmentId,
                    Guid? companyId,
                    bool? isForDisabledCandidates,
                    string? q,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetPublicJobsQuery(
                        provinceId, employmentTypeId, workLocationTypeId, positionId, departmentId, companyId,
                        isForDisabledCandidates, q)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetPublicJobs")
            .WithTags("Employer");
    }
}
