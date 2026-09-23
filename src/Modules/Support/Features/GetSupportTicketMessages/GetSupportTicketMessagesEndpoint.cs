using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTicketMessages;

internal static class GetSupportTicketMessagesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/support/tickets/{supportTicketId:guid}/messages",
                async (Guid supportTicketId, int? page, int? pageSize, ISender sender, CancellationToken cancellationToken) =>
                {
                    var query = new GetSupportTicketMessagesQuery(supportTicketId)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("GetSupportTicketMessages")
            .WithTags("Support");
    }
}
