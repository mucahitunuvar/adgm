using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.CreateSupportTicket;

// CandidateCvId/CompanyId/OpenedByRole istemciden alınmaz - hepsi handler'da çağıranın kendi
// UserId'sinden (AGENTS.md §26: istemciden gelen bir id'ye asla güvenilmez). Priority,
// RegisterUserCommand.Role ile aynı sebeple string (bkz. AddCandidateNoteCommand'daki NoteType notu).
public sealed record CreateSupportTicketCommand(string Subject, string Priority) : IRequest<Result<CreateSupportTicketResponse>>;
