using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetCandidateNotes;

public sealed record GetCandidateNotesQuery(Guid CandidateCvId) : PagedRequest, IRequest<Result<GetCandidateNotesResponse>>;
