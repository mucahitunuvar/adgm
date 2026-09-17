using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidateCvs;

// "Admin"/"CareerAdvisor" are literal role-claim strings, not
// GenclikMerkezi.Modules.Identity.Domain.UserRole: Candidate must not depend on another module's
// Domain types (AGENTS.md §9/§12, ModuleBoundaryTests) even though both modules agree on what these
// strings mean at runtime - the same approach ReferenceData's LookupEndpoints already uses.
internal static class SearchCandidateCvsEndpoint
{
    private const string AdminRole = "Admin";
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/candidates",
                async (
                    string? searchText,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new SearchCandidateCvsQuery(searchText)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole, CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .Produces<SearchCandidateCvsResponse>(StatusCodes.Status200OK)
            .WithName("SearchCandidateCvs")
            .WithTags("Candidate");
    }
}
