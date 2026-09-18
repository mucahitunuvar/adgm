using System.Security.Claims;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.ExportCandidateCvPdf;

// "Admin"/"CareerAdvisor" are literal role-claim strings, not
// GenclikMerkezi.Modules.Identity.Domain.UserRole - see SearchCandidatesEndpoint's comment for why.
// Any authenticated user may call this endpoint (unlike SearchCandidates, which is role-gated at the
// route level); the handler's own ownership check is what actually restricts a Candidate to their
// own CV, since Admin/CareerAdvisor callers must also be let through (ADR-021).
internal static class ExportCandidateCvPdfEndpoint
{
    private const string AdminRole = "Admin";
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/candidates/{candidateCvId:guid}/cv/export",
                async (Guid candidateCvId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
                {
                    var callerIsPrivileged = user.IsInRole(AdminRole) || user.IsInRole(CareerAdvisorRole);
                    var query = new ExportCandidateCvPdfQuery(candidateCvId, callerIsPrivileged);
                    var result = await sender.Send(query, cancellationToken);

                    return result.IsSuccess
                        ? Results.File(result.Value.Content, "application/pdf", result.Value.FileName)
                        : result.ToProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .WithName("ExportCandidateCvPdf")
            .WithTags("Candidate");
    }
}
