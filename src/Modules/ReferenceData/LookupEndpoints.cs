using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;
using GenclikMerkezi.Modules.ReferenceData.Features.GetLookupItems;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.ReferenceData;

// ADR-016 Decision 3's generic endpoint-mapping helper: each lookup type gets its own explicit,
// individually-named route (not one parameterized "/lookups/{type}" route), so every one of the
// 23 lookup types shows up as its own path in the generated OpenAPI document.
//
// "Admin" is a literal role-claim string, not GenclikMerkezi.Modules.Identity.Domain.UserRole:
// ReferenceData must not depend on another module's Domain types (AGENTS.md §9/§12,
// ModuleBoundaryTests) even though both modules agree on what the string "Admin" means at runtime.
internal static class LookupEndpoints
{
    private const string BasePath = "/api/v1/reference-data";
    private const string AdminRole = "Admin";

    public static void MapReadOnly(IEndpointRouteBuilder app, string routeSegment, ReferenceDataLookupType lookupType)
    {
        app.MapGet(
                $"{BasePath}/{routeSegment}",
                async (bool? activeOnly, ISender sender, CancellationToken cancellationToken) =>
                {
                    var query = new GetLookupItemsQuery(lookupType, activeOnly ?? true);
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .WithName($"Get{routeSegment}")
            .WithTags("ReferenceData");
    }

    public static void MapWithCrud<TLookup>(IEndpointRouteBuilder app, string routeSegment, ReferenceDataLookupType lookupType)
        where TLookup : LookupItem, ILookupItemFactory<TLookup>
    {
        MapReadOnly(app, routeSegment, lookupType);

        app.MapPost(
                $"{BasePath}/{routeSegment}",
                async (CreateLookupItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateLookupItemCommand<TLookup>(request.Code, request.DisplayName, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"{BasePath}/{routeSegment}/{result.Value}", new { id = result.Value })
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName($"Create{routeSegment}")
            .WithTags("ReferenceData");

        app.MapPut(
                $"{BasePath}/{routeSegment}/{{id:guid}}",
                async (Guid id, UpdateLookupItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateLookupItemCommand<TLookup>(id, request.DisplayName, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName($"Update{routeSegment}")
            .WithTags("ReferenceData");

        // Soft-delete only (ADR-016 Decision 1): other modules may already reference this row's id.
        app.MapDelete(
                $"{BasePath}/{routeSegment}/{{id:guid}}",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new DeactivateLookupItemCommand<TLookup>(id);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName($"Deactivate{routeSegment}")
            .WithTags("ReferenceData");
    }
}
