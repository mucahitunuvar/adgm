using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTicketMessages;

public sealed record GetSupportTicketMessagesQuery(Guid SupportTicketId)
    : PagedRequest, IRequest<Result<GetSupportTicketMessagesResponse>>;
