using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.CloseSupportTicket;

public sealed record CloseSupportTicketCommand(Guid SupportTicketId, string? Reason) : IRequest<Result>;
