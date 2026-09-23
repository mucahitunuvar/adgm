using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTickets;

public sealed record GetSupportTicketsQuery : PagedRequest, IRequest<Result<GetSupportTicketsResponse>>;
