using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.ChangeSupportTicketPriority;

public sealed record ChangeSupportTicketPriorityCommand(Guid SupportTicketId, string Priority) : IRequest<Result>;
