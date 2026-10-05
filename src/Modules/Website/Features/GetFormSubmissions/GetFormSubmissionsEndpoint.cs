using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissions;

internal static class GetFormSubmissionsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/form-submissions",
                async (
                    string? formKey,
                    string? status,
                    bool? archived,
                    Guid? assignedToUserId,
                    DateTime? from,
                    DateTime? to,
                    string? referenceNumber,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetFormSubmissionsQuery(
                        formKey, status, archived ?? false, assignedToUserId, from, to, referenceNumber)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            // ADR-024 §12.2: the list never returns personal data, so unlike the detail/file-download
            // endpoints below, no PersonalDataAccessLog entry is written for it.
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .WithName("GetFormSubmissions")
            .WithTags("Website");
    }
}
