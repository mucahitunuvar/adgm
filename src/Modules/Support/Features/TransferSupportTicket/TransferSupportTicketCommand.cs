using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.TransferSupportTicket;

public sealed record TransferSupportTicketCommand(Guid SupportTicketId, Guid NewAssigneeUserId) : IRequest<Result>;
