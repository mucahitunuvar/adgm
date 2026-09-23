using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.AddSupportTicketMessage;

// SenderRole istemciden alınmaz - handler, çağıranın ticket.OpenedByUserId/AssignedToUserId ile
// eşleşmesinden ve (atanmışsa) ICareerAdvisorModuleContract'tan güvenilir şekilde çözer.
public sealed record AddSupportTicketMessageCommand(Guid SupportTicketId, string Content)
    : IRequest<Result<AddSupportTicketMessageResponse>>;
