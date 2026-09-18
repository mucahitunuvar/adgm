using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidates;

// "Admin"/"CareerAdvisor" are literal role-claim strings, not
// GenclikMerkezi.Modules.Identity.Domain.UserRole: Candidate must not depend on another module's
// Domain types (AGENTS.md §9/§12, ModuleBoundaryTests) even though both modules agree on what these
// strings mean at runtime - the same approach ReferenceData's LookupEndpoints already uses.
//
// CareerAdvisor assignment doesn't exist yet (ADR-020): until it does, "CareerAdvisor" sees the same
// unfiltered candidate pool "Admin" does - there is no per-advisor scoping to apply.
internal static class SearchCandidatesEndpoint
{
    private const string AdminRole = "Admin";
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/candidates",
                async (
                    string? searchText,
                    Guid? provinceId,
                    Guid? districtId,
                    Guid? educationLevelId,
                    Guid? sectorId,
                    int? minCompletionPercentage,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new SearchCandidatesQuery(
                        searchText, provinceId, districtId, educationLevelId, sectorId, minCompletionPercentage)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole, CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .Produces<SearchCandidatesResponse>(StatusCodes.Status200OK)
            .WithName("SearchCandidates")
            .WithTags("Candidate");
    }
}
