using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicEvents;

public sealed record GetPublicEventsQuery(string? TypeKey, string? When, DateTime? From, DateTime? To, string? Format, string? Lang)
    : PagedRequest, IRequest<Result<PagedResult<PublicEventListItemResponse>>>;
